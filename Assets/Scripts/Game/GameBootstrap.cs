using System.Collections;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using Featherguard.Core;
using Featherguard.Feedback;
using Featherguard.Meta;
using Featherguard.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Featherguard.Game
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] Camera battleCamera;
        [SerializeField] CameraShake cameraShake;

        FeedbackHub feedback;
        BattleHud hud;
        BattleVisuals visuals;
        BattleController battle;
        ToonMaterials toon;

        IEnumerator Start()
        {
            Time.timeScale = 1f;
            feedback = gameObject.AddComponent<FeedbackHub>();
            feedback.Init(config.sounds, battleCamera, cameraShake, config.particleMaterial, config.feedback);
            Ui.Sprites = config.ui;
            DebugTools.Install(config);
            hud = gameObject.AddComponent<BattleHud>();
            visuals = new BattleVisuals(config.presentation);

            var world = SavedGame.World(config);
            if (world.Chosen == null)
            {
                SceneManager.LoadScene(GameScenes.WorldMap);
                yield break;
            }
            yield return FightActivity(world, world.Chosen);
        }

        void Update()
        {
            if (!GameClock.Paused) battle?.Tick();
        }

        void OnDestroy()
        {
            DebugTools.Track(null);
            toon?.Dispose();
        }

        IEnumerator FightActivity(WorldProgress world, ActivityDefinition activity)
        {
            var flow = new ActivityFlow(hud, world, activity, config.presentation.afterVictory);
            hud.Init(battleCamera, visuals, flow.Abandon, () => true);
            var roster = SavedGame.Roster(config);
            var party = roster.IsPartyComplete ? roster.Party : config.heroes.Take(HeroRoster.PartySize).ToList();
            var setup = ActivityBattle.CreateSetup(activity, party);
            StartBattle(setup);
            yield return battle.Run();
            yield return flow.Conclude(battle.Result);
        }

        void StartBattle(BattleSetup setup)
        {
            setup.AwaitBlocks = config.presentation.timingPrompts;
            var model = new BattleModel(setup, config.rules, UnityRandomSource.Shared);
            toon ??= new ToonMaterials(config.toonMaterial);
            battle = new BattleController(model, new BattleServices
            {
                Config = config.presentation,
                Visuals = visuals,
                Factory = new UnitViewFactory(config.presentation, config.flashMaterial, config.ringMaterial, visuals.Units, toon),
                View = hud,
                TimingCues = hud,
                TimingInput = new PointerTimingInput(),
                Random = UnityRandomSource.Shared,
                Feedback = feedback,
                Host = this,
                UnitPicker = new PointerUnitPicker(battleCamera, config.presentation.tapTolerance),
            });
            hud.BindBattle(battle.Commands, battle.Presenter.Rage);
            DebugTools.Track(battle);
        }
    }
}

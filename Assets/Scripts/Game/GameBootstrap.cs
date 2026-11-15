using System.Collections;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using Featherguard.Core;
using Featherguard.Feedback;
using Featherguard.Meta;
using Featherguard.Run;
using Featherguard.UI;
using UnityEngine;

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

        MetaProgression Meta => SavedGame.Meta(config);

        IEnumerator Start()
        {
            Time.timeScale = 1f;
            feedback = gameObject.AddComponent<FeedbackHub>();
            feedback.Init(config.sounds, battleCamera, cameraShake, config.particleMaterial, config.feedback);
            Ui.Sprites = config.ui;
            hud = gameObject.AddComponent<BattleHud>();
            visuals = new BattleVisuals(config.presentation);

            var world = SavedGame.World(config);
            if (world.Chosen != null)
                yield return FightActivity(world, world.Chosen);
            else
                yield return PlayPrototypeRun();
        }

        void Update()
        {
            if (!GameClock.Paused) battle?.Tick();
        }

        void OnDestroy() => toon?.Dispose();

        IEnumerator FightActivity(WorldProgress world, ActivityDefinition activity)
        {
            var flow = new ActivityFlow(hud, world, activity, config.presentation.afterVictory);
            hud.Init(battleCamera, visuals, flow.Abandon, () => true);
            hud.BindWallet(new Wallet());
            var roster = SavedGame.Roster(config);
            var party = roster.IsPartyComplete ? roster.Party : config.heroes.Take(HeroRoster.PartySize).ToList();
            var setup = ActivityBattle.CreateSetup(activity, party);
            StartBattle(setup, UnityRandomSource.Shared);
            yield return battle.Run();
            yield return flow.Conclude(battle.Result);
        }

        IEnumerator PlayPrototypeRun()
        {
            var flow = new RunFlow(hud, Meta, config.presentation.afterVictory);
            hud.Init(battleCamera, visuals, flow.Abandon, () => RunSession.Current != null);

            var session = RunSession.Current;
            if (session == null)
            {
                yield return CampScreen.Show(hud, Meta);
                var roster = SavedGame.Roster(config);
                session = RunSession.Start(config.run, roster.IsPartyComplete ? roster.Party : config.heroes, UnityRandomSource.Shared);
                Meta.ApplyTo(session);
            }
            hud.BindWallet(session.Wallet);

            var setup = session.CreateBattleSetup();
            var testEncounter = TestEncounterSetting.Current;
            if (testEncounter != null)
                UseTestEncounter(setup, testEncounter);
            var model = StartBattle(setup, session.Random);
            flow.TrackKills(session, model, battle.Presenter);
            yield return battle.Run();
            yield return flow.Conclude(session, battle.Result);
        }

        BattleModel StartBattle(BattleSetup setup, IRandomSource random)
        {
            setup.AwaitBlocks = config.presentation.timingPrompts;
            var model = new BattleModel(setup, config.rules, random);
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
            return model;
        }

        static void UseTestEncounter(BattleSetup setup, EncounterDefinition encounter)
        {
            setup.Encounter = encounter;
            setup.EnemyHealthScale = encounter.enemyHealth;
            setup.EnemyDamageScale = encounter.enemyDamage;
            setup.Elite = false;
        }
    }
}

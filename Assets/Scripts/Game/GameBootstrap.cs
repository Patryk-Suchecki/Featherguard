using System.Collections;
using System.Collections.Generic;
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
            var testEncounter = TestEncounterSetting.Current;
            if (world.Chosen != null)
                yield return FightActivity(world, world.Chosen, testEncounter);
            else if (testEncounter != null)
                yield return FightTestEncounter(testEncounter);
            else
                Load(GameScenes.WorldMap);
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

        IEnumerator FightActivity(WorldProgress world, ActivityDefinition activity, EncounterDefinition testEncounter)
        {
            var flow = new ActivityFlow(hud, world, activity, config.presentation.afterVictory, SavedGame.Storage(config), config.materials, UnityRandomSource.Shared, SavedGame.Recipes(config));
            hud.Init(battleCamera, visuals, flow.Abandon, () => true);
            var setup = testEncounter != null ? ActivityBattle.CreateSetup(testEncounter, Party()) : ActivityBattle.CreateSetup(activity, Party());
            setup.Potions = SavedGame.Loadout(config).Picked.Select(p => new BattlePotion(p.Potion, p.Count)).ToList();
            StartBattle(setup);
            yield return battle.Run();
            yield return flow.Conclude(battle.Result);
        }

        IEnumerator FightTestEncounter(EncounterDefinition encounter)
        {
            hud.Init(battleCamera, visuals, () => Load(GameScenes.WorldMap), () => true);
            StartBattle(ActivityBattle.CreateSetup(encounter, Party()));
            yield return battle.Run();
            yield return new WaitForSeconds(config.presentation.afterVictory);
            if (battle.Result.Victory)
                ActivityResultScreen.ShowVictory(hud, encounter.displayName, "Test encounter", null, null, () => Load(GameScenes.WorldMap));
            else
                ActivityResultScreen.ShowDefeat(hud, encounter.displayName, "test", () => Load(GameScenes.Battle), () => Load(GameScenes.WorldMap));
        }

        IReadOnlyList<UnitDefinition> Party()
        {
            var roster = SavedGame.Roster(config);
            return roster.IsPartyComplete ? roster.Party : config.heroes.Take(HeroRoster.PartySize).ToList();
        }

        void StartBattle(BattleSetup setup)
        {
            setup.AwaitBlocks = config.presentation.timingPrompts;
            var model = new BattleModel(setup, config.rules, UnityRandomSource.Shared);
            var bag = SavedGame.Potions(config);
            model.PotionSpent += potion => bag.Remove(potion);
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
            hud.BindPotions(battle.Potions, model.Potions);
            DebugTools.Track(battle);
        }

        static void Load(string scene)
        {
            GameClock.Paused = false;
            SceneManager.LoadScene(scene);
        }
    }
}

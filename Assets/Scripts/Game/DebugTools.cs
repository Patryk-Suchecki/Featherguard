using System.Linq;
using Featherguard.Battle.Presentation;
using Featherguard.Core;
using Featherguard.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Featherguard.Game
{
    public static class DebugTools
    {
        static readonly int[] ItemAmounts = { 1, 5, 10 };

        static DebugMenu menu;
        static BattleController battle;

        public static void Install(GameConfig config)
        {
            if (menu != null || !Debug.isDebugBuild) return;
            menu = DebugMenu.Create(new[]
            {
                new DebugAction("Win battle", () => battle.End(true), available: InBattle),
                new DebugAction("Add materials", AddMaterials(config)),
                new DebugAction("Wipe storage", () => SavedGame.Storage(config).Clear(), confirm: true, keepOpen: true),
                new DebugAction("Add potions", AddPotions(config)),
                new DebugAction("Learn a schematic", LearnSchematics(config)),
                new DebugAction("Learn all schematics", () => SavedGame.Recipes(config).LearnAll(), confirm: true, keepOpen: true),
                new DebugAction("Reset schematics", () => SavedGame.Recipes(config).ForgetAll(), confirm: true, keepOpen: true),
                new DebugAction("Empty potion bag", () => SavedGame.Potions(config).Clear(), confirm: true, keepOpen: true),
                new DebugAction("Reroll side activities", () => RerollSideActivities(config)),
                new DebugAction("Reset all progress", () => ResetProgress(config), confirm: true),
            });
        }

        static DebugAction[] AddMaterials(GameConfig config) =>
            SavedGame.Storage(config).All
                .Select(m => new DebugAction(m.displayName, ItemAmounts, amount => SavedGame.Storage(config).Add(m, amount), Ui.Sprite(m.name)))
                .ToArray();

        static DebugAction[] AddPotions(GameConfig config) =>
            SavedGame.Potions(config).All
                .Select(p => new DebugAction(p.displayName, ItemAmounts, amount => SavedGame.Potions(config).Add(p, amount), Ui.Sprite(p.name)))
                .ToArray();

        static DebugAction[] LearnSchematics(GameConfig config) =>
            SavedGame.Recipes(config).All
                .Where(r => !r.knownFromStart && r.potion != null)
                .Select(r => new DebugAction(r.potion.displayName, () => SavedGame.Recipes(config).Learn(r), available: () => !SavedGame.Recipes(config).Knows(r), keepOpen: true, icon: Ui.Sprite(r.potion.name)))
                .ToArray();

        public static void Track(BattleController controller) => battle = controller;

        static bool InBattle() => battle != null && !battle.Model.IsFinished;

        static void ResetProgress(GameConfig config)
        {
            SavedGame.ResetProgress(config);
            Load(GameScenes.Stronghold);
        }

        static void RerollSideActivities(GameConfig config)
        {
            SavedGame.World(config).Reroll();
            if (SceneManager.GetActiveScene().name == GameScenes.WorldMap) Load(GameScenes.WorldMap);
        }

        static void Load(string scene)
        {
            GameClock.Paused = false;
            SceneManager.LoadScene(scene);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLoad() => battle = null;
    }
}

using Featherguard.Battle.Presentation;
using Featherguard.Core;
using Featherguard.Run;
using Featherguard.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Featherguard.Game
{
    public static class DebugTools
    {
        static DebugMenu menu;
        static BattleController battle;

        public static void Install(GameConfig config)
        {
            if (menu != null || !Debug.isDebugBuild) return;
            menu = DebugMenu.Create(new[]
            {
                new DebugAction("Win battle", () => battle.End(true), available: InBattle),
                new DebugAction("Reroll side activities", () => RerollSideActivities(config)),
                new DebugAction("Reset all progress", () => ResetProgress(config), confirm: true),
            });
        }

        public static void Track(BattleController controller) => battle = controller;

        static bool InBattle() => battle != null && !battle.Model.IsFinished;

        static void ResetProgress(GameConfig config)
        {
            SavedGame.ResetProgress(config);
            RunSession.End();
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

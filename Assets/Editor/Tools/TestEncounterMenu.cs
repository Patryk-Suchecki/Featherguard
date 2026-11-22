using Featherguard.Battle;
using Featherguard.Game;
using UnityEditor;

namespace Featherguard.EditorTools
{
    public static class TestEncounterMenu
    {
        const string Root = "Featherguard/Test Encounter/";

        [MenuItem(Root + "Use Selected")]
        static void UseSelected()
        {
            var encounter = (EncounterDefinition)Selection.activeObject;
            TestEncounterSetting.Set(encounter);
            EditorUtility.DisplayDialog("Featherguard", $"Every battle in the editor now uses {encounter.name}, also when you press Play in the Battle scene.", "OK");
        }

        [MenuItem(Root + "Use Selected", true)]
        static bool CanUseSelected() => Selection.activeObject is EncounterDefinition;

        [MenuItem(Root + "Clear")]
        static void Clear()
        {
            TestEncounterSetting.Clear();
            EditorUtility.DisplayDialog("Featherguard", "Battles in the editor use their activity's encounter again.", "OK");
        }

        [MenuItem(Root + "Show Current")]
        static void ShowCurrent()
        {
            var encounter = TestEncounterSetting.Current;
            EditorUtility.DisplayDialog("Featherguard", encounter != null ? $"Test encounter: {encounter.name}." : "No test encounter: battles use their activity's encounter.", "OK");
        }
    }
}

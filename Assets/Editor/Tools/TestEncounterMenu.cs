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
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            EditorPrefs.SetString(GameBootstrap.TestEncounterPref, path);
            EditorUtility.DisplayDialog("Featherguard", $"Every battle in the editor now uses {Selection.activeObject.name}.", "OK");
        }

        [MenuItem(Root + "Use Selected", true)]
        static bool CanUseSelected() => Selection.activeObject is EncounterDefinition;

        [MenuItem(Root + "Clear")]
        static void Clear()
        {
            EditorPrefs.DeleteKey(GameBootstrap.TestEncounterPref);
            EditorUtility.DisplayDialog("Featherguard", "Battles in the editor use the run's encounters again.", "OK");
        }

        [MenuItem(Root + "Show Current")]
        static void ShowCurrent()
        {
            string path = EditorPrefs.GetString(GameBootstrap.TestEncounterPref, "");
            var encounter = path.Length > 0 ? AssetDatabase.LoadAssetAtPath<EncounterDefinition>(path) : null;
            EditorUtility.DisplayDialog("Featherguard", encounter != null ? $"Test encounter: {encounter.name}." : "No test encounter: battles use the run.", "OK");
        }
    }
}

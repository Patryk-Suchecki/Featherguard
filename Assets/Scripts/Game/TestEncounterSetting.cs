using Featherguard.Battle;
using UnityEngine;

namespace Featherguard.Game
{
    public static class TestEncounterSetting
    {
        static string Key => $"Featherguard.TestEncounter.{Application.dataPath}";

#if UNITY_EDITOR
        public static EncounterDefinition Current
        {
            get
            {
                string path = UnityEditor.EditorPrefs.GetString(Key, "");
                return path.Length > 0 ? UnityEditor.AssetDatabase.LoadAssetAtPath<EncounterDefinition>(path) : null;
            }
        }

        public static void Set(EncounterDefinition encounter) => UnityEditor.EditorPrefs.SetString(Key, UnityEditor.AssetDatabase.GetAssetPath(encounter));

        public static void Clear() => UnityEditor.EditorPrefs.DeleteKey(Key);
#else
        public static EncounterDefinition Current => null;
#endif
    }
}

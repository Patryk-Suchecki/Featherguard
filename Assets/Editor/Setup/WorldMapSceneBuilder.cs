using Featherguard.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class WorldMapSceneBuilder
    {
        [MenuItem("Featherguard/Rebuild World Map Scene")]
        public static void Run()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>($"{ContentPaths.Data}/GameConfig.asset");
            if (config == null)
            {
                Debug.LogError("Featherguard: GameConfig.asset is missing. Run Featherguard > Rebuild Battle Scene first.");
                return;
            }
            config.world = WorldCatalog.Build(new Seeder(false));
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Build(config);
        }

        public static void Build(GameConfig config)
        {
            string configPath = AssetDatabase.GetAssetPath(config);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            config = AssetDatabase.LoadAssetAtPath<GameConfig>(configPath);
            var camera = NightScene.CreateCamera(Vector3.zero, Vector3.forward, 45f);

            var bootstrap = new GameObject("Game").AddComponent<WorldMapBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.FindProperty("sceneCamera").objectReferenceValue = camera;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ContentPaths.WorldMapScene);
            BuildScenes.Register();
            AssetDatabase.SaveAssets();
        }
    }
}

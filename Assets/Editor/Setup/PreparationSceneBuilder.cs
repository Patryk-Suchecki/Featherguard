using System.Linq;
using Featherguard.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public sealed class PreparationSceneBuilder
    {
        static readonly Vector3[] HeroSpots = { new(-2.6f, 0f, 0f), new(-3.8f, 0f, 1.4f), new(-3.8f, 0f, -1.4f) };
        static readonly Vector3[] EnemySpots = { new(1.8f, 0f, 0f), new(3f, 0f, 1.5f), new(3f, 0f, -1.5f), new(4.2f, 0f, 0.6f), new(4.2f, 0f, -0.9f) };

        NightPalette palette;

        [MenuItem("Featherguard/Rebuild Preparation Scene")]
        public static void Run()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>($"{ContentPaths.Data}/GameConfig.asset");
            if (config == null)
            {
                Debug.LogError("Featherguard: GameConfig.asset is missing. Run Featherguard > Rebuild Battle Scene first.");
                return;
            }
            Build(config);
        }

        public static void Build(GameConfig config) => new PreparationSceneBuilder().Create(config);

        PreparationSceneBuilder() { }

        void Create(GameConfig config)
        {
            string configPath = AssetDatabase.GetAssetPath(config);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            config = AssetDatabase.LoadAssetAtPath<GameConfig>(configPath);
            palette = new NightPalette();
            NightScene.PostFX(ContentPaths.PreparationPostProfile);
            NightScene.ConfigureLighting();
            var camera = NightScene.CreateCamera(new Vector3(-5.2f, 4.2f, -10.5f), new Vector3(-3.7f, 1.1f, 0.5f), 40f);

            var world = new GameObject("Field").transform;
            NightScene.Ground(world, palette.Ground, 4);
            Road(world);
            Dressing(world);
            var heroes = Spots(world, "Hero Spots", HeroSpots);
            var enemies = Spots(world, "Enemy Spots", EnemySpots);

            NightScene.Lamp(world, "Party Lantern", new Vector3(-4.6f, 1.6f, -2.6f), NightScene.WarmLight, 14f, 9f, palette.Lantern, 0.15f, scale: Vector3.one * 0.25f);
            NightScene.Decoration(PrimitiveType.Cylinder, "Lantern Pole", world, new Vector3(-4.6f, 0.75f, -2.6f), new Vector3(0.08f, 0.75f, 0.08f), palette.WoodDark);
            NightScene.Lamp(world, "Enemy Glow", new Vector3(4.2f, 2.6f, 2.4f), new Color(0.75f, 0.35f, 0.9f), 12f, 10f, null, 0.25f);
            NightScene.Lamp(world, "Wisp", new Vector3(1.2f, 1.8f, 3.2f), NightScene.SpiritLight, 6f, 7f, palette.Spirit, 0.5f, scale: Vector3.one * 0.18f);

            var bootstrap = new GameObject("Game").AddComponent<PreparationBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.FindProperty("sceneCamera").objectReferenceValue = camera;
            SerializedArray.Set(serialized.FindProperty("heroSpots"), heroes);
            SerializedArray.Set(serialized.FindProperty("enemySpots"), enemies);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ContentPaths.PreparationScene);
            BuildScenes.Register();
            AssetDatabase.SaveAssets();
        }

        void Road(Transform world)
        {
            NightScene.Decoration(PrimitiveType.Cube, "Road", world, new Vector3(0f, 0.01f, 0f), new Vector3(30f, 0.02f, 2.6f), palette.Road);
            for (int i = 0; i < 6; i++)
                NightScene.Decoration(PrimitiveType.Cube, "Stone", world, new Vector3(-6f + i * 2.4f, 0.08f, i % 2 == 0 ? 1.5f : -1.6f), new Vector3(0.35f, 0.16f, 0.3f), palette.StoneDark, new Vector3(0f, i * 37f, 0f));
        }

        void Dressing(Transform world)
        {
            NightScene.DeadTree(world, new Vector3(-7.5f, 0f, 3.5f), 20f, 1.6f, palette.Bark);
            NightScene.DeadTree(world, new Vector3(1.5f, 0f, 5f), 140f, 1.9f, palette.Bark);
            NightScene.DeadTree(world, new Vector3(7.5f, 0f, 4f), 75f, 1.5f, palette.Bark);
            NightScene.DeadTree(world, new Vector3(9f, 0f, -3f), 210f, 1.3f, palette.Bark);
            for (int i = 0; i < 4; i++)
                NightScene.Decoration(PrimitiveType.Cube, "Grave", world, new Vector3(6.5f + i * 0.9f, 0.4f, 3f - i * 0.4f), new Vector3(0.45f, 0.8f, 0.12f), palette.Ash, new Vector3(0f, -20f, (i % 3 - 1) * 8f));
            NightScene.Decoration(PrimitiveType.Cube, "Signpost", world, new Vector3(-0.6f, 0.9f, -2.2f), new Vector3(0.12f, 1.8f, 0.12f), palette.WoodDark);
            NightScene.Decoration(PrimitiveType.Cube, "Sign", world, new Vector3(-0.3f, 1.55f, -2.2f), new Vector3(0.8f, 0.25f, 0.06f), palette.Wood, new Vector3(0f, 0f, -6f));
        }

        static Transform[] Spots(Transform world, string name, Vector3[] positions)
        {
            var group = new GameObject(name).transform;
            group.SetParent(world, false);
            return positions.Select((position, i) =>
            {
                var spot = new GameObject($"Spot {i + 1}").transform;
                spot.SetParent(group, false);
                spot.localPosition = position;
                return spot;
            }).ToArray();
        }
    }
}

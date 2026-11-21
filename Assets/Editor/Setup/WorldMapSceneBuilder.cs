using Featherguard.Game;
using Featherguard.Meta;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public sealed class WorldMapSceneBuilder
    {
        static readonly Vector3[] LandSpots = { new(-11f, 0f, -1f), new(11f, 0f, -1f), new(-11f, 0f, 8f), new(11f, 0f, 8f), new(0f, 0f, 16.5f) };
        static readonly Vector3 StrongholdSpot = new(0f, 0f, 3.5f);
        static readonly Vector3 AreaSize = new(8.5f, 0.02f, 7f);

        NightPalette palette;

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

        public static void Build(GameConfig config) => new WorldMapSceneBuilder().Create(config);

        WorldMapSceneBuilder() { }

        void Create(GameConfig config)
        {
            string configPath = AssetDatabase.GetAssetPath(config);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            config = AssetDatabase.LoadAssetAtPath<GameConfig>(configPath);
            palette = new NightPalette();
            NightScene.PostFX(ContentPaths.WorldMapPostProfile);
            NightScene.ConfigureLighting().intensity = 4f;
            var camera = NightScene.CreateCamera(new Vector3(0f, 30f, -5f), new Vector3(0f, 0f, 6.5f), 45f);

            var map = new GameObject("Map").transform;
            NightScene.Ground(map, palette.Ground);
            River(map);
            Stronghold(map);
            var lands = config.world.lands;
            var built = new[]
            {
                Land(map, lands[0], LandSpots[0], palette.Rye, RyeFields),
                Land(map, lands[1], LandSpots[1], palette.Bog, Marshes),
                Land(map, lands[2], LandSpots[2], palette.StoneDark, BaldMountain),
                Land(map, lands[3], LandSpots[3], palette.Withered, OldForest),
                Land(map, lands[4], LandSpots[4], palette.Ash, Nav),
            };
            var bootstrap = new GameObject("Game").AddComponent<WorldMapBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.FindProperty("sceneCamera").objectReferenceValue = camera;
            SerializedArray.Set(serialized.FindProperty("lands"), built);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ContentPaths.WorldMapScene);
            BuildScenes.Register();
            AssetDatabase.SaveAssets();
        }

        WorldMapLand Land(Transform map, LandDefinition definition, Vector3 position, Material ground, System.Action<Transform> props)
        {
            var root = new GameObject(definition.displayName).transform;
            root.SetParent(map, false);
            root.position = position;
            NightScene.Decoration(PrimitiveType.Cylinder, "Area", root, new Vector3(0f, 0.02f, 0f), AreaSize, ground);
            props(root);
            NightScene.Lamp(root, "Land Light", new Vector3(0f, 3.5f, -0.5f), new Color(0.75f, 0.8f, 1f), 12f, 10f, null, 0f);

            var cover = NightScene.Decoration(PrimitiveType.Sphere, "Locked Mist", root, new Vector3(0f, 0f, 0f), new Vector3(9.2f, 3.5f, 7.8f), palette.Mist);

            var label = Anchor(root, "Label", new Vector3(0f, 0.5f, 3.4f));
            var buttons = Anchor(root, "Activities", new Vector3(0f, 0.5f, 0.2f));
            var land = new SerializedObject(root.gameObject.AddComponent<WorldMapLand>());
            land.FindProperty("land").objectReferenceValue = definition;
            land.FindProperty("activitiesAnchor").objectReferenceValue = buttons;
            land.FindProperty("labelAnchor").objectReferenceValue = label;
            land.FindProperty("lockedCover").objectReferenceValue = cover;
            land.ApplyModifiedPropertiesWithoutUndo();
            return root.GetComponent<WorldMapLand>();
        }

        static Transform Anchor(Transform root, string name, Vector3 position)
        {
            var anchor = new GameObject(name).transform;
            anchor.SetParent(root, false);
            anchor.localPosition = position;
            return anchor;
        }

        void RyeFields(Transform land)
        {
            for (int i = 0; i < 28; i++)
            {
                var spot = new Vector3((i % 7 - 3) * 0.75f, 0.35f, (i / 7 - 1.5f) * 0.9f);
                NightScene.Decoration(PrimitiveType.Cylinder, "Rye", land, spot, new Vector3(0.08f, 0.35f + i % 3 * 0.06f, 0.08f), palette.Straw);
            }
            NightScene.Decoration(PrimitiveType.Cube, "Scarecrow", land, new Vector3(2.6f, 0.9f, 1.4f), new Vector3(0.1f, 1.8f, 0.1f), palette.WoodDark);
            NightScene.Decoration(PrimitiveType.Cube, "Scarecrow Arms", land, new Vector3(2.6f, 1.4f, 1.4f), new Vector3(1.1f, 0.1f, 0.1f), palette.WoodDark);
            NightScene.Lamp(land, "Lantern", new Vector3(-2.8f, 1.2f, 1.6f), NightScene.WarmLight, 8f, 8f, palette.Lantern, 0.15f, scale: Vector3.one * 0.25f);
        }

        void Marshes(Transform land)
        {
            NightScene.Decoration(PrimitiveType.Cylinder, "Bog Water", land, new Vector3(0.5f, 0.05f, 0.3f), new Vector3(4.2f, 0.02f, 3.2f), palette.Water);
            for (int i = 0; i < 10; i++)
                NightScene.Decoration(PrimitiveType.Cylinder, "Reed", land, new Vector3(-2.6f + i * 0.55f, 0.45f, i % 2 == 0 ? 1.9f : -1.8f), new Vector3(0.05f, 0.45f, 0.05f), palette.Withered);
            NightScene.Lamp(land, "Wisp", new Vector3(0.8f, 1.4f, 0.4f), NightScene.SpiritLight, 8f, 9f, palette.Spirit, 0.5f, scale: Vector3.one * 0.22f);
        }

        void BaldMountain(Transform land)
        {
            NightScene.Decoration(PrimitiveType.Cube, "Peak", land, new Vector3(0f, 1.6f, 1.2f), new Vector3(2.6f, 2.6f, 2.6f), palette.StoneDark, new Vector3(45f, 0f, 45f));
            NightScene.Decoration(PrimitiveType.Cube, "Peak", land, new Vector3(-2.2f, 1f, 0.6f), new Vector3(1.6f, 1.6f, 1.6f), palette.Stone, new Vector3(45f, 20f, 45f));
            NightScene.Lamp(land, "Dragon Fire", new Vector3(0f, 3.4f, 1.2f), new Color(1f, 0.35f, 0.1f), 10f, 9f, palette.Ember, 0.4f, scale: Vector3.one * 0.3f);
        }

        void OldForest(Transform land)
        {
            for (int i = 0; i < 9; i++)
                NightScene.DeadTree(land, land.position + new Vector3((i % 3 - 1) * 2.2f + (i / 3 - 1) * 0.4f, 0f, (i / 3 - 1) * 1.6f + 0.6f), i * 41f, 1.1f + i % 3 * 0.2f, palette.Bark);
            NightScene.Lamp(land, "Witch Light", new Vector3(0f, 1.3f, -0.4f), new Color(0.45f, 1f, 0.4f), 8f, 8f, palette.WitchGlow, 0.3f, scale: Vector3.one * 0.22f);
        }

        void Nav(Transform land)
        {
            for (int i = 0; i < 7; i++)
            {
                var spot = new Vector3((i - 3) * 0.95f, 0.4f, (i % 2) * 0.9f);
                NightScene.Decoration(PrimitiveType.Cube, "Grave", land, spot, new Vector3(0.45f, 0.8f, 0.12f), palette.Ash, new Vector3(0f, 0f, (i % 3 - 1) * 8f));
            }
            NightScene.Lamp(land, "Spirit", new Vector3(0f, 1.6f, 0.8f), NightScene.SpiritLight, 10f, 10f, palette.Spirit, 0.5f, scale: Vector3.one * 0.3f);
        }

        void River(Transform map)
        {
            NightScene.Decoration(PrimitiveType.Cube, "Smorodina River", map, new Vector3(0f, 0.03f, 12.6f), new Vector3(60f, 0.02f, 1.6f), palette.Water);
            NightScene.Decoration(PrimitiveType.Cube, "Kalinov Bridge", map, new Vector3(0f, 0.12f, 12.6f), new Vector3(1.4f, 0.12f, 2.4f), palette.Wood);
        }

        void Stronghold(Transform map)
        {
            var stronghold = new GameObject("Stronghold").transform;
            stronghold.SetParent(map, false);
            stronghold.localPosition = StrongholdSpot;
            foreach (var land in LandSpots)
            {
                var road = land - StrongholdSpot;
                NightScene.Decoration(PrimitiveType.Cube, "Road", map, StrongholdSpot + road * 0.5f + Vector3.up * 0.02f,
                    new Vector3(0.5f, 0.02f, road.magnitude), palette.Road, new Vector3(0f, Mathf.Atan2(road.x, road.z) * Mathf.Rad2Deg, 0f));
            }
            NightScene.Decoration(PrimitiveType.Cylinder, "Palisade", stronghold, new Vector3(0f, 0.4f, 0f), new Vector3(3.2f, 0.4f, 3.2f), palette.WoodDark);
            NightScene.Decoration(PrimitiveType.Cube, "Hall", stronghold, new Vector3(0f, 1f, 0f), new Vector3(1.4f, 1.2f, 1f), palette.Wood);
            NightScene.Decoration(PrimitiveType.Cube, "Roof", stronghold, new Vector3(0f, 1.8f, 0f), new Vector3(1.1f, 1.1f, 1.1f), palette.RoofDark, new Vector3(0f, 0f, 45f));
            NightScene.Lamp(stronghold, "Hearth", new Vector3(0f, 1.2f, -1f), NightScene.WarmLight, 10f, 7f, palette.Flame, 0.2f, scale: Vector3.one * 0.3f);
        }
    }
}

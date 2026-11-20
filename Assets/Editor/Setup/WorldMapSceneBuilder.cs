using System.Linq;
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
        static readonly Vector3[] SlotOffsets = { new(-2.8f, 0.3f, -1f), new(2.8f, 0.3f, -1f), new(0f, 0.3f, 1.9f) };

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
            var templates = new GameObject("Marker Templates").transform;
            var markers = new[]
            {
                SingleFightMarker(templates), WavesFightMarker(templates), PuzzleFightMarker(templates), EliteFightMarker(templates), BossFightMarker(templates),
            };
            var mainQuest = MainQuestMarker(templates);

            var bootstrap = new GameObject("Game").AddComponent<WorldMapBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.FindProperty("sceneCamera").objectReferenceValue = camera;
            SetArray(serialized.FindProperty("lands"), built);
            SetArray(serialized.FindProperty("markerTemplates"), markers);
            serialized.FindProperty("mainQuestTemplate").objectReferenceValue = mainQuest;
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

            var slots = SlotOffsets.Select((offset, i) =>
            {
                var slot = new GameObject($"Slot {i + 1}").transform;
                slot.SetParent(root, false);
                slot.localPosition = offset;
                return slot;
            }).ToArray();
            var label = new GameObject("Label").transform;
            label.SetParent(root, false);
            label.localPosition = new Vector3(0f, 0.5f, -3.9f);

            var area = root.gameObject.AddComponent<BoxCollider>();
            area.center = new Vector3(0f, 0.1f, 0f);
            area.size = new Vector3(AreaSize.x, 0.2f, AreaSize.z);
            var land = new SerializedObject(root.gameObject.AddComponent<WorldMapLand>());
            land.FindProperty("land").objectReferenceValue = definition;
            SetArray(land.FindProperty("slots"), slots);
            land.FindProperty("labelAnchor").objectReferenceValue = label;
            land.FindProperty("lockedCover").objectReferenceValue = cover;
            land.ApplyModifiedPropertiesWithoutUndo();
            return root.GetComponent<WorldMapLand>();
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

        GameObject SingleFightMarker(Transform templates)
        {
            var marker = Template(templates, "Single Fight");
            NightScene.Part(PrimitiveType.Cylinder, "Token", marker.transform, Vector3.zero, new Vector3(0.9f, 0.08f, 0.9f), palette.Stone);
            NightScene.Part(PrimitiveType.Cube, "Blade", marker.transform, new Vector3(0f, 0.6f, 0f), new Vector3(0.12f, 1.1f, 0.05f), palette.Bone);
            return Finish(marker);
        }

        GameObject WavesFightMarker(Transform templates)
        {
            var marker = Template(templates, "Waves Fight");
            for (int i = 0; i < 3; i++)
                NightScene.Part(PrimitiveType.Cylinder, "Token", marker.transform, new Vector3(0f, i * 0.2f, 0f), new Vector3(0.9f - i * 0.15f, 0.08f, 0.9f - i * 0.15f), i % 2 == 0 ? palette.Iron : palette.Bone);
            return Finish(marker);
        }

        GameObject PuzzleFightMarker(Transform templates)
        {
            var marker = Template(templates, "Puzzle Fight");
            NightScene.Part(PrimitiveType.Cube, "Rune Stone", marker.transform, new Vector3(0f, 0.5f, 0f), Vector3.one * 0.6f, palette.Stone, new Vector3(45f, 0f, 45f));
            NightScene.Part(PrimitiveType.Sphere, "Rune", marker.transform, new Vector3(0f, 0.5f, -0.3f), Vector3.one * 0.18f, palette.Spirit);
            return Finish(marker);
        }

        GameObject EliteFightMarker(Transform templates)
        {
            var marker = Template(templates, "Elite Fight");
            NightScene.Part(PrimitiveType.Cube, "Spike", marker.transform, new Vector3(0f, 0.7f, 0f), new Vector3(0.25f, 1.4f, 0.25f), palette.Iron);
            NightScene.Part(PrimitiveType.Sphere, "Skull", marker.transform, new Vector3(0f, 1.55f, 0f), Vector3.one * 0.4f, palette.Bone);
            return Finish(marker);
        }

        GameObject BossFightMarker(Transform templates)
        {
            var marker = Template(templates, "Boss Fight");
            NightScene.Part(PrimitiveType.Sphere, "Skull", marker.transform, new Vector3(0f, 0.8f, 0f), Vector3.one * 0.8f, palette.Bone);
            NightScene.Part(PrimitiveType.Cylinder, "Horn", marker.transform, new Vector3(-0.45f, 1.3f, 0f), new Vector3(0.12f, 0.35f, 0.12f), palette.Cloth, new Vector3(0f, 0f, 30f));
            NightScene.Part(PrimitiveType.Cylinder, "Horn", marker.transform, new Vector3(0.45f, 1.3f, 0f), new Vector3(0.12f, 0.35f, 0.12f), palette.Cloth, new Vector3(0f, 0f, -30f));
            NightScene.Part(PrimitiveType.Sphere, "Eyes", marker.transform, new Vector3(0f, 0.85f, -0.36f), new Vector3(0.45f, 0.12f, 0.1f), palette.Ember);
            return Finish(marker);
        }

        GameObject MainQuestMarker(Transform templates)
        {
            var marker = Template(templates, "Main Quest");
            NightScene.Part(PrimitiveType.Cylinder, "Pole", marker.transform, new Vector3(0f, 1.2f, 0f), new Vector3(0.1f, 1.2f, 0.1f), palette.WoodDark);
            NightScene.Part(PrimitiveType.Cube, "Banner", marker.transform, new Vector3(0.45f, 1.9f, 0f), new Vector3(0.8f, 0.6f, 0.05f), palette.Lantern);
            NightScene.Lamp(marker.transform, "Glow", new Vector3(0f, 2.4f, 0f), NightScene.WarmLight, 6f, 6f, palette.Lantern, 0.1f, scale: Vector3.one * 0.2f);
            return Finish(marker);
        }

        static GameObject Template(Transform templates, string name)
        {
            var marker = new GameObject(name);
            marker.transform.SetParent(templates, false);
            marker.transform.localScale = Vector3.one * 1.5f;
            return marker;
        }

        static GameObject Finish(GameObject marker)
        {
            NightScene.FitCollider(marker.transform, 1.2f);
            marker.SetActive(false);
            return marker;
        }

        static void SetArray(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}

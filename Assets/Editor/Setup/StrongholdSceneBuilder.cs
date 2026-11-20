using System.Linq;
using Featherguard.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public sealed class StrongholdSceneBuilder
    {
        static readonly Vector3 CampfirePosition = Vector3.zero;
        static readonly Vector3 SignpostPosition = new(8f, 0f, -4.5f);

        const float SeatRadius = 2.6f;
        const float SeatArcStart = 165f;
        const float SeatArcEnd = 15f;

        NightPalette palette;

        [MenuItem("Featherguard/Rebuild Stronghold Scene")]
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

        public static void Build(GameConfig config) => new StrongholdSceneBuilder().Create(config);

        StrongholdSceneBuilder() { }

        void Create(GameConfig config)
        {
            string configPath = AssetDatabase.GetAssetPath(config);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            config = AssetDatabase.LoadAssetAtPath<GameConfig>(configPath);
            palette = new NightPalette();
            NightScene.PostFX(ContentPaths.StrongholdPostProfile);

            NightScene.ConfigureLighting();
            var camera = NightScene.CreateCamera(new Vector3(0f, 18f, -17.5f), new Vector3(0f, 0f, 3.2f), 40f);

            var world = new GameObject("World").transform;
            Ground(world);
            var (campfire, seats) = Campfire(world, config.campSeats);
            Forge(world, new Vector3(-8f, 0f, 3f));
            WiseWomansHut(world, new Vector3(-4f, 0f, 8f));
            SacredOak(world, new Vector3(4.5f, 0f, 9f));
            Tavern(world, new Vector3(8.5f, 0f, 3.5f));
            Shop(world, new Vector3(-8f, 0f, -4f));
            Signpost(world, SignpostPosition);
            Graveyard(world);
            DeadTrees(world);

            var bootstrap = new GameObject("Game").AddComponent<StrongholdBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.FindProperty("sceneCamera").objectReferenceValue = camera;
            serialized.FindProperty("campfire").objectReferenceValue = campfire;
            serialized.FindProperty("campView").objectReferenceValue = CampView(world);
            var seatsProperty = serialized.FindProperty("seats");
            seatsProperty.arraySize = seats.Length;
            for (int i = 0; i < seats.Length; i++)
                seatsProperty.GetArrayElementAtIndex(i).objectReferenceValue = seats[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Seeder.EnsureFolder(System.IO.Path.GetDirectoryName(ContentPaths.StrongholdScene));
            EditorSceneManager.SaveScene(scene, ContentPaths.StrongholdScene);
            BuildScenes.Register();
            AssetDatabase.SaveAssets();
        }

        void Ground(Transform world)
        {
            NightScene.Ground(world, palette.Ground);
            Road(world, CampfirePosition, SignpostPosition);
            Road(world, SignpostPosition, SignpostPosition + new Vector3(8f, 0f, -4.5f));
        }

        void Road(Transform world, Vector3 from, Vector3 to)
        {
            var direction = to - from;
            var road = NightScene.Part(PrimitiveType.Cube, "Road", world, (from + to) * 0.5f + Vector3.up * 0.01f,
                new Vector3(direction.magnitude, 0.02f, 1.4f), palette.Road, new Vector3(0f, -Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg, 0f));
            Object.DestroyImmediate(road.GetComponent<Collider>());
        }

        (Transform, CampSeat[]) Campfire(Transform world, int seatCount)
        {
            var camp = Place(world, "Campfire", StrongholdPlaceKind.Camp, "Camp", CampfirePosition, new Vector3(0f, -0.3f, -1.8f));
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                NightScene.Part(PrimitiveType.Sphere, "Stone", camp, new Vector3(Mathf.Cos(angle) * 0.75f, 0.1f, Mathf.Sin(angle) * 0.75f), new Vector3(0.38f, 0.26f, 0.38f), palette.StoneDark);
            }
            for (int i = 0; i < 3; i++)
                NightScene.Part(PrimitiveType.Cylinder, "Log", camp, new Vector3(0f, 0.16f, 0f), new Vector3(0.16f, 0.55f, 0.16f), palette.WoodDark, new Vector3(90f, i * 60f, 0f));
            var flame = NightScene.Part(PrimitiveType.Sphere, "Flame", camp, new Vector3(0f, 0.55f, 0f), new Vector3(0.5f, 0.9f, 0.5f), palette.Flame).transform;
            NightScene.Lamp(camp, "Fire Light", new Vector3(0f, 1.3f, 0f), new Color(1f, 0.55f, 0.2f), 14f, 16f, null, 0.35f, flame);

            NightScene.FitCollider(camp, 1.6f);
            return (camp, Seats(world, seatCount));
        }

        CampSeat[] Seats(Transform world, int count)
        {
            var party = new GameObject("Seats").transform;
            party.SetParent(world, false);
            return Enumerable.Range(0, count).Select(i =>
            {
                float angle = Mathf.Lerp(SeatArcStart, SeatArcEnd, count > 1 ? i / (count - 1f) : 0.5f) * Mathf.Deg2Rad;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                var seat = new GameObject($"Seat {i + 1}").transform;
                seat.SetParent(party, false);
                seat.position = CampfirePosition + direction * SeatRadius;
                seat.rotation = Quaternion.LookRotation(-direction);
                NightScene.Decoration(PrimitiveType.Cylinder, "Stump", seat, new Vector3(0f, 0.18f, -0.55f), new Vector3(0.45f, 0.18f, 0.45f), palette.WoodDark);
                var collider = seat.gameObject.AddComponent<CapsuleCollider>();
                collider.center = Vector3.up * 0.8f;
                collider.height = 1.7f;
                collider.radius = 0.6f;
                var label = new GameObject("Label").transform;
                label.SetParent(seat, false);
                label.localPosition = Vector3.up * 2.1f;
                var component = new SerializedObject(seat.gameObject.AddComponent<CampSeat>());
                component.FindProperty("index").intValue = i;
                component.FindProperty("labelAnchor").objectReferenceValue = label;
                component.ApplyModifiedPropertiesWithoutUndo();
                return seat.GetComponent<CampSeat>();
            }).ToArray();
        }

        static Transform CampView(Transform world)
        {
            var view = new GameObject("Camp View").transform;
            view.SetParent(world, false);
            view.position = CampfirePosition + new Vector3(0f, 6.5f, -7f);
            view.rotation = Quaternion.LookRotation(CampfirePosition + new Vector3(0f, 0.8f, 1.4f) - view.position);
            return view;
        }

        void Forge(Transform world, Vector3 position)
        {
            var forge = Place(world, "Forge", StrongholdPlaceKind.Forge, "Forge", position, Vector3.up * 4.4f);
            House(forge, 3.2f, 2.2f, 2.6f, palette.Stone, palette.RoofDark, Vector3.zero, palette.Window);
            NightScene.Lamp(forge, "Forge Glow", new Vector3(0f, 0.5f, -1.45f), new Color(1f, 0.38f, 0.12f), 12f, 9f, palette.Ember, 0.3f, scale: new Vector3(0.6f, 0.35f, 0.15f));
            NightScene.Part(PrimitiveType.Cube, "Chimney", forge, new Vector3(0.9f, 3.2f, 0.4f), new Vector3(0.5f, 2.2f, 0.5f), palette.StoneDark);
            NightScene.Part(PrimitiveType.Cube, "Anvil Base", forge, new Vector3(2.1f, 0.25f, -1.2f), new Vector3(0.35f, 0.5f, 0.35f), palette.Iron);
            NightScene.Part(PrimitiveType.Cube, "Anvil", forge, new Vector3(2.1f, 0.6f, -1.2f), new Vector3(0.8f, 0.2f, 0.35f), palette.Iron);
            NightScene.FitCollider(forge);
        }

        void WiseWomansHut(Transform world, Vector3 position)
        {
            var hut = Place(world, "Wise Woman's Hut", StrongholdPlaceKind.WiseWomansHut, "Wise Woman's Hut", position, Vector3.up * 3.8f);
            House(hut, 2.4f, 1.8f, 2.2f, palette.WoodDark, palette.RoofMoss, new Vector3(0f, 7f, 5f), palette.WitchGlow);
            NightScene.Part(PrimitiveType.Cylinder, "Cauldron", hut, new Vector3(1.6f, 0.35f, -1.2f), new Vector3(0.7f, 0.35f, 0.7f), palette.Iron);
            NightScene.Lamp(hut, "Cauldron Glow", new Vector3(1.6f, 0.72f, -1.2f), new Color(0.45f, 1f, 0.4f), 9f, 8f, palette.WitchGlow, 0.25f, scale: new Vector3(0.6f, 0.08f, 0.6f));
            NightScene.Part(PrimitiveType.Cylinder, "Pole", hut, new Vector3(-1.6f, 0.8f, -1.1f), new Vector3(0.06f, 0.8f, 0.06f), palette.WoodDark);
            NightScene.Part(PrimitiveType.Sphere, "Skull", hut, new Vector3(-1.6f, 1.7f, -1.1f), Vector3.one * 0.3f, palette.Bone);
            NightScene.FitCollider(hut);
        }

        void SacredOak(Transform world, Vector3 position)
        {
            var oak = Place(world, "Sacred Oak", StrongholdPlaceKind.SacredOak, "Sacred Oak", position, Vector3.up * 5.6f);
            NightScene.Part(PrimitiveType.Cylinder, "Trunk", oak, new Vector3(0f, 2.6f, 0f), new Vector3(1.2f, 2.6f, 1.2f), palette.Bark);
            var branches = new[] { new Vector3(0f, 0f, 38f), new Vector3(0f, 90f, 42f), new Vector3(0f, 200f, 35f), new Vector3(0f, 300f, 48f) };
            foreach (var euler in branches)
                NightScene.Part(PrimitiveType.Cylinder, "Branch", oak, Quaternion.Euler(0f, euler.y, 0f) * new Vector3(0.9f, 5.4f, 0f), new Vector3(0.3f, 1.3f, 0.3f), palette.Bark, euler);
            NightScene.Part(PrimitiveType.Sphere, "Withered Crown", oak, new Vector3(-1.2f, 6.4f, 0.3f), new Vector3(2.6f, 1.6f, 2.4f), palette.Withered);
            NightScene.Part(PrimitiveType.Sphere, "Withered Crown", oak, new Vector3(1.4f, 6.1f, -0.2f), new Vector3(2f, 1.3f, 2f), palette.Withered);
            for (int i = 0; i < 3; i++)
                NightScene.Part(PrimitiveType.Cube, "Shrine Stone", oak, new Vector3(-1f + i, 0.3f, -1.6f), new Vector3(0.5f, 0.6f + i % 2 * 0.3f, 0.4f), palette.StoneDark);
            NightScene.Lamp(oak, "Grave Candles", new Vector3(0f, 0.95f, -1.6f), NightScene.WarmLight, 8f, 8f, palette.Lantern, 0.3f, scale: new Vector3(0.12f, 0.18f, 0.12f));
            NightScene.FitCollider(oak);
        }

        void Tavern(Transform world, Vector3 position)
        {
            var tavern = Place(world, "Tavern", StrongholdPlaceKind.Tavern, "Tavern", position, Vector3.up * 5f);
            House(tavern, 3.8f, 2.6f, 3f, palette.Wood, palette.RoofDark, Vector3.zero, palette.Window);
            NightScene.Part(PrimitiveType.Cylinder, "Sign Post", tavern, new Vector3(-2.3f, 1.2f, -1.6f), new Vector3(0.1f, 1.2f, 0.1f), palette.WoodDark);
            NightScene.Part(PrimitiveType.Cube, "Sign", tavern, new Vector3(-2.3f, 2f, -1.6f), new Vector3(0.9f, 0.55f, 0.08f), palette.Wood);
            NightScene.Lamp(tavern, "Lantern", new Vector3(-2.3f, 1.5f, -1.8f), NightScene.WarmLight, 10f, 9f, palette.Lantern, 0.15f, scale: Vector3.one * 0.24f);
            NightScene.FitCollider(tavern);
        }

        void Shop(Transform world, Vector3 position)
        {
            var shop = Place(world, "Shop", StrongholdPlaceKind.Shop, "Shop", position, Vector3.up * 3.2f);
            NightScene.Part(PrimitiveType.Cube, "Counter", shop, new Vector3(0f, 0.45f, 0f), new Vector3(2.4f, 0.9f, 1.1f), palette.Wood);
            NightScene.Part(PrimitiveType.Cylinder, "Post", shop, new Vector3(-1.2f, 1f, 0.5f), new Vector3(0.09f, 1f, 0.09f), palette.WoodDark);
            NightScene.Part(PrimitiveType.Cylinder, "Post", shop, new Vector3(1.2f, 1f, 0.5f), new Vector3(0.09f, 1f, 0.09f), palette.WoodDark);
            NightScene.Part(PrimitiveType.Cube, "Awning", shop, new Vector3(0f, 2f, 0.1f), new Vector3(2.9f, 0.08f, 1.7f), palette.Cloth, new Vector3(-18f, 0f, 0f));
            NightScene.Part(PrimitiveType.Cube, "Crate", shop, new Vector3(-1.7f, 0.3f, -0.8f), Vector3.one * 0.6f, palette.WoodDark, new Vector3(0f, 15f, 0f));
            NightScene.Part(PrimitiveType.Cube, "Crate", shop, new Vector3(1.8f, 0.25f, -0.7f), Vector3.one * 0.5f, palette.WoodDark, new Vector3(0f, -10f, 0f));
            NightScene.Lamp(shop, "Lantern", new Vector3(0f, 1.45f, -0.75f), NightScene.WarmLight, 9f, 8f, palette.Lantern, 0.15f, scale: Vector3.one * 0.22f);
            NightScene.FitCollider(shop);
        }

        void Signpost(Transform world, Vector3 position)
        {
            var sign = Place(world, "Signpost", StrongholdPlaceKind.Embark, "Embark", position, Vector3.up * 3.4f);
            NightScene.Part(PrimitiveType.Cylinder, "Post", sign, new Vector3(0f, 1.3f, 0f), new Vector3(0.14f, 1.3f, 0.14f), palette.WoodDark);
            NightScene.Part(PrimitiveType.Cube, "Board", sign, new Vector3(0.45f, 2.3f, 0f), new Vector3(1.4f, 0.32f, 0.08f), palette.Wood, new Vector3(0f, 25f, 6f));
            NightScene.Part(PrimitiveType.Cube, "Board", sign, new Vector3(-0.35f, 1.8f, 0f), new Vector3(1.1f, 0.28f, 0.08f), palette.Wood, new Vector3(0f, -20f, -5f));
            NightScene.Part(PrimitiveType.Sphere, "Skull", sign, new Vector3(0f, 2.75f, 0f), Vector3.one * 0.28f, palette.Bone);
            NightScene.Lamp(sign, "Lantern", new Vector3(0.2f, 1.4f, -0.2f), NightScene.WarmLight, 10f, 9f, palette.Lantern, 0.15f, scale: Vector3.one * 0.24f);
            NightScene.FitCollider(sign, 1.2f);
        }

        void Graveyard(Transform world)
        {
            var graves = new GameObject("Graveyard").transform;
            graves.SetParent(world, false);
            var spots = new[] { new Vector3(-0.6f, 0f, 6.2f), new Vector3(0.8f, 0f, 6.6f), new Vector3(2.1f, 0f, 6f), new Vector3(-1.2f, 0f, 4.6f), new Vector3(0.4f, 0f, 4.9f) };
            for (int i = 0; i < spots.Length; i++)
            {
                float tilt = (i % 3 - 1) * 9f;
                NightScene.Decoration(PrimitiveType.Sphere, "Mound", graves, spots[i] + new Vector3(0f, 0.02f, -0.5f), new Vector3(0.9f, 0.25f, 1.5f), palette.Road);
                if (i % 2 == 0)
                {
                    NightScene.Decoration(PrimitiveType.Cube, "Headstone", graves, spots[i] + Vector3.up * 0.4f, new Vector3(0.6f, 0.8f, 0.15f), palette.StoneDark, new Vector3(tilt, 0f, tilt * 0.5f));
                    continue;
                }
                NightScene.Decoration(PrimitiveType.Cube, "Cross", graves, spots[i] + Vector3.up * 0.5f, new Vector3(0.12f, 1f, 0.12f), palette.WoodDark, new Vector3(0f, 0f, tilt));
                NightScene.Decoration(PrimitiveType.Cube, "Cross Bar", graves, spots[i] + Vector3.up * 0.72f, new Vector3(0.55f, 0.12f, 0.12f), palette.WoodDark, new Vector3(0f, 0f, tilt));
            }
            var bones = new[] { new Vector3(3.5f, 0.05f, -2.6f), new Vector3(-3f, 0.05f, 1.5f), new Vector3(1.6f, 0.05f, 4.3f), new Vector3(9.8f, 0.05f, -3.4f) };
            for (int i = 0; i < bones.Length; i++)
                NightScene.Decoration(PrimitiveType.Capsule, "Bone", graves, bones[i], new Vector3(0.08f, 0.22f, 0.08f), palette.Bone, new Vector3(90f, i * 47f, 0f));

            NightScene.Lamp(graves, "Wisp", new Vector3(0.6f, 1.3f, 5.8f), NightScene.SpiritLight, 7f, 7f, palette.Spirit, 0.5f, scale: Vector3.one * 0.2f);
            NightScene.Lamp(graves, "Wisp", new Vector3(3.2f, 1.6f, 7.4f), NightScene.SpiritLight, 6f, 7f, palette.Spirit, 0.5f, scale: Vector3.one * 0.16f);
        }

        void DeadTrees(Transform world)
        {
            var trees = new GameObject("Dead Trees").transform;
            trees.SetParent(world, false);
            var spots = new[] { new Vector3(-13f, 0f, 9f), new Vector3(13f, 0f, 10f), new Vector3(-12.5f, 0f, -1f), new Vector3(13.5f, 0f, -1.5f), new Vector3(-0.5f, 0f, 12f), new Vector3(-8f, 0f, 12.5f), new Vector3(10f, 0f, 12.5f), new Vector3(-3f, 0f, -8f) };
            for (int i = 0; i < spots.Length; i++)
                NightScene.DeadTree(trees, spots[i], i * 53f, 2f + i % 3 * 0.4f, palette.Bark);
        }

        void House(Transform parent, float width, float height, float depth, Material walls, Material roof, Vector3 roofTilt, Material windows)
        {
            NightScene.Part(PrimitiveType.Cube, "Walls", parent, new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, depth), walls);
            float side = width * 0.74f;
            NightScene.Part(PrimitiveType.Cube, "Roof", parent, new Vector3(0f, height, 0f), new Vector3(side, side, depth * 1.12f), roof, new Vector3(roofTilt.x, roofTilt.y, 45f + roofTilt.z));
            NightScene.Part(PrimitiveType.Cube, "Door", parent, new Vector3(0f, height * 0.3f, -depth * 0.5f - 0.02f), new Vector3(width * 0.22f, height * 0.6f, 0.05f), palette.Door);
            foreach (float x in new[] { -width * 0.3f, width * 0.3f })
                NightScene.Part(PrimitiveType.Cube, "Window", parent, new Vector3(x, height * 0.58f, -depth * 0.5f - 0.02f), new Vector3(width * 0.15f, height * 0.2f, 0.04f), windows);
        }

        static Transform Place(Transform world, string name, StrongholdPlaceKind kind, string displayName, Vector3 position, Vector3 labelOffset)
        {
            var root = new GameObject(name).transform;
            root.SetParent(world, false);
            root.position = position;
            var label = new GameObject("Label").transform;
            label.SetParent(root, false);
            label.localPosition = labelOffset;
            var place = new SerializedObject(root.gameObject.AddComponent<StrongholdPlace>());
            place.FindProperty("kind").enumValueIndex = (int)kind;
            place.FindProperty("displayName").stringValue = displayName;
            place.FindProperty("labelAnchor").objectReferenceValue = label;
            place.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }
    }
}

using System.Linq;
using Featherguard.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Featherguard.EditorTools
{
    public sealed class StrongholdSceneBuilder
    {
        static readonly Color NightColor = new(0.05f, 0.06f, 0.08f);
        static readonly Vector3 CampfirePosition = Vector3.zero;
        static readonly Vector3 SignpostPosition = new(8f, 0f, -4.5f);
        static readonly Color WarmLight = new(1f, 0.62f, 0.3f);

        const float SeatRadius = 2.6f;
        const float SeatArcStart = 165f;
        const float SeatArcEnd = 15f;
        const int GroundTiles = 6;
        const float GroundTileSize = 8f;

        Palette palette;

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
            palette = new Palette();
            var postProfile = CreatePostProfile();

            ConfigureLighting();
            var camera = CreateCamera();
            var volume = new GameObject("PostFX").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = postProfile;

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

        static void ConfigureLighting()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.24f, 0.27f, 0.36f);
            RenderSettings.ambientEquatorColor = new Color(0.16f, 0.17f, 0.21f);
            RenderSettings.ambientGroundColor = new Color(0.08f, 0.08f, 0.08f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = NightColor;
            RenderSettings.fogDensity = 0.012f;

            var moon = new GameObject("Moon").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.52f, 0.6f, 0.82f);
            moon.intensity = 2.8f;
            moon.shadows = LightShadows.Soft;
            moon.shadowStrength = 0.8f;
            moon.transform.rotation = Quaternion.Euler(48f, 35f, 0f);
        }

        static Camera CreateCamera()
        {
            var position = new Vector3(0f, 18f, -17.5f);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            cameraObject.transform.SetPositionAndRotation(position, Quaternion.LookRotation(new Vector3(0f, 0f, 3.2f) - position));
            var camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = 40f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = NightColor;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 120f;
            var data = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            return camera;
        }

        void Ground(Transform world)
        {
            var ground = new GameObject("Ground").transform;
            ground.SetParent(world, false);
            for (int x = -GroundTiles; x < GroundTiles; x++)
            for (int z = -GroundTiles; z < GroundTiles; z++)
                Decoration(PrimitiveType.Plane, "Tile", ground, new Vector3((x + 0.5f) * GroundTileSize, 0f, (z + 0.5f) * GroundTileSize),
                    new Vector3(GroundTileSize * 0.1f, 1f, GroundTileSize * 0.1f), palette.Ground);
            Road(world, CampfirePosition, SignpostPosition);
            Road(world, SignpostPosition, SignpostPosition + new Vector3(8f, 0f, -4.5f));
        }

        void Road(Transform world, Vector3 from, Vector3 to)
        {
            var direction = to - from;
            var road = Part(PrimitiveType.Cube, "Road", world, (from + to) * 0.5f + Vector3.up * 0.01f,
                new Vector3(direction.magnitude, 0.02f, 1.4f), palette.Road, new Vector3(0f, -Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg, 0f));
            Object.DestroyImmediate(road.GetComponent<Collider>());
        }

        (Transform, CampSeat[]) Campfire(Transform world, int seatCount)
        {
            var camp = Place(world, "Campfire", StrongholdPlaceKind.Camp, "Camp", CampfirePosition, new Vector3(0f, -0.3f, -1.8f));
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                Part(PrimitiveType.Sphere, "Stone", camp, new Vector3(Mathf.Cos(angle) * 0.75f, 0.1f, Mathf.Sin(angle) * 0.75f), new Vector3(0.38f, 0.26f, 0.38f), palette.StoneDark);
            }
            for (int i = 0; i < 3; i++)
                Part(PrimitiveType.Cylinder, "Log", camp, new Vector3(0f, 0.16f, 0f), new Vector3(0.16f, 0.55f, 0.16f), palette.WoodDark, new Vector3(90f, i * 60f, 0f));
            var flame = Part(PrimitiveType.Sphere, "Flame", camp, new Vector3(0f, 0.55f, 0f), new Vector3(0.5f, 0.9f, 0.5f), palette.Flame).transform;
            Lamp(camp, "Fire Light", new Vector3(0f, 1.3f, 0f), new Color(1f, 0.55f, 0.2f), 14f, 16f, null, 0.35f, flame);

            FinishPlace(camp, 1.6f);
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
                Decoration(PrimitiveType.Cylinder, "Stump", seat, new Vector3(0f, 0.18f, -0.55f), new Vector3(0.45f, 0.18f, 0.45f), palette.WoodDark);
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
            Lamp(forge, "Forge Glow", new Vector3(0f, 0.5f, -1.45f), new Color(1f, 0.38f, 0.12f), 12f, 9f, palette.Ember, 0.3f, scale: new Vector3(0.6f, 0.35f, 0.15f));
            Part(PrimitiveType.Cube, "Chimney", forge, new Vector3(0.9f, 3.2f, 0.4f), new Vector3(0.5f, 2.2f, 0.5f), palette.StoneDark);
            Part(PrimitiveType.Cube, "Anvil Base", forge, new Vector3(2.1f, 0.25f, -1.2f), new Vector3(0.35f, 0.5f, 0.35f), palette.Iron);
            Part(PrimitiveType.Cube, "Anvil", forge, new Vector3(2.1f, 0.6f, -1.2f), new Vector3(0.8f, 0.2f, 0.35f), palette.Iron);
            FinishPlace(forge);
        }

        void WiseWomansHut(Transform world, Vector3 position)
        {
            var hut = Place(world, "Wise Woman's Hut", StrongholdPlaceKind.WiseWomansHut, "Wise Woman's Hut", position, Vector3.up * 3.8f);
            House(hut, 2.4f, 1.8f, 2.2f, palette.WoodDark, palette.RoofMoss, new Vector3(0f, 7f, 5f), palette.WitchGlow);
            Part(PrimitiveType.Cylinder, "Cauldron", hut, new Vector3(1.6f, 0.35f, -1.2f), new Vector3(0.7f, 0.35f, 0.7f), palette.Iron);
            Lamp(hut, "Cauldron Glow", new Vector3(1.6f, 0.72f, -1.2f), new Color(0.45f, 1f, 0.4f), 9f, 8f, palette.WitchGlow, 0.25f, scale: new Vector3(0.6f, 0.08f, 0.6f));
            Part(PrimitiveType.Cylinder, "Pole", hut, new Vector3(-1.6f, 0.8f, -1.1f), new Vector3(0.06f, 0.8f, 0.06f), palette.WoodDark);
            Part(PrimitiveType.Sphere, "Skull", hut, new Vector3(-1.6f, 1.7f, -1.1f), Vector3.one * 0.3f, palette.Bone);
            FinishPlace(hut);
        }

        void SacredOak(Transform world, Vector3 position)
        {
            var oak = Place(world, "Sacred Oak", StrongholdPlaceKind.SacredOak, "Sacred Oak", position, Vector3.up * 5.6f);
            Part(PrimitiveType.Cylinder, "Trunk", oak, new Vector3(0f, 2.6f, 0f), new Vector3(1.2f, 2.6f, 1.2f), palette.Bark);
            var branches = new[] { new Vector3(0f, 0f, 38f), new Vector3(0f, 90f, 42f), new Vector3(0f, 200f, 35f), new Vector3(0f, 300f, 48f) };
            foreach (var euler in branches)
                Part(PrimitiveType.Cylinder, "Branch", oak, Quaternion.Euler(0f, euler.y, 0f) * new Vector3(0.9f, 5.4f, 0f), new Vector3(0.3f, 1.3f, 0.3f), palette.Bark, euler);
            Part(PrimitiveType.Sphere, "Withered Crown", oak, new Vector3(-1.2f, 6.4f, 0.3f), new Vector3(2.6f, 1.6f, 2.4f), palette.Withered);
            Part(PrimitiveType.Sphere, "Withered Crown", oak, new Vector3(1.4f, 6.1f, -0.2f), new Vector3(2f, 1.3f, 2f), palette.Withered);
            for (int i = 0; i < 3; i++)
                Part(PrimitiveType.Cube, "Shrine Stone", oak, new Vector3(-1f + i, 0.3f, -1.6f), new Vector3(0.5f, 0.6f + i % 2 * 0.3f, 0.4f), palette.StoneDark);
            Lamp(oak, "Grave Candles", new Vector3(0f, 0.95f, -1.6f), WarmLight, 8f, 8f, palette.Lantern, 0.3f, scale: new Vector3(0.12f, 0.18f, 0.12f));
            FinishPlace(oak);
        }

        void Tavern(Transform world, Vector3 position)
        {
            var tavern = Place(world, "Tavern", StrongholdPlaceKind.Tavern, "Tavern", position, Vector3.up * 5f);
            House(tavern, 3.8f, 2.6f, 3f, palette.Wood, palette.RoofDark, Vector3.zero, palette.Window);
            Part(PrimitiveType.Cylinder, "Sign Post", tavern, new Vector3(-2.3f, 1.2f, -1.6f), new Vector3(0.1f, 1.2f, 0.1f), palette.WoodDark);
            Part(PrimitiveType.Cube, "Sign", tavern, new Vector3(-2.3f, 2f, -1.6f), new Vector3(0.9f, 0.55f, 0.08f), palette.Wood);
            Lamp(tavern, "Lantern", new Vector3(-2.3f, 1.5f, -1.8f), WarmLight, 10f, 9f, palette.Lantern, 0.15f, scale: Vector3.one * 0.24f);
            FinishPlace(tavern);
        }

        void Shop(Transform world, Vector3 position)
        {
            var shop = Place(world, "Shop", StrongholdPlaceKind.Shop, "Shop", position, Vector3.up * 3.2f);
            Part(PrimitiveType.Cube, "Counter", shop, new Vector3(0f, 0.45f, 0f), new Vector3(2.4f, 0.9f, 1.1f), palette.Wood);
            Part(PrimitiveType.Cylinder, "Post", shop, new Vector3(-1.2f, 1f, 0.5f), new Vector3(0.09f, 1f, 0.09f), palette.WoodDark);
            Part(PrimitiveType.Cylinder, "Post", shop, new Vector3(1.2f, 1f, 0.5f), new Vector3(0.09f, 1f, 0.09f), palette.WoodDark);
            Part(PrimitiveType.Cube, "Awning", shop, new Vector3(0f, 2f, 0.1f), new Vector3(2.9f, 0.08f, 1.7f), palette.Cloth, new Vector3(-18f, 0f, 0f));
            Part(PrimitiveType.Cube, "Crate", shop, new Vector3(-1.7f, 0.3f, -0.8f), Vector3.one * 0.6f, palette.WoodDark, new Vector3(0f, 15f, 0f));
            Part(PrimitiveType.Cube, "Crate", shop, new Vector3(1.8f, 0.25f, -0.7f), Vector3.one * 0.5f, palette.WoodDark, new Vector3(0f, -10f, 0f));
            Lamp(shop, "Lantern", new Vector3(0f, 1.45f, -0.75f), WarmLight, 9f, 8f, palette.Lantern, 0.15f, scale: Vector3.one * 0.22f);
            FinishPlace(shop);
        }

        void Signpost(Transform world, Vector3 position)
        {
            var sign = Place(world, "Signpost", StrongholdPlaceKind.Embark, "Embark", position, Vector3.up * 3.4f);
            Part(PrimitiveType.Cylinder, "Post", sign, new Vector3(0f, 1.3f, 0f), new Vector3(0.14f, 1.3f, 0.14f), palette.WoodDark);
            Part(PrimitiveType.Cube, "Board", sign, new Vector3(0.45f, 2.3f, 0f), new Vector3(1.4f, 0.32f, 0.08f), palette.Wood, new Vector3(0f, 25f, 6f));
            Part(PrimitiveType.Cube, "Board", sign, new Vector3(-0.35f, 1.8f, 0f), new Vector3(1.1f, 0.28f, 0.08f), palette.Wood, new Vector3(0f, -20f, -5f));
            Part(PrimitiveType.Sphere, "Skull", sign, new Vector3(0f, 2.75f, 0f), Vector3.one * 0.28f, palette.Bone);
            Lamp(sign, "Lantern", new Vector3(0.2f, 1.4f, -0.2f), WarmLight, 10f, 9f, palette.Lantern, 0.15f, scale: Vector3.one * 0.24f);
            FinishPlace(sign, 1.2f);
        }

        void Graveyard(Transform world)
        {
            var graves = new GameObject("Graveyard").transform;
            graves.SetParent(world, false);
            var spots = new[] { new Vector3(-0.6f, 0f, 6.2f), new Vector3(0.8f, 0f, 6.6f), new Vector3(2.1f, 0f, 6f), new Vector3(-1.2f, 0f, 4.6f), new Vector3(0.4f, 0f, 4.9f) };
            for (int i = 0; i < spots.Length; i++)
            {
                float tilt = (i % 3 - 1) * 9f;
                Decoration(PrimitiveType.Sphere, "Mound", graves, spots[i] + new Vector3(0f, 0.02f, -0.5f), new Vector3(0.9f, 0.25f, 1.5f), palette.Road);
                if (i % 2 == 0)
                {
                    Decoration(PrimitiveType.Cube, "Headstone", graves, spots[i] + Vector3.up * 0.4f, new Vector3(0.6f, 0.8f, 0.15f), palette.StoneDark, new Vector3(tilt, 0f, tilt * 0.5f));
                    continue;
                }
                Decoration(PrimitiveType.Cube, "Cross", graves, spots[i] + Vector3.up * 0.5f, new Vector3(0.12f, 1f, 0.12f), palette.WoodDark, new Vector3(0f, 0f, tilt));
                Decoration(PrimitiveType.Cube, "Cross Bar", graves, spots[i] + Vector3.up * 0.72f, new Vector3(0.55f, 0.12f, 0.12f), palette.WoodDark, new Vector3(0f, 0f, tilt));
            }
            var bones = new[] { new Vector3(3.5f, 0.05f, -2.6f), new Vector3(-3f, 0.05f, 1.5f), new Vector3(1.6f, 0.05f, 4.3f), new Vector3(9.8f, 0.05f, -3.4f) };
            for (int i = 0; i < bones.Length; i++)
                Decoration(PrimitiveType.Capsule, "Bone", graves, bones[i], new Vector3(0.08f, 0.22f, 0.08f), palette.Bone, new Vector3(90f, i * 47f, 0f));

            var spirit = new Color(0.55f, 0.75f, 1f);
            Lamp(graves, "Wisp", new Vector3(0.6f, 1.3f, 5.8f), spirit, 7f, 7f, palette.Spirit, 0.5f, scale: Vector3.one * 0.2f);
            Lamp(graves, "Wisp", new Vector3(3.2f, 1.6f, 7.4f), spirit, 6f, 7f, palette.Spirit, 0.5f, scale: Vector3.one * 0.16f);
        }

        static void Lamp(Transform parent, string name, Vector3 position, Color color, float intensity, float range, Material glow, float flicker,
            Transform flame = null, Vector3 scale = default)
        {
            if (glow != null)
                Decoration(PrimitiveType.Sphere, $"{name} Glow", parent, position, scale, glow);
            var lightObject = new GameObject(name, typeof(Light));
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = position + Vector3.up * 0.2f;
            var lamp = lightObject.GetComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = color;
            lamp.range = range;
            lamp.intensity = intensity;
            lamp.shadows = LightShadows.None;
            var flickering = new SerializedObject(lightObject.AddComponent<FlickeringLight>());
            flickering.FindProperty("baseIntensity").floatValue = intensity;
            flickering.FindProperty("flicker").floatValue = flicker;
            flickering.FindProperty("flame").objectReferenceValue = flame;
            flickering.ApplyModifiedPropertiesWithoutUndo();
        }

        void DeadTrees(Transform world)
        {
            var trees = new GameObject("Dead Trees").transform;
            trees.SetParent(world, false);
            var spots = new[] { new Vector3(-13f, 0f, 9f), new Vector3(13f, 0f, 10f), new Vector3(-12.5f, 0f, -1f), new Vector3(13.5f, 0f, -1.5f), new Vector3(-0.5f, 0f, 12f), new Vector3(-8f, 0f, 12.5f), new Vector3(10f, 0f, 12.5f), new Vector3(-3f, 0f, -8f) };
            for (int i = 0; i < spots.Length; i++)
            {
                float height = 2f + i % 3 * 0.4f;
                var tree = new GameObject("Dead Tree").transform;
                tree.SetParent(trees, false);
                tree.SetPositionAndRotation(spots[i], Quaternion.Euler(0f, i * 53f, 0f));
                Decoration(PrimitiveType.Cylinder, "Trunk", tree, new Vector3(0f, height, 0f), new Vector3(0.35f, height, 0.35f), palette.Bark);
                Decoration(PrimitiveType.Cylinder, "Branch", tree, new Vector3(0.45f, height * 1.7f, 0f), new Vector3(0.11f, 0.8f, 0.11f), palette.Bark, new Vector3(0f, 0f, -40f));
                Decoration(PrimitiveType.Cylinder, "Branch", tree, new Vector3(-0.4f, height * 1.5f, 0f), new Vector3(0.1f, 0.7f, 0.1f), palette.Bark, new Vector3(0f, 0f, 45f));
                Decoration(PrimitiveType.Cylinder, "Branch", tree, new Vector3(0f, height * 1.85f, 0.35f), new Vector3(0.08f, 0.5f, 0.08f), palette.Bark, new Vector3(35f, 0f, 0f));
            }
        }

        void House(Transform parent, float width, float height, float depth, Material walls, Material roof, Vector3 roofTilt, Material windows)
        {
            Part(PrimitiveType.Cube, "Walls", parent, new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, depth), walls);
            float side = width * 0.74f;
            Part(PrimitiveType.Cube, "Roof", parent, new Vector3(0f, height, 0f), new Vector3(side, side, depth * 1.12f), roof, new Vector3(roofTilt.x, roofTilt.y, 45f + roofTilt.z));
            Part(PrimitiveType.Cube, "Door", parent, new Vector3(0f, height * 0.3f, -depth * 0.5f - 0.02f), new Vector3(width * 0.22f, height * 0.6f, 0.05f), palette.Door);
            foreach (float x in new[] { -width * 0.3f, width * 0.3f })
                Part(PrimitiveType.Cube, "Window", parent, new Vector3(x, height * 0.58f, -depth * 0.5f - 0.02f), new Vector3(width * 0.15f, height * 0.2f, 0.04f), windows);
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

        static void FinishPlace(Transform root, float minimumSize = 0f)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            foreach (var collider in root.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(collider);
            var box = root.gameObject.AddComponent<BoxCollider>();
            box.center = root.InverseTransformPoint(bounds.center);
            box.size = Vector3.Max(bounds.size, Vector3.one * minimumSize);
        }

        static GameObject Decoration(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3 euler = default)
        {
            var part = Part(type, name, parent, position, scale, material, euler);
            Object.DestroyImmediate(part.GetComponent<Collider>());
            return part;
        }

        static GameObject Part(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3 euler = default)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = Quaternion.Euler(euler);
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part;
        }

        static VolumeProfile CreatePostProfile()
        {
            AssetDatabase.DeleteAsset(ContentPaths.StrongholdPostProfile);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ContentPaths.StrongholdPostProfile);

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(1.2f);
            bloom.threshold.Override(0.95f);
            var color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(-35f);
            color.contrast.Override(8f);
            color.postExposure.Override(0.45f);
            color.colorFilter.Override(new Color(0.88f, 0.92f, 1f));
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.4f);
            vignette.smoothness.Override(0.45f);
            profile.Add<Tonemapping>(true).mode.Override(TonemappingMode.Neutral);

            foreach (var component in profile.components)
            {
                component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            AssetDatabase.SaveAssets();
            return profile;
        }

        sealed class Palette
        {
            public readonly Material Ground = Lit("Ground", new Color(0.11f, 0.16f, 0.09f));
            public readonly Material Road = Lit("Road", new Color(0.17f, 0.14f, 0.11f));
            public readonly Material Wood = Lit("Wood", new Color(0.27f, 0.19f, 0.13f));
            public readonly Material WoodDark = Lit("Wood Dark", new Color(0.17f, 0.12f, 0.09f));
            public readonly Material Stone = Lit("Stone", new Color(0.29f, 0.29f, 0.3f));
            public readonly Material StoneDark = Lit("Stone Dark", new Color(0.17f, 0.17f, 0.19f));
            public readonly Material RoofDark = Lit("Roof Dark", new Color(0.12f, 0.1f, 0.11f));
            public readonly Material RoofMoss = Lit("Roof Moss", new Color(0.13f, 0.16f, 0.11f));
            public readonly Material Door = Lit("Door", new Color(0.06f, 0.05f, 0.05f));
            public readonly Material Bark = Lit("Bark", new Color(0.2f, 0.17f, 0.08f));
            public readonly Material Withered = Lit("Withered", new Color(0.16f, 0.17f, 0.12f));
            public readonly Material Iron = Lit("Iron", new Color(0.12f, 0.12f, 0.13f));
            public readonly Material Bone = Lit("Bone", new Color(0.76f, 0.73f, 0.64f));
            public readonly Material Cloth = Lit("Cloth", new Color(0.32f, 0.09f, 0.09f));
            public readonly Material Flame = Unlit("Flame", new Color(4f, 1.6f, 0.4f));
            public readonly Material Ember = Unlit("Ember", new Color(4f, 1.2f, 0.3f));
            public readonly Material Lantern = Unlit("Lantern", new Color(3f, 1.8f, 0.7f));
            public readonly Material Window = Unlit("Window", new Color(1.8f, 1f, 0.4f));
            public readonly Material WitchGlow = Unlit("Witch Glow", new Color(0.7f, 2.4f, 0.6f));
            public readonly Material Spirit = Unlit("Spirit", new Color(1.2f, 1.8f, 3.2f));

            static Material Lit(string name, Color color)
            {
                var material = Load(name, "Universal Render Pipeline/Lit");
                material.SetColor("_BaseColor", color);
                material.SetFloat("_Smoothness", 0f);
                EditorUtility.SetDirty(material);
                return material;
            }

            static Material Unlit(string name, Color color)
            {
                var material = Load(name, "Universal Render Pipeline/Unlit");
                material.SetColor("_BaseColor", color);
                EditorUtility.SetDirty(material);
                return material;
            }

            static Material Load(string name, string shader)
            {
                Seeder.EnsureFolder(ContentPaths.StrongholdMaterials);
                string path = $"{ContentPaths.StrongholdMaterials}/{name}.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material != null && material.shader.name == shader) return material;
                if (material != null) AssetDatabase.DeleteAsset(path);
                material = new Material(Shader.Find(shader));
                AssetDatabase.CreateAsset(material, path);
                return material;
            }
        }
    }
}

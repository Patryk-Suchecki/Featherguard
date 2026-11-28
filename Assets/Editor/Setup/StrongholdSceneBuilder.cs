using System;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public sealed class StrongholdSceneBuilder
    {
        const string Models = ContentPaths.Environment + "/Stronghold";
        const string LayoutPath = Models + "/Stronghold.layout.json";
        const string Materials = Models + "/Materials";
        const string StumpModel = "Stump";
        const string FireGroup = "Fire";
        const float SeatInFront = 0.55f;
        const float LabelGap = 0.6f;
        const float FarClip = 400f;
        const float MoonIntensity = 1.4f;
        const float FogDensity = 0.009f;

        static readonly Color Sky = new(0.13f, 0.16f, 0.24f);
        static readonly Quaternion ImportTurn = Quaternion.Euler(90f, 0f, 0f);

        static readonly Dictionary<string, (StrongholdPlaceKind Kind, string Name)> Places = new()
        {
            ["Forge"] = (StrongholdPlaceKind.Forge, "Forge"),
            ["WiseWomansHut"] = (StrongholdPlaceKind.WiseWomansHut, "Wise Woman's Hut"),
            ["SacredOak"] = (StrongholdPlaceKind.SacredOak, "Sacred Oak"),
            ["Tavern"] = (StrongholdPlaceKind.Tavern, "Tavern"),
            ["Shop"] = (StrongholdPlaceKind.Shop, "Shop"),
            ["Camp"] = (StrongholdPlaceKind.Camp, "Camp"),
            ["Embark"] = (StrongholdPlaceKind.Embark, "Embark"),
        };

        static readonly Dictionary<string, float> Flicker = new() { [FireGroup] = 0.35f, ["Forge"] = 0.3f };

        NightPalette palette;
        Vector3 campfirePosition;

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
            var layout = StrongholdLayout.Load(LayoutPath);
            if (layout == null)
            {
                Debug.LogError($"Featherguard: {LayoutPath} is missing. Export it with Art/Blender/Environment/Stronghold/convert_camp.py.");
                return;
            }

            string configPath = AssetDatabase.GetAssetPath(config);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            config = AssetDatabase.LoadAssetAtPath<GameConfig>(configPath);
            palette = new NightPalette();
            campfirePosition = StrongholdLayout.Point(layout.campfire);
            NightScene.PostFX(ContentPaths.StrongholdPostProfile);
            NightScene.ConfigureLighting().intensity = MoonIntensity;
            RenderSettings.fogColor = Sky;
            RenderSettings.fogDensity = FogDensity;
            var camera = Camera(layout.camera);

            var world = new GameObject("World").transform;
            Transform campfire = null;
            foreach (var model in layout.models.Where(m => m.name != StumpModel))
            {
                var root = Model(world, model);
                if (model.place == "Camp") campfire = root;
            }
            var lights = new GameObject("Lights").transform;
            lights.SetParent(world, false);
            foreach (var light in layout.lights)
                Light(lights, light);
            Flame(campfire, lights.Find(layout.lights.First(l => l.group == FireGroup).name));
            var seats = Seats(world, layout.seats, layout.models.First(m => m.name == StumpModel), config.campSeats);

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

        static Camera Camera(CameraPlacement placement)
        {
            var position = StrongholdLayout.Point(placement.position);
            var forward = StrongholdLayout.Point(placement.forward);
            var camera = NightScene.CreateCamera(position, position + forward, placement.fieldOfView);
            camera.transform.rotation = Quaternion.LookRotation(forward, StrongholdLayout.Point(placement.up));
            camera.farClipPlane = FarClip;
            camera.backgroundColor = Sky;
            return camera;
        }

        static Transform Model(Transform world, ModelPlacement placement)
        {
            bool tappable = placement.place != null && Places.ContainsKey(placement.place);
            var root = tappable
                ? Place(world, placement, Places[placement.place].Kind, Places[placement.place].Name)
                : Group(world, placement.name, StrongholdLayout.Point(placement.position));
            Instance(root, placement.name, Quaternion.Euler(0f, placement.yaw, 0f), placement.scale);
            if (tappable)
                NightScene.FitCollider(root, placement.place == "Camp" ? 1.6f : 0f);
            return root;
        }

        static Transform Group(Transform parent, string name, Vector3 position)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            group.position = position;
            return group;
        }

        static GameObject Instance(Transform parent, string name, Quaternion rotation, float scale)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Models}/{name}.fbx");
            if (prefab == null) throw new InvalidOperationException($"Featherguard: {Models}/{name}.fbx is missing.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = rotation * ImportTurn;
            instance.transform.localScale = Vector3.one * scale;
            var material = ToonMaterial(name);
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterial = material;
            return instance;
        }

        static Material ToonMaterial(string name)
        {
            string path = $"{Materials}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Seeder.EnsureFolder(Materials);
                material = new Material(Shader.Find("Featherguard/Toon"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = Shader.Find("Featherguard/Toon");
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{Models}/Textures/{name}_Albedo.png"));
            material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
            return material;
        }

        void Flame(Transform campfire, Transform fireLight)
        {
            var flame = NightScene.Decoration(PrimitiveType.Sphere, "Flame", campfire, new Vector3(0f, 0.35f, 0f), new Vector3(0.3f, 0.55f, 0.3f), palette.Flame).transform;
            var flickering = fireLight != null ? fireLight.GetComponent<FlickeringLight>() : null;
            if (flickering == null) return;
            var serialized = new SerializedObject(flickering);
            serialized.FindProperty("flame").objectReferenceValue = flame;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static CampSeat[] Seats(Transform world, SeatPlacement[] placements, ModelPlacement stump, int count)
        {
            var party = new GameObject("Seats").transform;
            party.SetParent(world, false);
            return placements.Take(count).Select((placement, i) =>
            {
                var rotation = Quaternion.Euler(0f, placement.yaw, 0f);
                var seat = new GameObject($"Seat {i + 1}").transform;
                seat.SetParent(party, false);
                seat.SetPositionAndRotation(StrongholdLayout.Point(placement.position) + rotation * Vector3.forward * SeatInFront, rotation);
                var model = Instance(seat, StumpModel, Quaternion.identity, stump.scale);
                model.transform.localPosition = Vector3.back * SeatInFront;
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

        static void Light(Transform parent, LightPlacement placement)
        {
            var color = new Color(placement.color[0], placement.color[1], placement.color[2]);
            float intensity = Mathf.Clamp(placement.energy / 6f, 1f, 12f);
            float range = Mathf.Clamp(5f + placement.energy / 8f, 5f, 16f);
            float flicker = Flicker.TryGetValue(placement.group, out float value) ? value : 0.1f;
            NightScene.Lamp(parent, placement.name, StrongholdLayout.Point(placement.position) - Vector3.up * 0.2f, color, intensity, range, null, flicker);
        }

        Transform CampView(Transform world)
        {
            var view = new GameObject("Camp View").transform;
            view.SetParent(world, false);
            view.position = campfirePosition + new Vector3(0f, 3.4f, -4.6f);
            view.rotation = Quaternion.LookRotation(campfirePosition + new Vector3(0f, 0.7f, 0.9f) - view.position);
            return view;
        }

        static Transform Place(Transform world, ModelPlacement placement, StrongholdPlaceKind kind, string displayName)
        {
            var root = Group(world, displayName, StrongholdLayout.Point(placement.position));
            var label = new GameObject("Label").transform;
            label.SetParent(root, false);
            label.localPosition = Vector3.up * (placement.height + LabelGap);
            var place = new SerializedObject(root.gameObject.AddComponent<StrongholdPlace>());
            place.FindProperty("kind").enumValueIndex = (int)kind;
            place.FindProperty("displayName").stringValue = displayName;
            place.FindProperty("labelAnchor").objectReferenceValue = label;
            place.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }
    }
}

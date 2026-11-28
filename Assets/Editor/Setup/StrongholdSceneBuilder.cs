using System;
using System.Collections.Generic;
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
        const string Models = ContentPaths.Environment + "/Stronghold";
        const string LayoutPath = Models + "/Stronghold.layout.json";
        const string Materials = Models + "/Materials";
        const string StumpModel = "Stump";
        const string FireGroup = "Fire";
        const float SeatInFront = 0.3f;
        const float HeroScale = 0.55f;
        const float LeafCutoff = 0.45f;
        const string LeafShader = "Featherguard/Toon Leaves";
        const float LabelGap = 0.6f;
        const float FarClip = 400f;
        const float MoonIntensity = 1.4f;
        const float FogDensity = 0.006f;
        const float SkyRotation = 90f;
        const string SkyTexture = Models + "/Textures/Sky_Panorama.png";
        const float Saturation = -12f;

        static readonly Color Horizon = new(0.34f, 0.39f, 0.49f);
        static readonly Color Moonlight = new(0.72f, 0.74f, 0.86f);
        static readonly Color ToonShadow = new(0.38f, 0.38f, 0.48f);
        static readonly Color ColorFilter = new(1f, 0.97f, 0.94f);
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

        static readonly HashSet<string> LeafCards = new() { "SacredOakCrown" };

        static readonly Dictionary<string, float> Flicker = new() { [FireGroup] = 0.35f, ["Forge"] = 0.3f };

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
            campfirePosition = StrongholdLayout.Point(layout.campfire);
            NightScene.PostFX(ContentPaths.StrongholdPostProfile);
            WarmPostFX();
            var moon = NightScene.ConfigureLighting();
            moon.intensity = MoonIntensity;
            moon.color = Moonlight;
            RenderSettings.fogColor = Horizon;
            RenderSettings.fogDensity = FogDensity;
            RenderSettings.skybox = SkyMaterial();
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
            serialized.FindProperty("heroScale").floatValue = HeroScale;
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
            camera.clearFlags = CameraClearFlags.Skybox;
            return camera;
        }

        static Material SkyMaterial()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(SkyTexture);
            if (importer.mipmapEnabled || importer.maxTextureSize != 4096 || importer.wrapModeV != TextureWrapMode.Clamp)
            {
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 4096;
                importer.wrapModeU = TextureWrapMode.Repeat;
                importer.wrapModeV = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
            string path = $"{Materials}/Sky.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Seeder.EnsureFolder(Materials);
                material = new Material(Shader.Find("Skybox/Panoramic"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = Shader.Find("Skybox/Panoramic");
            material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(SkyTexture));
            material.SetFloat("_Mapping", 1f);
            material.SetFloat("_ImageType", 0f);
            material.SetFloat("_Rotation", SkyRotation);
            EditorUtility.SetDirty(material);
            return material;
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
            material.SetColor("_ShadowColor", ToonShadow);
            if (LeafCards.Contains(name)) Leaves(material, name);
            EditorUtility.SetDirty(material);
            return material;
        }

        static void Leaves(Material material, string name)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath($"{Models}/Textures/{name}_Albedo.png");
            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            material.shader = Shader.Find(LeafShader);
            material.SetFloat("_Cutoff", LeafCutoff);
        }

        static void WarmPostFX()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ContentPaths.StrongholdPostProfile);
            if (profile == null || !profile.TryGet<ColorAdjustments>(out var color)) return;
            color.saturation.Override(Saturation);
            color.colorFilter.Override(ColorFilter);
            EditorUtility.SetDirty(profile);
        }

        void Flame(Transform campfire, Transform fireLight)
        {
            var flame = CampfireEffect.Create(campfire, Materials);
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
                collider.center = Vector3.up * 0.8f * HeroScale;
                collider.height = 1.7f * HeroScale;
                collider.radius = 0.6f * HeroScale;
                var label = new GameObject("Label").transform;
                label.SetParent(seat, false);
                label.localPosition = Vector3.up * 2.1f * HeroScale;
                var component = new SerializedObject(seat.gameObject.AddComponent<CampSeat>());
                component.FindProperty("index").intValue = i;
                component.FindProperty("labelAnchor").objectReferenceValue = label;
                component.ApplyModifiedPropertiesWithoutUndo();
                return seat.GetComponent<CampSeat>();
            }).ToArray();
        }

        static void Light(Transform parent, LightPlacement placement)
        {
            var color = placement.group == FireGroup ? NightScene.WarmLight : new Color(placement.color[0], placement.color[1], placement.color[2]);
            float intensity = Mathf.Clamp(placement.energy / 30f, 0.5f, 3f);
            float range = Mathf.Clamp(4f + placement.energy / 10f, 4f, 12f);
            float flicker = Flicker.TryGetValue(placement.group, out float value) ? value : 0.1f;
            NightScene.Lamp(parent, placement.name, StrongholdLayout.Point(placement.position) - Vector3.up * 0.2f, color, intensity, range, null, flicker);
        }

        Transform CampView(Transform world)
        {
            var view = new GameObject("Camp View").transform;
            view.SetParent(world, false);
            view.position = campfirePosition + new Vector3(0f, 2.3f, -3.4f);
            view.rotation = Quaternion.LookRotation(campfirePosition + new Vector3(0f, 0.45f, 0.8f) - view.position);
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

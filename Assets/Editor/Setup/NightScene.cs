using Featherguard.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Featherguard.EditorTools
{
    public static class NightScene
    {
        public static readonly Color NightColor = new(0.05f, 0.06f, 0.08f);
        public static readonly Color WarmLight = new(1f, 0.62f, 0.3f);
        public static readonly Color SpiritLight = new(0.55f, 0.75f, 1f);

        public static Light ConfigureLighting()
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
            return moon;
        }

        public static Camera CreateCamera(Vector3 position, Vector3 target, float fieldOfView)
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            cameraObject.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
            var camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = fieldOfView;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = NightColor;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 120f;
            var data = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            return camera;
        }

        public static void PostFX(string profilePath)
        {
            var volume = new GameObject("PostFX").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = CreatePostProfile(profilePath);
        }

        public static void Ground(Transform world, Material material, int tiles = 6, float tileSize = 8f)
        {
            var ground = new GameObject("Ground").transform;
            ground.SetParent(world, false);
            for (int x = -tiles; x < tiles; x++)
            for (int z = -tiles; z < tiles; z++)
                Decoration(PrimitiveType.Plane, "Tile", ground, new Vector3((x + 0.5f) * tileSize, 0f, (z + 0.5f) * tileSize),
                    new Vector3(tileSize * 0.1f, 1f, tileSize * 0.1f), material);
        }

        public static void DeadTree(Transform parent, Vector3 position, float yaw, float height, Material bark)
        {
            var tree = new GameObject("Dead Tree").transform;
            tree.SetParent(parent, false);
            tree.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            Decoration(PrimitiveType.Cylinder, "Trunk", tree, new Vector3(0f, height, 0f), new Vector3(0.35f, height, 0.35f), bark);
            Decoration(PrimitiveType.Cylinder, "Branch", tree, new Vector3(0.45f, height * 1.7f, 0f), new Vector3(0.11f, 0.8f, 0.11f), bark, new Vector3(0f, 0f, -40f));
            Decoration(PrimitiveType.Cylinder, "Branch", tree, new Vector3(-0.4f, height * 1.5f, 0f), new Vector3(0.1f, 0.7f, 0.1f), bark, new Vector3(0f, 0f, 45f));
            Decoration(PrimitiveType.Cylinder, "Branch", tree, new Vector3(0f, height * 1.85f, 0.35f), new Vector3(0.08f, 0.5f, 0.08f), bark, new Vector3(35f, 0f, 0f));
        }

        public static void Lamp(Transform parent, string name, Vector3 position, Color color, float intensity, float range, Material glow, float flicker,
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

        public static void FitCollider(Transform root, float minimumSize = 0f)
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

        public static GameObject Decoration(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3 euler = default)
        {
            var part = Part(type, name, parent, position, scale, material, euler);
            Object.DestroyImmediate(part.GetComponent<Collider>());
            return part;
        }

        public static GameObject Part(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3 euler = default)
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

        static VolumeProfile CreatePostProfile(string path)
        {
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

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
    }
}

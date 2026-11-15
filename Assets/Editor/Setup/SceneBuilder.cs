using Featherguard.Feedback;
using Featherguard.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Featherguard.EditorTools
{
    public static class SceneBuilder
    {
        public static void Build(GameConfig config)
        {
            var postProfile = CreatePostProfile();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConfigureLighting();
            var (camera, shake) = CreateCamera();
            var volume = new GameObject("PostFX").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = postProfile;

            var arena = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{ContentPaths.Environment}/Arena.fbx"));
            arena.transform.position = Vector3.zero;

            var bootstrap = new GameObject("Game").AddComponent<GameBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.FindProperty("battleCamera").objectReferenceValue = camera;
            serialized.FindProperty("cameraShake").objectReferenceValue = shake;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Seeder.EnsureFolder(System.IO.Path.GetDirectoryName(ContentPaths.Scene));
            EditorSceneManager.SaveScene(scene, ContentPaths.Scene);
            BuildScenes.Register();
        }

        static void ConfigureLighting()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.74f, 0.95f);
            RenderSettings.ambientEquatorColor = new Color(0.56f, 0.62f, 0.56f);
            RenderSettings.ambientGroundColor = new Color(0.32f, 0.28f, 0.22f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.68f, 0.84f, 0.98f);
            RenderSettings.fogStartDistance = 30f;
            RenderSettings.fogEndDistance = 95f;

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.86f);
            sun.intensity = 1.6f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.7f;
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        }

        static (Camera, CameraShake) CreateCamera()
        {
            var rig = new GameObject("CameraRig").transform;
            var position = new Vector3(0f, 3.2f, -11.5f);
            rig.SetPositionAndRotation(position, Quaternion.LookRotation(new Vector3(0f, 1f, 0.3f) - position));
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            cameraObject.transform.SetParent(rig, false);
            var camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = 32f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.56f, 0.78f, 0.98f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 120f;
            var data = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            return (camera, cameraObject.AddComponent<CameraShake>());
        }

        static VolumeProfile CreatePostProfile()
        {
            AssetDatabase.DeleteAsset(ContentPaths.PostProfile);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ContentPaths.PostProfile);

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.7f);
            bloom.threshold.Override(1f);
            var color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(28f);
            color.contrast.Override(16f);
            color.postExposure.Override(-0.05f);
            profile.Add<Vignette>(true).intensity.Override(0.22f);
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

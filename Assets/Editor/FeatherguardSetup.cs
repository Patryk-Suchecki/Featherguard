using System.Collections.Generic;
using System.IO;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Feel;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Featherguard.EditorTools
{
    public static class FeatherguardSetup
    {
        const string CharacterDir = "Assets/Art/Characters";
        const string EnvironmentDir = "Assets/Art/Environment";
        const string VfxDir = "Assets/Art/VFX";
        const string MaterialDir = "Assets/Art/Materials";
        const string AnimationDir = "Assets/Animation";
        const string DataDir = "Assets/Data";
        const string AbilityDir = "Assets/Data/Abilities";
        const string UnitDir = "Assets/Data/Units";
        const string EncounterDir = "Assets/Data/Encounters";
        const string SceneDir = "Assets/Scenes";
        const string ScenePath = "Assets/Scenes/Battle.unity";

        [MenuItem("Featherguard/Rebuild Battle Scene")]
        public static void Run()
        {
            foreach (var dir in new[] { VfxDir, MaterialDir, AnimationDir, AbilityDir, UnitDir, EncounterDir, SceneDir })
                Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();

            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/Art" }))
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);

            var dot = CreateTexture($"{VfxDir}/SoftDot.png", r => Mathf.Pow(Mathf.Clamp01(1f - r), 2f));
            var ring = CreateTexture($"{VfxDir}/Ring.png", r => Mathf.Clamp01(1f - Mathf.Abs(r - 0.78f) / 0.14f) + Mathf.Clamp01(0.75f - r) * 0.25f);

            var flash = CreateMaterial("Flash", "Universal Render Pipeline/Unlit", Color.white, null);
            var particle = CreateMaterial("Particle", "Sprites/Default", Color.white, dot);
            var ringMaterial = CreateMaterial("Ring", "Sprites/Default", Color.white, ring);
            var sounds = SfxSynth.CreateAll("Assets/Audio/Sfx");

            var abilities = CreateAbilities();
            var units = CreateUnits(abilities);

            var config = LoadOrCreate<BattleConfig>($"{DataDir}/BattleConfig.asset");
            config.heroes = new[] { units["Knight"], units["Archer"], units["Owl"] };
            config.encounter = CreateEncounter("Forest1", "Leśna polana",
                new[] { units["Grunt"], units["Shaman"], units["Grunt"] },
                new[] { units["Grunt"], units["Brute"], units["Grunt"] },
                new[] { units["Brute"], units["Shaman"], units["Brute"] });
            config.arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EnvironmentDir}/Arrow.fbx");
            config.flashMaterial = flash;
            config.particleMaterial = particle;
            config.ringMaterial = ringMaterial;
            config.sounds = sounds;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            BuildScene();
            Debug.Log("Featherguard: battle scene rebuilt.");
        }

        static EncounterDefinition CreateEncounter(string id, string name, params UnitDefinition[][] waves)
        {
            var encounter = LoadOrCreate<EncounterDefinition>($"{EncounterDir}/{id}.asset");
            encounter.displayName = name;
            encounter.waves = waves.Select(w => new EncounterDefinition.Wave { enemies = w }).ToArray();
            EditorUtility.SetDirty(encounter);
            return encounter;
        }

        static StatusApplication Status(StatusType type, int turns = 1, int potency = 0, float chance = 1f, bool onSelf = false) =>
            new() { type = type, turns = turns, potency = potency, chance = chance, onSelf = onSelf };

        static Dictionary<string, AbilityDefinition> CreateAbilities()
        {
            var list = new Dictionary<string, AbilityDefinition>();

            void Add(string id, string name, AbilityEffect effect, AbilityTarget target, AbilityDelivery delivery, string state,
                int power, Color color, SfxId cast, SfxId impact, int hits = 1, bool heavy = false, params StatusApplication[] statuses)
            {
                var a = LoadOrCreate<AbilityDefinition>($"{AbilityDir}/{id}.asset");
                a.displayName = name;
                a.effect = effect;
                a.target = target;
                a.delivery = delivery;
                a.animationState = state;
                a.power = power;
                a.hits = hits;
                a.heavy = heavy;
                a.statuses = statuses;
                a.fxColor = color;
                a.castSound = cast;
                a.impactSound = impact;
                EditorUtility.SetDirty(a);
                list[id] = a;
            }

            var spark = new Color(1f, 0.62f, 0.15f);
            Add("KnightAttack", "Cięcie", AbilityEffect.Damage, AbilityTarget.Enemy, AbilityDelivery.Melee, "Attack", 30, spark, SfxId.Slash, SfxId.Hit);
            Add("KnightSupport", "Osłona", AbilityEffect.Status, AbilityTarget.Ally, AbilityDelivery.Instant, "Cast", 0, new Color(0.45f, 0.75f, 1f), SfxId.None, SfxId.Shield, statuses: new[] { Status(StatusType.Shield), Status(StatusType.Taunt, onSelf: true) });
            Add("KnightSpecial", "Grom Tarczy", AbilityEffect.Damage, AbilityTarget.Enemy, AbilityDelivery.Melee, "Attack", 85, new Color(1f, 0.7f, 0.25f), SfxId.Slash, SfxId.Smash, heavy: true, statuses: new[] { Status(StatusType.Stun) });
            Add("ArcherAttack", "Podwójny strzał", AbilityEffect.Damage, AbilityTarget.Enemy, AbilityDelivery.Arrow, "Attack", 14, spark, SfxId.Arrow, SfxId.Hit, hits: 2, statuses: new[] { Status(StatusType.Poison, 2, 5, 0.3f) });
            Add("ArcherSupport", "Okrzyk bojowy", AbilityEffect.Status, AbilityTarget.Ally, AbilityDelivery.Instant, "Cast", 0, new Color(1f, 0.6f, 0.2f), SfxId.None, SfxId.Buff, statuses: new[] { Status(StatusType.Empower) });
            Add("ArcherSpecial", "Deszcz strzał", AbilityEffect.Damage, AbilityTarget.AllEnemies, AbilityDelivery.Arrow, "Attack", 24, spark, SfxId.Arrow, SfxId.Hit, hits: 2, heavy: true, statuses: new[] { Status(StatusType.Poison, 2, 6) });
            Add("OwlAttack", "Iskra", AbilityEffect.Damage, AbilityTarget.Enemy, AbilityDelivery.Magic, "Attack", 20, new Color(0.4f, 1f, 0.6f), SfxId.Magic, SfxId.Hit, statuses: new[] { Status(StatusType.Weaken, chance: 0.35f) });
            Add("OwlSupport", "Leczenie", AbilityEffect.Heal, AbilityTarget.Ally, AbilityDelivery.Instant, "Cast", 40, new Color(0.45f, 1f, 0.5f), SfxId.None, SfxId.Heal);
            Add("OwlSpecial", "Odrodzenie lasu", AbilityEffect.Heal, AbilityTarget.AllAllies, AbilityDelivery.Instant, "Cast", 60, new Color(0.5f, 1f, 0.6f), SfxId.None, SfxId.Heal, heavy: true);
            Add("GruntAttack", "Pałka", AbilityEffect.Damage, AbilityTarget.Enemy, AbilityDelivery.Melee, "Attack", 15, spark, SfxId.Slash, SfxId.Hit);
            Add("ShamanAttack", "Klątwa", AbilityEffect.Damage, AbilityTarget.Enemy, AbilityDelivery.Magic, "Attack", 12, new Color(0.8f, 0.4f, 1f), SfxId.Magic, SfxId.Hit, statuses: new[] { Status(StatusType.Weaken, chance: 0.5f) });
            Add("ShamanSupport", "Mroczne leczenie", AbilityEffect.Heal, AbilityTarget.Ally, AbilityDelivery.Instant, "Cast", 25, new Color(0.8f, 0.45f, 1f), SfxId.None, SfxId.Heal);
            Add("BruteAttack", "Miażdżenie", AbilityEffect.Damage, AbilityTarget.Enemy, AbilityDelivery.Melee, "Attack", 26, spark, SfxId.Slash, SfxId.Smash, heavy: true, statuses: new[] { Status(StatusType.Stun, chance: 0.25f) });
            return list;
        }

        static Dictionary<string, UnitDefinition> CreateUnits(Dictionary<string, AbilityDefinition> a)
        {
            var units = new Dictionary<string, UnitDefinition>();

            void Add(string id, string name, Team team, int health, float height, float scale, string attack, string support, string special)
            {
                var u = LoadOrCreate<UnitDefinition>($"{UnitDir}/{id}.asset");
                string fbx = $"{CharacterDir}/{id}.fbx";
                u.displayName = name;
                u.team = team;
                u.model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
                u.animator = CreateController(id, fbx);
                u.maxHealth = health;
                u.height = height;
                u.modelScale = scale;
                u.attack = a[attack];
                u.support = support != null ? a[support] : null;
                u.special = special != null ? a[special] : null;
                EditorUtility.SetDirty(u);
                units[id] = u;
            }

            Add("Knight", "Rudzik", Team.Heroes, 150, 1.4f, 1f, "KnightAttack", "KnightSupport", "KnightSpecial");
            Add("Archer", "Sójka", Team.Heroes, 105, 1.4f, 1f, "ArcherAttack", "ArcherSupport", "ArcherSpecial");
            Add("Owl", "Puszczyk", Team.Heroes, 115, 1.4f, 1f, "OwlAttack", "OwlSupport", "OwlSpecial");
            Add("Grunt", "Zbir", Team.Enemies, 80, 1.5f, 1f, "GruntAttack", null, null);
            Add("Shaman", "Szaman", Team.Enemies, 70, 1.5f, 1f, "ShamanAttack", "ShamanSupport", null);
            Add("Brute", "Osiłek", Team.Enemies, 170, 1.5f, 1.4f, "BruteAttack", null, null);
            return units;
        }

        static AnimatorController CreateController(string id, string fbxPath)
        {
            string path = $"{AnimationDir}/{id}.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path) ?? AnimatorController.CreateAnimatorControllerAtPath(path);
            var machine = controller.layers[0].stateMachine;
            foreach (var child in machine.states.ToArray())
                machine.RemoveState(child.state);

            var clips = AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__"))
                .ToDictionary(c => c.name);

            var states = new Dictionary<string, AnimatorState>();
            string[] names = { "Idle", "Attack", "Cast", "Hit", "Death", "Victory" };
            for (int i = 0; i < names.Length; i++)
            {
                var state = machine.AddState(names[i], new Vector3(300f, 60f * i, 0f));
                state.motion = clips[names[i]];
                states[names[i]] = state;
            }
            machine.defaultState = states["Idle"];

            foreach (var name in new[] { "Attack", "Cast", "Hit" })
            {
                var transition = states[name].AddTransition(states["Idle"]);
                transition.hasExitTime = true;
                transition.exitTime = 0.95f;
                transition.duration = 0.1f;
                transition.hasFixedDuration = true;
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }

        static void BuildScene()
        {
            var postProfile = CreatePostProfile();
            var config = AssetDatabase.LoadAssetAtPath<BattleConfig>($"{DataDir}/BattleConfig.asset");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

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

            var rig = new GameObject("CameraRig").transform;
            var cameraPosition = new Vector3(0f, 3.2f, -11.5f);
            rig.SetPositionAndRotation(cameraPosition, Quaternion.LookRotation(new Vector3(0f, 1f, 0.3f) - cameraPosition));
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            cameraObject.transform.SetParent(rig, false);
            var camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = 32f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.56f, 0.78f, 0.98f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 120f;
            var cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var shake = cameraObject.AddComponent<CameraShake>();

            var volume = new GameObject("PostFX").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = postProfile;

            var arena = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{EnvironmentDir}/Arena.fbx"));
            arena.transform.position = Vector3.zero;

            var battle = new GameObject("Battle").AddComponent<BattleManager>();
            var serialized = new SerializedObject(battle);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.FindProperty("battleCamera").objectReferenceValue = camera;
            serialized.FindProperty("cameraShake").objectReferenceValue = shake;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static VolumeProfile CreatePostProfile()
        {
            const string path = "Assets/Settings/BattlePostFX.asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.7f);
            bloom.threshold.Override(1f);
            var color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(28f);
            color.contrast.Override(16f);
            color.postExposure.Override(-0.05f);
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.22f);
            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.Neutral);

            foreach (var component in profile.components)
            {
                component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            AssetDatabase.SaveAssets();
            return profile;
        }

        static Texture2D CreateTexture(string path, System.Func<float, float> alpha)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, r >= 1f ? 0f : Mathf.Clamp01(alpha(r))));
            }
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Material CreateMaterial(string name, string shader, Color color, Texture texture)
        {
            string path = $"{MaterialDir}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(shader));
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = Shader.Find(shader);
            material.color = color;
            if (texture != null)
                material.mainTexture = texture;
            EditorUtility.SetDirty(material);
            return material;
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}

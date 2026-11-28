using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Featherguard.EditorTools
{
    public static class CampfireEffect
    {
        const string Shader = "Universal Render Pipeline/Particles/Unlit";
        const int FlipbookTiles = 8;
        const string SparkSource = ContentPaths.Materials + "/Particle.mat";

        static readonly Color FlameTint = new(2.4f, 1.5f, 0.9f, 1f);
        static readonly Color SparkTint = new(3f, 1.4f, 0.4f, 1f);

        public static Transform Create(Transform parent, string flipbookPath, string materialFolder)
        {
            var root = new GameObject("Fire").transform;
            root.SetParent(parent, false);
            Flames(root, Additive($"{materialFolder}/Flame.mat", AssetDatabase.LoadAssetAtPath<Texture2D>(flipbookPath), FlameTint));
            var dot = AssetDatabase.LoadAssetAtPath<Material>(SparkSource);
            Sparks(root, Additive($"{materialFolder}/Spark.mat", dot != null ? dot.mainTexture : null, SparkTint));
            return root;
        }

        static void Flames(Transform root, Material material)
        {
            var system = Emitter(root, "Flames", material);
            var main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.85f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.55f, 0.85f);
            main.startRotation = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
            main.maxParticles = 24;
            var emission = system.emission;
            emission.rateOverTime = 16f;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 6f;
            shape.radius = 0.18f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            var sheet = system.textureSheetAnimation;
            sheet.enabled = true;
            sheet.numTilesX = FlipbookTiles;
            sheet.numTilesY = FlipbookTiles;
            sheet.startFrame = new ParticleSystem.MinMaxCurve(0f, FlipbookTiles * FlipbookTiles - 1);
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.7f), new Keyframe(0.3f, 1f), new Keyframe(1f, 0.35f)));
            Fade(system, 0.15f, 0.6f);
        }

        static void Sparks(Transform root, Material material)
        {
            var system = Emitter(root, "Sparks", material);
            var main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.07f);
            main.maxParticles = 30;
            var emission = system.emission;
            emission.rateOverTime = 7f;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 10f;
            shape.radius = 0.25f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            var noise = system.noise;
            noise.enabled = true;
            noise.strength = 0.2f;
            noise.frequency = 0.8f;
            Fade(system, 0.05f, 0.4f);
        }

        static ParticleSystem Emitter(Transform root, string name, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            var system = go.AddComponent<ParticleSystem>();
            var main = system.main;
            main.loop = true;
            main.prewarm = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return system;
        }

        static void Fade(ParticleSystem system, float fadeIn, float fadeOutFrom)
        {
            var color = system.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, fadeIn), new GradientAlphaKey(1f, fadeOutFrom), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;
        }

        static Material Additive(string path, Texture texture, Color tint)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Seeder.EnsureFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));
                material = new Material(UnityEngine.Shader.Find(Shader));
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = UnityEngine.Shader.Find(Shader);
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", tint);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 2f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.One);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetOverrideTag("RenderType", "Transparent");
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}

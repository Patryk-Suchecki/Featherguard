using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Featherguard.EditorTools
{
    public static class CampfireEffect
    {
        const string Shader = "Universal Render Pipeline/Particles/Unlit";
        const string FlameShader = "Featherguard/Toon Flame";
        const float BaseHeight = 0.04f;
        const string SparkSource = ContentPaths.Materials + "/Particle.mat";

        static readonly (Vector2 Offset, Vector2 Size)[] Tongues =
        {
            (new Vector2(0f, 0f), new Vector2(0.4f, 0.56f)),
            (new Vector2(-0.1f, 0.06f), new Vector2(0.3f, 0.38f)),
            (new Vector2(0.1f, 0.03f), new Vector2(0.32f, 0.44f)),
        };
        static readonly Color SparkTint = new(3f, 1.4f, 0.4f, 1f);

        public static Transform Create(Transform parent, string materialFolder)
        {
            var root = new GameObject("Fire").transform;
            root.SetParent(parent, false);
            Flames(root, FlameMaterial($"{materialFolder}/Flame.mat"));
            var dot = AssetDatabase.LoadAssetAtPath<Material>(SparkSource);
            Sparks(root, Additive($"{materialFolder}/Spark.mat", dot != null ? dot.mainTexture : null, SparkTint));
            return root;
        }

        static void Flames(Transform root, Material material)
        {
            foreach (var (offset, size) in Tongues)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Object.DestroyImmediate(quad.GetComponent<Collider>());
                quad.name = "Flame";
                quad.transform.SetParent(root, false);
                quad.transform.localPosition = new Vector3(offset.x, BaseHeight + size.y * 0.5f, offset.y);
                quad.transform.localScale = new Vector3(size.x, size.y, 1f);
                var renderer = quad.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        static Material FlameMaterial(string path)
        {
            var shader = UnityEngine.Shader.Find(FlameShader);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            EditorUtility.SetDirty(material);
            return material;
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

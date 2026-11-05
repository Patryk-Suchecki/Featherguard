using System.Collections.Generic;
using System.IO;
using System.Linq;
using Featherguard.UI;
using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public sealed class PresentationAssets
    {
        public Material Flash;
        public Material Particle;
        public Material Ring;
        public AudioClip[] Sounds;
        public UiSprites Ui;
        public Dictionary<string, Sprite> Icons;

        public Sprite Icon(string name) => Icons.TryGetValue(name, out var sprite) ? sprite : null;
        public Sprite Portrait(string model) => AssetDatabase.LoadAssetAtPath<Sprite>($"{ContentPaths.Portraits}/{model}.png");
    }

    public static class PresentationBuilder
    {
        public static PresentationAssets Build(Seeder seeder)
        {
            Seeder.EnsureFolder(ContentPaths.Vfx);
            Seeder.EnsureFolder(ContentPaths.Materials);
            var dot = CreateRadialTexture($"{ContentPaths.Vfx}/SoftDot.png", r => Mathf.Pow(Mathf.Clamp01(1f - r), 2f));
            var ring = CreateRadialTexture($"{ContentPaths.Vfx}/Ring.png", r => Mathf.Clamp01(1f - Mathf.Abs(r - 0.78f) / 0.14f) + Mathf.Clamp01(0.75f - r) * 0.25f);

            var icons = IconFactory.CreateAll(ContentPaths.Icons);
            var sprites = icons.Values.ToList();
            foreach (var path in Directory.GetFiles(ContentPaths.Portraits, "*.png"))
                sprites.Add(IconFactory.ImportSprite(path.Replace('\\', '/'), Vector4.zero));

            return new PresentationAssets
            {
                Flash = CreateMaterial("Flash", "Universal Render Pipeline/Unlit", null),
                Particle = CreateMaterial("Particle", "Sprites/Default", dot),
                Ring = CreateMaterial("Ring", "Sprites/Default", ring),
                Sounds = SfxSynth.CreateAll(ContentPaths.Sounds),
                Ui = seeder.Seed<UiSprites>($"{ContentPaths.Data}/UiSprites.asset", null, ui => ui.sprites = sprites.ToArray()),
                Icons = icons,
            };
        }

        static Texture2D CreateRadialTexture(string path, System.Func<float, float> alpha)
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
            bool changed = GeneratedFiles.Write(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            if (changed) AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Material CreateMaterial(string name, string shader, Texture texture)
        {
            string path = $"{ContentPaths.Materials}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(shader));
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = Shader.Find(shader);
            material.color = Color.white;
            if (texture != null)
                material.mainTexture = texture;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}

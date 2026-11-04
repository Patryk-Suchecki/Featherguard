using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class MobileSettings
    {
        const string Company = "Patryk";
        const string Product = "Featherguard";
        const string Identifier = "com.patryk.featherguard";
        const int IconSize = 512;

        public static void Apply()
        {
            PlayerSettings.companyName = Company;
            PlayerSettings.productName = Product;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, Identifier);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, Identifier);
            PlayerSettings.runInBackground = false;
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { CreateAppIcon($"{ContentPaths.Portraits}/Knight.png") }, IconKind.Any);
            AssetDatabase.SaveAssets();
        }

        static Texture2D CreateAppIcon(string portraitPath)
        {
            var portrait = new Texture2D(2, 2);
            portrait.LoadImage(File.ReadAllBytes(portraitPath));
            var icon = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
            for (int y = 0; y < IconSize; y++)
            for (int x = 0; x < IconSize; x++)
            {
                float u = (x + 0.5f) / IconSize, v = (y + 0.5f) / IconSize;
                var background = Color.Lerp(new Color(0.95f, 0.45f, 0.12f), new Color(1f, 0.82f, 0.25f), v);
                float vignette = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.55f)) * 0.9f);
                background = Color.Lerp(background * 0.7f, background, vignette);
                var pixel = portrait.GetPixelBilinear(u, v);
                var color = Color.Lerp(background, pixel, pixel.a);
                color.a = 1f;
                icon.SetPixel(x, y, color);
            }
            File.WriteAllBytes(ContentPaths.AppIcon, icon.EncodeToPNG());
            Object.DestroyImmediate(icon);
            Object.DestroyImmediate(portrait);
            AssetDatabase.ImportAsset(ContentPaths.AppIcon, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.AppIcon);
        }
    }
}

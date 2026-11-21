using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public sealed class NightPalette
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
        public readonly Material Ash = Lit("Ash", new Color(0.2f, 0.2f, 0.22f));
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

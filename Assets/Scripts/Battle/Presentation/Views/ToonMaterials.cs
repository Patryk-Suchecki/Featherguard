using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class ToonMaterials : System.IDisposable
    {
        readonly Material toonMaterial;
        readonly Dictionary<Material, Material> variants = new();

        public ToonMaterials(Material toonMaterial)
        {
            this.toonMaterial = toonMaterial;
        }

        public void Apply(GameObject model)
        {
            if (toonMaterial == null) return;
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials = renderer.sharedMaterials.Select(VariantOf).ToArray();
        }

        public void Dispose()
        {
            foreach (var variant in variants.Values)
                Object.Destroy(variant);
            variants.Clear();
        }

        Material VariantOf(Material original)
        {
            if (original == null) return toonMaterial;
            if (variants.TryGetValue(original, out var variant)) return variant;

            variant = new Material(toonMaterial) { name = $"{original.name} (Toon)" };
            variant.SetTexture("_BaseMap", original.HasProperty("_BaseMap") ? original.GetTexture("_BaseMap") : original.mainTexture);
            variant.SetColor("_BaseColor", original.HasProperty("_BaseColor") ? original.GetColor("_BaseColor") : original.color);
            variants[original] = variant;
            return variant;
        }
    }
}

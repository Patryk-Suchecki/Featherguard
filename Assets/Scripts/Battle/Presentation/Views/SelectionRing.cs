using UnityEngine;
using UnityEngine.Rendering;

namespace Featherguard.Battle.Presentation
{
    public class SelectionRing : MonoBehaviour
    {
        const float StrongScale = 1.3f;
        const float BaseScale = 0.92f;
        const int BaseTextureSize = 64;
        static readonly Color Shade = new(0f, 0f, 0f, 0.35f);
        static Texture2D baseTexture;

        MeshRenderer ring;
        MeshRenderer shade;
        MeshRenderer disc;
        Material material;
        Material shadeMaterial;
        Material discMaterial;
        bool strong;

        public static SelectionRing Create(Transform parent, float scale, Material template, Color baseColor)
        {
            var root = new GameObject("Base");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.up * 0.03f;
            root.transform.localScale = Vector3.one * scale;
            var selection = root.AddComponent<SelectionRing>();

            selection.shade = Quad("Shade", root.transform, BaseScale * 1.08f);
            selection.shadeMaterial = new Material(template) { mainTexture = BaseTexture(), color = Shade, renderQueue = template.renderQueue - 2 };
            selection.shade.sharedMaterial = selection.shadeMaterial;

            selection.disc = Quad("Disc", root.transform, BaseScale);
            selection.discMaterial = new Material(template) { mainTexture = BaseTexture(), color = baseColor, renderQueue = template.renderQueue - 1 };
            selection.disc.sharedMaterial = selection.discMaterial;

            selection.ring = Quad("Ring", root.transform, 1f);
            selection.material = new Material(template);
            selection.ring.sharedMaterial = selection.material;

            selection.Show(null);
            return selection;
        }

        public void Show(Color? color, bool emphasised = false)
        {
            ring.enabled = color.HasValue;
            strong = emphasised;
            if (color.HasValue)
                material.color = color.Value;
        }

        public void Hide()
        {
            Show(null);
            shade.enabled = false;
            disc.enabled = false;
        }

        void Update()
        {
            float pulse = !ring.enabled ? 1f
                : strong ? 1f + Mathf.Sin(Time.unscaledTime * 12f) * 0.08f
                : 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.05f;
            ring.transform.localScale = Vector3.one * (strong ? StrongScale : 1f) * pulse;
        }

        void OnDestroy()
        {
            if (material != null) Destroy(material);
            if (shadeMaterial != null) Destroy(shadeMaterial);
            if (discMaterial != null) Destroy(discMaterial);
        }

        static MeshRenderer Quad(string name, Transform parent, float scale)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            Destroy(quad.GetComponent<Collider>());
            quad.transform.SetParent(parent, false);
            quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            quad.transform.localScale = Vector3.one * scale;
            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return renderer;
        }

        static Texture2D BaseTexture()
        {
            if (baseTexture != null) return baseTexture;
            baseTexture = new Texture2D(BaseTextureSize, BaseTextureSize, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "UnitBase" };
            float half = BaseTextureSize * 0.5f;
            for (int y = 0; y < BaseTextureSize; y++)
            for (int x = 0; x < BaseTextureSize; x++)
            {
                float distance = new Vector2(x + 0.5f - half, y + 0.5f - half).magnitude / half;
                float alpha = Mathf.Clamp01((1f - distance) / 0.12f);
                baseTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
            baseTexture.Apply();
            return baseTexture;
        }
    }
}

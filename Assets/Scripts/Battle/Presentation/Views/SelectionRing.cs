using UnityEngine;
using UnityEngine.Rendering;

namespace Featherguard.Battle.Presentation
{
    public class SelectionRing : MonoBehaviour
    {
        const float StrongScale = 1.3f;
        const float BaseScale = 0.92f;
        const int BaseTextureSize = 64;
        static readonly Color BaseColor = new(0f, 0f, 0f, 0.45f);
        static Texture2D baseTexture;

        MeshRenderer ring;
        MeshRenderer disc;
        Material material;
        Material discMaterial;
        float baseScale;
        bool strong;

        public static SelectionRing Create(Transform parent, float scale, Material template)
        {
            var quad = Quad("Ring", parent);
            var selection = quad.AddComponent<SelectionRing>();
            selection.ring = quad.GetComponent<MeshRenderer>();
            selection.material = new Material(template);
            selection.ring.sharedMaterial = selection.material;

            var disc = Quad("Base", quad.transform);
            disc.transform.localPosition = Vector3.zero;
            disc.transform.localRotation = Quaternion.identity;
            disc.transform.localScale = Vector3.one * BaseScale;
            selection.disc = disc.GetComponent<MeshRenderer>();
            selection.discMaterial = new Material(template) { mainTexture = BaseTexture(), color = BaseColor, renderQueue = template.renderQueue - 1 };
            selection.disc.sharedMaterial = selection.discMaterial;

            selection.baseScale = scale;
            selection.Show(null);
            return selection;
        }

        public void Show(Color? color, bool emphasised = false)
        {
            ring.enabled = color.HasValue;
            disc.enabled = color.HasValue;
            strong = emphasised;
            if (color.HasValue)
                material.color = color.Value;
        }

        void Update()
        {
            if (!ring.enabled) return;
            float pulse = strong ? 1f + Mathf.Sin(Time.unscaledTime * 12f) * 0.08f : 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.05f;
            transform.localScale = Vector3.one * baseScale * (strong ? StrongScale : 1f) * pulse;
        }

        void OnDestroy()
        {
            if (material != null) Destroy(material);
            if (discMaterial != null) Destroy(discMaterial);
        }

        static GameObject Quad(string name, Transform parent)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            Destroy(quad.GetComponent<Collider>());
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = Vector3.up * 0.03f;
            quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            quad.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            return quad;
        }

        static Texture2D BaseTexture()
        {
            if (baseTexture != null) return baseTexture;
            baseTexture = new Texture2D(BaseTextureSize, BaseTextureSize, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "SelectionBase" };
            float half = BaseTextureSize * 0.5f;
            for (int y = 0; y < BaseTextureSize; y++)
            for (int x = 0; x < BaseTextureSize; x++)
            {
                float distance = new Vector2(x + 0.5f - half, y + 0.5f - half).magnitude / half;
                float alpha = Mathf.Clamp01((1f - distance) / 0.25f);
                baseTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
            baseTexture.Apply();
            return baseTexture;
        }
    }
}

using UnityEngine;
using UnityEngine.Rendering;

namespace Featherguard.Battle.Presentation
{
    public class SelectionRing : MonoBehaviour
    {
        const float HighlightScale = 1.18f;
        const float StrongScale = 1.3f;

        MeshRenderer baseRing;
        MeshRenderer ring;
        Material baseMaterial;
        Material material;
        bool strong;

        public static SelectionRing Create(Transform parent, float scale, Material template, Color baseColor)
        {
            var root = new GameObject("Base");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.up * 0.03f;
            root.transform.localScale = Vector3.one * scale;
            var selection = root.AddComponent<SelectionRing>();

            selection.baseRing = Quad("BaseRing", root.transform);
            selection.baseMaterial = new Material(template) { color = baseColor, renderQueue = template.renderQueue - 1 };
            selection.baseRing.sharedMaterial = selection.baseMaterial;

            selection.ring = Quad("Highlight", root.transform);
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

        public void ShowBase(bool visible) => baseRing.enabled = visible;

        public void Hide()
        {
            Show(null);
            ShowBase(false);
        }

        void Update()
        {
            if (!ring.enabled) return;
            float pulse = strong ? 1f + Mathf.Sin(Time.unscaledTime * 12f) * 0.08f : 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.05f;
            ring.transform.localScale = Vector3.one * HighlightScale * (strong ? StrongScale : 1f) * pulse;
        }

        void OnDestroy()
        {
            if (material != null) Destroy(material);
            if (baseMaterial != null) Destroy(baseMaterial);
        }

        static MeshRenderer Quad(string name, Transform parent)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            Destroy(quad.GetComponent<Collider>());
            quad.transform.SetParent(parent, false);
            quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return renderer;
        }
    }
}

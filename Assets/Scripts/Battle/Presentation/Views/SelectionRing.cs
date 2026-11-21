using UnityEngine;
using UnityEngine.Rendering;

namespace Featherguard.Battle.Presentation
{
    public class SelectionRing : MonoBehaviour
    {
        const float StrongScale = 1.3f;

        MeshRenderer ring;
        Material material;
        float baseScale;
        bool strong;

        public static SelectionRing Create(Transform parent, float scale, Material template)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Ring";
            Destroy(quad.GetComponent<Collider>());
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = Vector3.up * 0.03f;
            quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var selection = quad.AddComponent<SelectionRing>();
            selection.ring = quad.GetComponent<MeshRenderer>();
            selection.ring.shadowCastingMode = ShadowCastingMode.Off;
            selection.material = new Material(template);
            selection.ring.sharedMaterial = selection.material;
            selection.baseScale = scale;
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

        void Update()
        {
            if (!ring.enabled) return;
            float pulse = strong ? 1f + Mathf.Sin(Time.unscaledTime * 12f) * 0.08f : 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.05f;
            transform.localScale = Vector3.one * baseScale * (strong ? StrongScale : 1f) * pulse;
        }

        void OnDestroy()
        {
            if (material != null) Destroy(material);
        }
    }
}

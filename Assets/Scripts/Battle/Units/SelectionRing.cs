using UnityEngine;
using UnityEngine.Rendering;

namespace Featherguard.Battle
{
    public class SelectionRing : MonoBehaviour
    {
        MeshRenderer ring;
        Material material;
        float baseScale;

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

        public void Show(Color? color)
        {
            ring.enabled = color.HasValue;
            if (color.HasValue)
                material.color = color.Value;
        }

        void Update()
        {
            if (!ring.enabled) return;
            transform.localScale = Vector3.one * baseScale * (1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.05f);
        }

        void OnDestroy()
        {
            if (material != null) Destroy(material);
        }
    }
}

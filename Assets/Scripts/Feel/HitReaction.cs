using System.Collections;
using System.Linq;
using UnityEngine;

namespace Featherguard.Feel
{
    public class HitReaction : MonoBehaviour
    {
        Renderer[] renderers;
        Material[][] originals;
        Material[][] flashes;
        Transform model;
        Vector3 modelHome;
        Coroutine flashRoutine;
        Coroutine knockbackRoutine;

        public void Init(Material flashMaterial, Transform modelRoot)
        {
            model = modelRoot;
            modelHome = model.localPosition;
            renderers = model.GetComponentsInChildren<Renderer>();
            originals = renderers.Select(r => r.sharedMaterials).ToArray();
            flashes = originals.Select(set => Enumerable.Repeat(flashMaterial, set.Length).ToArray()).ToArray();
        }

        public void Flash(float duration = 0.07f)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(duration));
        }

        public void Knockback(Vector3 direction, float distance)
        {
            if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);
            knockbackRoutine = StartCoroutine(KnockbackRoutine(direction, distance));
        }

        IEnumerator FlashRoutine(float duration)
        {
            Apply(flashes);
            yield return new WaitForSecondsRealtime(duration);
            Apply(originals);
            flashRoutine = null;
        }

        IEnumerator KnockbackRoutine(Vector3 direction, float distance)
        {
            const float outTime = 0.1f;
            const float backTime = 0.28f;
            Vector3 offset = transform.InverseTransformDirection(direction.normalized) * distance;
            offset.y = 0f;

            for (float t = 0f; t < outTime; t += Time.deltaTime)
            {
                float eased = 1f - Mathf.Pow(1f - t / outTime, 3f);
                model.localPosition = modelHome + offset * eased;
                yield return null;
            }
            for (float t = 0f; t < backTime; t += Time.deltaTime)
            {
                model.localPosition = modelHome + offset * (1f - Mathf.SmoothStep(0f, 1f, t / backTime));
                yield return null;
            }
            model.localPosition = modelHome;
            knockbackRoutine = null;
        }

        void Apply(Material[][] sets)
        {
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].sharedMaterials = sets[i];
        }
    }
}

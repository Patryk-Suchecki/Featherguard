using System.Collections;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public static class UnitMotion
    {
        public static IEnumerator HopTo(Transform target, Vector3 destination, float duration, float height)
        {
            Vector3 start = target.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
            {
                float eased = Mathf.SmoothStep(0f, 1f, t);
                target.position = Vector3.Lerp(start, destination, eased) + Vector3.up * height * 4f * t * (1f - t);
                yield return null;
            }
            target.position = destination;
        }

        public static IEnumerator SinkAndDestroy(Transform target, float duration = 0.6f, float depth = 1.2f)
        {
            Vector3 start = target.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
            {
                target.position = start + Vector3.down * t * depth;
                yield return null;
            }
            Object.Destroy(target.gameObject);
        }
    }
}

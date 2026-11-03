using UnityEngine;

namespace Featherguard.Feel
{
    public class CameraShake : MonoBehaviour
    {
        Vector3 origin;
        float amplitude;
        float duration;
        float until;

        void Awake() => origin = transform.localPosition;

        public void Shake(float strength, float seconds)
        {
            if (Time.unscaledTime < until && strength < amplitude) return;
            amplitude = strength;
            duration = seconds;
            until = Time.unscaledTime + seconds;
        }

        void LateUpdate()
        {
            float left = until - Time.unscaledTime;
            if (left <= 0f)
            {
                transform.localPosition = origin;
                return;
            }
            float strength = amplitude * (left / duration);
            transform.localPosition = origin + (Vector3)(Random.insideUnitCircle * strength);
        }
    }
}

using UnityEngine;

namespace Featherguard.UI
{
    public class Pulser : MonoBehaviour
    {
        public float speed = 7f;
        public float amount = 0.05f;

        public static void Attach(Component target, float speed = 7f, float amount = 0.05f)
        {
            var pulser = target.gameObject.AddComponent<Pulser>();
            pulser.speed = speed;
            pulser.amount = amount;
        }

        void Update() => transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * speed) * amount);
    }
}

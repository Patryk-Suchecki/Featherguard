using UnityEngine;

namespace Featherguard.UI
{
    public class FallingCoin : MonoBehaviour
    {
        const float Gravity = 2600f;
        const float Lifetime = 2.5f;

        Vector2 velocity;
        float spin;
        float age;

        public void Launch(Vector2 initial)
        {
            velocity = initial;
            spin = Random.Range(-720f, 720f);
        }

        void Update()
        {
            age += Time.unscaledDeltaTime;
            velocity += Vector2.down * Gravity * Time.unscaledDeltaTime;
            var rect = (RectTransform)transform;
            rect.anchoredPosition += velocity * Time.unscaledDeltaTime;
            rect.Rotate(0f, 0f, spin * Time.unscaledDeltaTime);
            rect.localScale = new Vector3(Mathf.Abs(Mathf.Cos(age * 9f)) * 0.8f + 0.2f, 1f, 1f);
            if (age > Lifetime) Destroy(gameObject);
        }
    }
}

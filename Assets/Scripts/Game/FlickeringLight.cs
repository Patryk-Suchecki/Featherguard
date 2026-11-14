using UnityEngine;

namespace Featherguard.Game
{
    [RequireComponent(typeof(Light))]
    public class FlickeringLight : MonoBehaviour
    {
        [SerializeField, Min(0f)] float baseIntensity = 6f;
        [SerializeField, Range(0f, 1f)] float flicker = 0.35f;
        [SerializeField, Min(0f)] float speed = 6f;
        [SerializeField] Transform flame;

        Light fireLight;
        Vector3 flameScale;
        float seed;

        void Awake()
        {
            fireLight = GetComponent<Light>();
            if (flame != null) flameScale = flame.localScale;
            seed = transform.position.x * 13.7f + transform.position.z * 7.3f;
        }

        void Update()
        {
            float noise = Mathf.PerlinNoise(seed, Time.time * speed);
            fireLight.intensity = baseIntensity * (1f - flicker + flicker * 2f * noise);
            if (flame != null)
                flame.localScale = new Vector3(flameScale.x, flameScale.y * (0.85f + 0.3f * noise), flameScale.z);
        }
    }
}

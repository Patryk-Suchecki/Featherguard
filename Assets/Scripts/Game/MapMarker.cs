using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.Game
{
    public class MapMarker : MonoBehaviour
    {
        const float BobHeight = 0.12f;
        const float BobSpeed = 1.6f;

        Vector3 home;

        public ActivityDefinition Activity { get; private set; }

        public void Bind(ActivityDefinition activity)
        {
            Activity = activity;
            home = transform.localPosition;
        }

        void Update() => transform.localPosition = home + Vector3.up * (Mathf.Sin(Time.time * BobSpeed + home.x) * BobHeight);
    }
}

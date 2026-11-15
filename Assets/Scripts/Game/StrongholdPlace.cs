using System.Collections;
using UnityEngine;

namespace Featherguard.Game
{
    public enum StrongholdPlaceKind { Forge, WiseWomansHut, SacredOak, Tavern, Shop, Camp, Embark }

    public class StrongholdPlace : MonoBehaviour
    {
        const float PulseTime = 0.18f;
        const float PulseScale = 1.07f;

        [SerializeField] StrongholdPlaceKind kind;
        [SerializeField] string displayName;
        [SerializeField] Transform labelAnchor;

        Coroutine pulse;

        public StrongholdPlaceKind Kind => kind;
        public string DisplayName => displayName;
        public Transform LabelAnchor => labelAnchor != null ? labelAnchor : transform;

        public void Pulse()
        {
            if (pulse != null) StopCoroutine(pulse);
            pulse = StartCoroutine(PulseRoutine());
        }

        IEnumerator PulseRoutine()
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / PulseTime)
            {
                transform.localScale = Vector3.one * Mathf.Lerp(PulseScale, 1f, t);
                yield return null;
            }
            transform.localScale = Vector3.one;
        }
    }
}

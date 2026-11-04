using System;
using System.Collections;
using UnityEngine;

namespace Featherguard.Battle
{
    [RequireComponent(typeof(Animator))]
    public class UnitAnimator : MonoBehaviour
    {
        Animator animator;

        public event Action HitFrame;

        void Awake() => animator = GetComponent<Animator>();

        public void Play(string state, float fade = 0.06f) => animator.CrossFadeInFixedTime(state, fade);

        public void OnHitFrame() => HitFrame?.Invoke();

        public float ClipLength(string state)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
                if (clip.name == state)
                    return clip.length;
            return 0.8f;
        }

        public IEnumerator PlayUntilHitFrame(string state)
        {
            bool fired = false;
            void OnFired() => fired = true;

            HitFrame += OnFired;
            Play(state);
            float timeout = ClipLength(state) + 0.2f;
            for (float t = 0f; !fired && t < timeout; t += Time.deltaTime)
                yield return null;
            HitFrame -= OnFired;
        }
    }
}

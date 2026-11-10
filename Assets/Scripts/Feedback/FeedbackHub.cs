using System;
using System.Collections;
using Featherguard.Core;
using UnityEngine;

namespace Featherguard.Feedback
{
    [Serializable]
    public class FeedbackTuning
    {
        public float lightHitstop = 0.09f;
        public float heavyHitstop = 0.16f;
        public Vector2 lightShake = new(0.06f, 0.18f);
        public Vector2 heavyShake = new(0.16f, 0.32f);
        public int voices = 6;
    }

    public class FeedbackHub : MonoBehaviour, ICombatFeedback
    {
        AudioClip[] sounds;
        AudioSource[] voices;
        int nextVoice;
        CameraShake shake;
        Vfx vfx;
        FeedbackTuning tuning;
        float hitstopUntil;
        bool inHitstop;

        public static FeedbackHub Instance { get; private set; }

        public static void Ui(SfxId id, float volume = 1f)
        {
            if (Instance != null) Instance.Play(id, volume);
        }

        public void Init(AudioClip[] clips, CameraShake cameraShake, Material particleMaterial, FeedbackTuning settings)
        {
            Instance = this;
            sounds = clips;
            shake = cameraShake;
            tuning = settings ?? new FeedbackTuning();
            vfx = gameObject.AddComponent<Vfx>();
            vfx.Init(particleMaterial);
            voices = new AudioSource[Mathf.Max(1, tuning.voices)];
            for (int i = 0; i < voices.Length; i++)
            {
                voices[i] = gameObject.AddComponent<AudioSource>();
                voices[i].playOnAwake = false;
                voices[i].spatialBlend = 0f;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Play(SfxId id, float volume = 1f, float pitchJitter = 0.08f)
        {
            int index = (int)id;
            if (id == SfxId.None || sounds == null || index >= sounds.Length || sounds[index] == null) return;
            var voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            voice.pitch = 1f + UnityEngine.Random.Range(-pitchJitter, pitchJitter);
            voice.PlayOneShot(sounds[index], volume);
        }

        public void Impact(Vector3 point, Color color, bool heavy)
        {
            Hitstop(heavy ? tuning.heavyHitstop : tuning.lightHitstop);
            var s = heavy ? tuning.heavyShake : tuning.lightShake;
            shake.Shake(s.x, s.y);
            vfx.Flash(point, new Color(1f, 0.97f, 0.85f, 0.95f), heavy ? 2.2f : 1.4f);
            vfx.Burst(point, color, heavy ? 34 : 18, heavy ? 8f : 5.5f, heavy ? 0.42f : 0.3f);
        }

        public void Shake(float strength, float seconds) => shake.Shake(strength, seconds);
        public void Burst(Vector3 position, Color color, int count, float speed, float size) => vfx.Burst(position, color, count, speed, size);
        public void Rise(Vector3 position, Color color, float radius) => vfx.Rise(position, color, radius);
        public ParticleSystem Trail(Transform parent, Color color) => vfx.Trail(parent, color);
        public void Release(ParticleSystem trail) => vfx.Release(trail);

        public void Hitstop(float duration)
        {
            hitstopUntil = Mathf.Max(hitstopUntil, Time.unscaledTime + duration);
            if (!inHitstop) StartCoroutine(HitstopRoutine());
        }

        IEnumerator HitstopRoutine()
        {
            inHitstop = true;
            Time.timeScale = 0f;
            while (Time.unscaledTime < hitstopUntil)
                yield return null;
            Time.timeScale = GameClock.Scale;
            inHitstop = false;
        }
    }
}

using System.Collections;
using UnityEngine;

namespace Featherguard.Feel
{
    public class HitFeedback : MonoBehaviour
    {
        AudioClip[] sounds;
        AudioSource[] voices;
        int nextVoice;
        CameraShake shake;
        float hitstopUntil;
        bool inHitstop;

        public static HitFeedback Instance { get; private set; }

        public void Init(AudioClip[] clips, CameraShake cameraShake)
        {
            Instance = this;
            sounds = clips;
            shake = cameraShake;
            voices = new AudioSource[6];
            for (int i = 0; i < voices.Length; i++)
            {
                voices[i] = gameObject.AddComponent<AudioSource>();
                voices[i].playOnAwake = false;
                voices[i].spatialBlend = 0f;
            }
        }

        public void Play(SfxId id, float volume = 1f, float pitchJitter = 0.08f)
        {
            int index = (int)id;
            if (id == SfxId.None || sounds == null || index >= sounds.Length || sounds[index] == null) return;
            var voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            voice.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            voice.PlayOneShot(sounds[index], volume);
        }

        public void Impact(Vector3 point, Color color, bool heavy)
        {
            Hitstop(heavy ? 0.16f : 0.09f);
            shake.Shake(heavy ? 0.16f : 0.06f, heavy ? 0.32f : 0.18f);
            Vfx.Instance.Flash(point, new Color(1f, 0.97f, 0.85f, 0.95f), heavy ? 2.2f : 1.4f);
            Vfx.Instance.Burst(point, color, heavy ? 34 : 18, heavy ? 8f : 5.5f, heavy ? 0.42f : 0.3f);
        }

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
            Time.timeScale = 1f;
            inHitstop = false;
        }
    }
}

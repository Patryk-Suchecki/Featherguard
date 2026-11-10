using UnityEngine;

namespace Featherguard.Feedback
{
    public interface ISoundPlayer
    {
        void Play(SfxId id, float volume = 1f, float pitchJitter = 0.08f);
    }

    public interface ICombatFeedback : ISoundPlayer
    {
        void Impact(Vector3 point, Color color, bool heavy);
        void Shake(float strength, float seconds);
        void Burst(Vector3 position, Color color, int count, float speed, float size);
        void Rise(Vector3 position, Color color, float radius);
        ParticleSystem Trail(Transform parent, Color color);
        void Release(ParticleSystem trail);
    }
}

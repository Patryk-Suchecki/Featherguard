using UnityEngine;

namespace Featherguard.Feel
{
    public class Vfx : MonoBehaviour
    {
        Material material;

        public void Init(Material particleMaterial)
        {
            material = particleMaterial;
        }

        public void Burst(Vector3 position, Color color, int count, float speed, float size)
        {
            var ps = Create("Burst", position, null);
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.white);
            main.gravityModifier = 0.8f;
            main.stopAction = ParticleSystemStopAction.Destroy;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
            ps.Play();
        }

        public void Flash(Vector3 position, Color color, float size)
        {
            var ps = Create("Flash", position, null);
            var main = ps.main;
            main.duration = 0.2f;
            main.loop = false;
            main.startLifetime = 0.14f;
            main.startSpeed = 0f;
            main.startSize = size;
            main.startColor = color;
            main.stopAction = ParticleSystemStopAction.Destroy;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
            var shape = ps.shape;
            shape.enabled = false;
            ps.Play();
        }

        public void Rise(Vector3 position, Color color, float radius)
        {
            var ps = Create("Rise", position + Vector3.up * 0.05f, null);
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.0f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.36f);
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.white);
            main.stopAction = ParticleSystemStopAction.Destroy;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 24), new ParticleSystem.Burst(0.15f, 18) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius * 1.2f;
            shape.rotation = new Vector3(90f, 0f, 0f);
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.y = new ParticleSystem.MinMaxCurve(1.2f, 2.4f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            ps.Play();
        }

        public ParticleSystem Trail(Transform parent, Color color)
        {
            var ps = Create("Trail", parent.position, parent);
            var main = ps.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.42f);
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.white);
            var emission = ps.emission;
            emission.rateOverTime = 90f;
            ps.Play();
            return ps;
        }

        public void Release(ParticleSystem ps)
        {
            ps.transform.SetParent(null, true);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            var main = ps.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
        }

        ParticleSystem Create(string name, Vector3 position, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;

            var main = ps.main;
            main.playOnAwake = false;
            main.useUnscaledTime = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 200;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.08f;
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
            return ps;
        }
    }
}

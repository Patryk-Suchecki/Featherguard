using System;
using System.Collections;
using System.Collections.Generic;
using Featherguard.Feedback;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Featherguard.Battle.Presentation
{
    public sealed class DeliveryContext
    {
        public UnitView Actor;
        public IReadOnlyList<UnitView> Targets;
        public AbilityDefinition Ability;
        public AbilityVisuals Visuals;
        public ICombatFeedback Feedback;
        public MonoBehaviour Host;
        public Action<int, UnitView> Impact;
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public abstract class AbilityDelivery
    {
        public abstract IEnumerator Deliver(DeliveryContext context);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class InstantDelivery : AbilityDelivery
    {
        public override IEnumerator Deliver(DeliveryContext context)
        {
            yield return context.Actor.Animator.PlayUntilHitFrame(context.Visuals.animationState);
            context.Feedback.Play(context.Visuals.castSound);
            for (int hit = 0; hit < context.Ability.hits; hit++)
                foreach (var target in context.Targets)
                    context.Impact(hit, target);
        }
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class MeleeDelivery : AbilityDelivery
    {
        public float approachTime = 0.3f;
        public float approachHop = 0.55f;
        public float returnTime = 0.32f;
        public float returnHop = 0.45f;
        public float hitInterval = 0.15f;
        public float recovery = 0.4f;
        public float reach = 0.3f;

        public override IEnumerator Deliver(DeliveryContext context)
        {
            var actor = context.Actor;
            var victim = context.Targets[0];
            var strike = victim.transform.position - actor.Facing * (victim.Radius + actor.Radius + reach);
            context.Feedback.Play(SfxId.Whoosh, 0.6f);
            yield return UnitMotion.HopTo(actor.transform, strike, approachTime, approachHop);
            yield return actor.Animator.PlayUntilHitFrame(context.Visuals.animationState);
            context.Feedback.Play(context.Visuals.castSound);
            for (int i = 0; i < context.Ability.hits; i++)
            {
                foreach (var target in context.Targets)
                    context.Impact(i, target);
                if (i < context.Ability.hits - 1)
                    yield return new WaitForSeconds(hitInterval);
            }
            yield return new WaitForSeconds(recovery);
            yield return UnitMotion.HopTo(actor.transform, actor.HomePosition, returnTime, returnHop);
        }
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class ProjectileDelivery : AbilityDelivery
    {
        [Tooltip("Model to fly. When empty, a particle trail is used.")]
        public GameObject prefab;
        public float speed = 10f;
        public float arc = 0.05f;
        public bool orientAlongPath;
        public float volleyInterval = 0.12f;
        public float spread = 0.12f;

        public override IEnumerator Deliver(DeliveryContext context)
        {
            yield return context.Actor.Animator.PlayUntilHitFrame(context.Visuals.animationState);
            var flights = new List<Coroutine>();
            for (int i = 0; i < context.Ability.hits; i++)
            {
                context.Feedback.Play(context.Visuals.castSound);
                foreach (var target in context.Targets)
                    flights.Add(context.Host.StartCoroutine(Fly(context, target, i)));
                yield return new WaitForSeconds(volleyInterval);
            }
            foreach (var flight in flights)
                yield return flight;
        }

        IEnumerator Fly(DeliveryContext context, UnitView target, int hit)
        {
            var actor = context.Actor;
            Vector3 from = actor.Center + actor.Facing * 0.45f;
            Vector3 to = target.Center + UnityEngine.Random.insideUnitSphere * spread;

            Transform projectile;
            ParticleSystem trail = null;
            if (prefab != null)
            {
                projectile = UnityEngine.Object.Instantiate(prefab).transform;
            }
            else
            {
                projectile = new GameObject("Projectile").transform;
                trail = context.Feedback.Trail(projectile, context.Visuals.fxColor);
            }
            projectile.position = from;

            float distance = Vector3.Distance(from, to);
            float duration = distance / Mathf.Max(0.01f, speed);
            float height = distance * arc;
            Vector3 previous = from;
            for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
            {
                Vector3 position = Vector3.Lerp(from, to, t) + Vector3.up * height * 4f * t * (1f - t);
                projectile.position = position;
                if (orientAlongPath && position != previous)
                    projectile.rotation = Quaternion.LookRotation(previous - position);
                previous = position;
                yield return null;
            }

            if (trail != null)
                context.Feedback.Release(trail);
            UnityEngine.Object.Destroy(projectile.gameObject);
            context.Impact(hit, target);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Feel;
using UnityEngine;
using UnityEngine.Rendering;

namespace Featherguard.Battle
{
    public class BattleUnit : MonoBehaviour
    {
        MeshRenderer ring;
        Material ringMaterial;
        float ringBaseScale;
        readonly List<StatusEffect> statuses = new();
        readonly List<StatModifier> modifiers = new();

        public UnitDefinition Definition { get; private set; }
        public Team Team => Definition.team;
        public int Health { get; private set; }
        public int MaxHealth => Mathf.Max(1, Mathf.RoundToInt(Stat(StatType.MaxHealth)));
        public bool IsAlive => Health > 0;
        public bool HasActed { get; set; }
        public IReadOnlyList<StatusEffect> Statuses => statuses;
        public Vector3 HomePosition { get; set; }
        public UnitAnimator Animator { get; private set; }
        public HitReaction Reaction { get; private set; }
        public float Height => Definition.height * Definition.modelScale;
        public float Radius => 0.45f * Definition.modelScale;
        public Vector3 Center => transform.position + Vector3.up * Height * 0.5f;
        public Vector3 Top => transform.position + Vector3.up * (Height + 0.1f);
        public Vector3 Facing => Team == Team.Heroes ? Vector3.right : Vector3.left;

        public event Action Changed;

        public static BattleUnit Spawn(UnitDefinition definition, Vector3 position, BattleConfig config)
        {
            var root = new GameObject(definition.displayName);
            float yaw = definition.team == Team.Heroes ? 115f : -115f;
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            var model = Instantiate(definition.model, root.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            model.transform.localScale = Vector3.one * definition.modelScale;
            var animator = model.GetComponent<Animator>();
            if (animator == null)
                animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = definition.animator;
            animator.applyRootMotion = false;

            var unit = root.AddComponent<BattleUnit>();
            unit.Definition = definition;
            unit.Health = unit.MaxHealth;
            unit.HomePosition = position;
            unit.Animator = model.AddComponent<UnitAnimator>();
            unit.Reaction = root.AddComponent<HitReaction>();
            unit.Reaction.Init(config.flashMaterial, model.transform);

            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = Vector3.up * unit.Height * 0.5f;
            collider.height = unit.Height;
            collider.radius = unit.Radius;

            var ringObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ringObject.name = "Ring";
            Destroy(ringObject.GetComponent<Collider>());
            ringObject.transform.SetParent(root.transform, false);
            ringObject.transform.localPosition = Vector3.up * 0.03f;
            ringObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            unit.ringBaseScale = unit.Radius * 3.4f;
            unit.ring = ringObject.GetComponent<MeshRenderer>();
            unit.ring.shadowCastingMode = ShadowCastingMode.Off;
            unit.ringMaterial = new Material(config.ringMaterial);
            unit.ring.sharedMaterial = unit.ringMaterial;
            unit.SetRing(null);
            return unit;
        }

        void Update()
        {
            if (!ring.enabled) return;
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.05f;
            ring.transform.localScale = Vector3.one * ringBaseScale * pulse;
        }

        public void SetRing(Color? color)
        {
            ring.enabled = color.HasValue;
            if (color.HasValue)
                ringMaterial.color = color.Value;
        }

        public float Stat(StatType stat)
        {
            float value = stat switch
            {
                StatType.CritChance => 0.15f,
                StatType.CritDamage => 1.5f,
                StatType.MaxHealth => Definition.maxHealth,
                _ => 1f,
            };
            float multiplier = 1f;
            foreach (var modifier in modifiers.Concat(StatusModifiers()))
            {
                if (modifier.stat != stat) continue;
                value += modifier.add;
                multiplier += modifier.multiply;
            }
            return value * Mathf.Max(0f, multiplier);
        }

        IEnumerable<StatModifier> StatusModifiers()
        {
            foreach (var status in statuses)
            {
                switch (status.Type)
                {
                    case StatusType.Shield: yield return new StatModifier(StatType.DamageTaken, multiply: -0.5f); break;
                    case StatusType.Empower: yield return new StatModifier(StatType.Damage, multiply: 0.5f); break;
                    case StatusType.Weaken: yield return new StatModifier(StatType.Damage, multiply: -0.3f); break;
                }
            }
        }

        public void AddModifier(StatModifier modifier)
        {
            modifiers.Add(modifier);
            Health = Mathf.Min(Health, MaxHealth);
            Changed?.Invoke();
        }

        public bool Has(StatusType type) => statuses.Any(s => s.Type == type);

        public void AddStatus(StatusApplication application, bool ownTurn)
        {
            var existing = statuses.FirstOrDefault(s => s.Type == application.type);
            if (existing != null)
            {
                existing.Turns = Mathf.Max(existing.Turns, application.turns);
                existing.Potency = Mathf.Max(existing.Potency, application.potency);
                existing.Fresh |= ownTurn;
            }
            else
            {
                statuses.Add(new StatusEffect { Type = application.type, Turns = application.turns, Potency = application.potency, Fresh = ownTurn });
            }
            Changed?.Invoke();
        }

        public void RemoveStatus(StatusType type)
        {
            if (statuses.RemoveAll(s => s.Type == type) > 0)
                Changed?.Invoke();
        }

        public void EndTurn()
        {
            foreach (var status in statuses)
            {
                if (status.Fresh)
                    status.Fresh = false;
                else
                    status.Turns--;
            }
            statuses.RemoveAll(s => s.Turns <= 0);
            Changed?.Invoke();
        }

        public void ClearStatuses()
        {
            statuses.Clear();
            Changed?.Invoke();
        }

        public int ApplyDamage(int amount)
        {
            amount = Mathf.RoundToInt(amount * Stat(StatType.DamageTaken));
            amount = Mathf.Clamp(amount, 0, Health);
            Health -= amount;
            Changed?.Invoke();
            return amount;
        }

        public int ApplyHeal(int amount)
        {
            if (!IsAlive) return 0;
            amount = Mathf.Clamp(amount, 0, MaxHealth - Health);
            Health += amount;
            Changed?.Invoke();
            return amount;
        }

        public void NotifyChanged() => Changed?.Invoke();
    }
}

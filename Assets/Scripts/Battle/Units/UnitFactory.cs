using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle
{
    public sealed class UnitFactory
    {
        readonly BattleRules rules;
        readonly Material flashMaterial;
        readonly Material ringMaterial;

        public UnitFactory(BattleRules rules, Material flashMaterial, Material ringMaterial)
        {
            this.rules = rules;
            this.flashMaterial = flashMaterial;
            this.ringMaterial = ringMaterial;
        }

        public BattleUnit Spawn(UnitDefinition definition, Vector3 position)
        {
            var root = new GameObject(definition.displayName);
            float yaw = definition.team == Team.Heroes ? rules.heroYaw : -rules.heroYaw;
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            var model = Object.Instantiate(definition.model, root.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            model.transform.localScale = Vector3.one * definition.modelScale;
            var animator = model.GetComponent<Animator>();
            if (animator == null)
                animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = definition.animator;
            animator.applyRootMotion = false;

            var unit = root.AddComponent<BattleUnit>();
            var reaction = root.AddComponent<HitReaction>();
            reaction.Init(flashMaterial, model.transform);
            float radius = 0.45f * definition.modelScale;
            var ring = SelectionRing.Create(root.transform, radius * 3.4f, ringMaterial);
            unit.Bind(definition, new StatBlock(definition.maxHealth, rules.baseCritChance, rules.baseCritDamage), model.AddComponent<UnitAnimator>(), reaction, ring);
            unit.HomePosition = position;

            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = Vector3.up * unit.Height * 0.5f;
            collider.height = unit.Height;
            collider.radius = unit.Radius;
            return unit;
        }
    }
}

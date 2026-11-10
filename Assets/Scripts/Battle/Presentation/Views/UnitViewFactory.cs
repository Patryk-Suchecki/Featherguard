using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class UnitViewFactory
    {
        readonly BattlePresentationConfig config;
        readonly Material flashMaterial;
        readonly Material ringMaterial;
        readonly ToonMaterials toon;

        public UnitViewFactory(BattlePresentationConfig config, Material flashMaterial, Material ringMaterial, ToonMaterials toon)
        {
            this.config = config;
            this.flashMaterial = flashMaterial;
            this.ringMaterial = ringMaterial;
            this.toon = toon;
        }

        public UnitView Create(BattleUnit unit, UnitSnapshot state, Vector3 position)
        {
            var definition = unit.Definition;
            var root = new GameObject(definition.displayName);
            float yaw = definition.team == Team.Heroes ? config.heroYaw : -config.heroYaw;
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

            toon.Apply(model);

            var view = root.AddComponent<UnitView>();
            var reaction = root.AddComponent<HitReaction>();
            reaction.Init(flashMaterial, model.transform);
            float radius = 0.45f * definition.modelScale;
            var ring = SelectionRing.Create(root.transform, radius * 3.4f, ringMaterial);
            view.Bind(unit, state, model.AddComponent<UnitAnimator>(), reaction, ring);
            view.HomePosition = position;

            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = Vector3.up * view.Height * 0.5f;
            collider.height = view.Height;
            collider.radius = view.Radius;
            return view;
        }
    }
}

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Featherguard.Battle.Presentation
{
    public sealed class UnitViewFactory
    {
        readonly BattlePresentationConfig config;
        readonly Material flashMaterial;
        readonly Material ringMaterial;
        readonly VisualsLibrary<UnitDefinition, UnitVisuals> visuals;
        readonly ToonMaterials toon;

        public UnitViewFactory(BattlePresentationConfig config, Material flashMaterial, Material ringMaterial, VisualsLibrary<UnitDefinition, UnitVisuals> visuals, ToonMaterials toon)
        {
            this.config = config;
            this.flashMaterial = flashMaterial;
            this.ringMaterial = ringMaterial;
            this.visuals = visuals;
            this.toon = toon;
        }

        public UnitView Create(BattleUnit unit, UnitSnapshot state, Vector3 position)
        {
            var definition = unit.Definition;
            var look = visuals.Of(definition);
            if (look.model == null)
                throw new InvalidOperationException($"Unit '{definition.name}' has no UnitVisuals with a model in BattlePresentationConfig.units. Run Featherguard > Rebuild Battle Scene.");
            var root = new GameObject(definition.displayName);
            float yaw = definition.team == Team.Heroes ? config.heroYaw : -config.heroYaw;
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            var model = Object.Instantiate(look.model, root.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            model.transform.localScale = Vector3.one * look.modelScale;
            var animator = model.GetComponent<Animator>();
            if (animator == null)
                animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = look.animator;
            animator.applyRootMotion = false;

            toon.Apply(model);

            var view = root.AddComponent<UnitView>();
            var reaction = root.AddComponent<HitReaction>();
            reaction.Init(flashMaterial, model.transform);
            var ring = SelectionRing.Create(root.transform, look.Radius * 3.4f, ringMaterial);
            view.Bind(unit, look, state, model.AddComponent<UnitAnimator>(), reaction, ring);
            view.HomePosition = position;

            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = Vector3.up * view.Height * 0.5f;
            collider.height = view.Height;
            collider.radius = view.Radius;
            return view;
        }
    }
}

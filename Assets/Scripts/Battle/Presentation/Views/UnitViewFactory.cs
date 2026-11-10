using System.Collections.Generic;
using System.Linq;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class UnitViewFactory : System.IDisposable
    {
        readonly BattlePresentationConfig config;
        readonly Material flashMaterial;
        readonly Material ringMaterial;
        readonly Material toonMaterial;
        readonly Dictionary<Material, Material> toonVariants = new();

        public UnitViewFactory(BattlePresentationConfig config, Material flashMaterial, Material ringMaterial, Material toonMaterial)
        {
            this.config = config;
            this.flashMaterial = flashMaterial;
            this.ringMaterial = ringMaterial;
            this.toonMaterial = toonMaterial;
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

            if (toonMaterial != null)
                ApplyToon(model);

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

        public void Dispose()
        {
            foreach (var variant in toonVariants.Values)
                Object.Destroy(variant);
            toonVariants.Clear();
        }

        void ApplyToon(GameObject model)
        {
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials = renderer.sharedMaterials.Select(ToonVariant).ToArray();
        }

        Material ToonVariant(Material original)
        {
            if (original == null) return toonMaterial;
            if (toonVariants.TryGetValue(original, out var variant)) return variant;

            variant = new Material(toonMaterial) { name = $"{original.name} (Toon)" };
            variant.SetTexture("_BaseMap", original.HasProperty("_BaseMap") ? original.GetTexture("_BaseMap") : original.mainTexture);
            variant.SetColor("_BaseColor", original.HasProperty("_BaseColor") ? original.GetColor("_BaseColor") : original.color);
            toonVariants[original] = variant;
            return variant;
        }
    }
}

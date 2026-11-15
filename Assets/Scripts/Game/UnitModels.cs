using Featherguard.Battle.Presentation;
using UnityEngine;

namespace Featherguard.Game
{
    public static class UnitModels
    {
        public static GameObject Place(UnitVisuals look, Transform spot, Vector3 facing)
        {
            if (look == null || look.model == null) return null;
            facing.y = 0f;
            var rotation = Quaternion.LookRotation(facing) * Quaternion.Euler(0f, 180f, 0f);
            var model = Object.Instantiate(look.model, spot.position, rotation, spot);
            model.transform.localScale = Vector3.one * look.modelScale;
            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = look.animator;
            animator.applyRootMotion = false;
            return model;
        }
    }
}

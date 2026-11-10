using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Featherguard.Feel
{
    public readonly struct ImpactContext
    {
        public readonly Vector3 Point;
        public readonly Vector3 Facing;
        public readonly float TargetRadius;
        public readonly Material Material;
        public readonly Camera Camera;

        public ImpactContext(Vector3 point, Vector3 facing, float targetRadius, Material material, Camera camera)
        {
            Point = point;
            Facing = facing;
            TargetRadius = targetRadius;
            Material = material;
            Camera = camera;
        }
    }

    [Serializable]
    public abstract class ImpactEffect
    {
        public abstract void Play(in ImpactContext context);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class ClawSlashEffect : ImpactEffect
    {
        [Min(1)] public int claws = 3;
        [Tooltip("Slash length as a multiple of the target's radius.")]
        [Min(0.1f)] public float length = 2.0f;
        [Tooltip("Widest point of one slash, in metres.")]
        [Min(0.01f)] public float width = 0.11f;
        [Tooltip("Distance between neighbouring claw marks, in metres.")]
        [Min(0f)] public float spacing = 0.15f;
        [Tooltip("Angle below the horizontal, in the direction of the swing.")]
        [Range(0f, 90f)] public float angle = 55f;
        [Tooltip("How much each slash curves.")]
        [Range(0f, 1f)] public float bend = 0.18f;
        public Color core = new(1f, 0.69f, 0.18f);
        public Color edge = new(0.71f, 0.33f, 0.12f);
        public Color outline = new(0.07f, 0.05f, 0.06f);
        [Min(0.05f)] public float duration = 0.35f;

        public override void Play(in ImpactContext context)
        {
            var camera = context.Camera;
            if (camera == null) return;

            var right = camera.transform.right;
            var up = camera.transform.up;
            float swing = Vector3.Dot(context.Facing, right) >= 0f ? 1f : -1f;
            float radians = angle * Mathf.Deg2Rad;
            var direction = (right * swing * Mathf.Cos(radians) - up * Mathf.Sin(radians)).normalized;
            var across = Vector3.Cross(direction, camera.transform.forward).normalized;
            float slashLength = length * context.TargetRadius;

            for (int i = 0; i < claws; i++)
            {
                var center = context.Point + across * (spacing * (i - (claws - 1) * 0.5f));
                var shape = new SlashShape(center - direction * slashLength * 0.5f, center + direction * slashLength * 0.5f,
                    across * slashLength * bend, width, core, edge, outline, duration);
                SlashMark.Spawn(shape, context.Material, camera.transform.forward);
            }
        }
    }
}

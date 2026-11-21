using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public enum GestureShape { Circle, Lightning, Thorns, Shield, Wave, Arc }

    public static class GestureShapes
    {
        public static bool IsClosed(GestureShape shape) => shape == GestureShape.Circle;

        public static List<Vector2> Points(GestureShape shape, float size) => Unit(shape).Select(p => p * size).ToList();

        static IEnumerable<Vector2> Unit(GestureShape shape) => shape switch
        {
            GestureShape.Circle => Curve(33, t => new Vector2(Mathf.Sin(t * Mathf.PI * 2f), Mathf.Cos(t * Mathf.PI * 2f))),
            GestureShape.Lightning => new[] { new Vector2(0.35f, 1f), new Vector2(-0.3f, 0.12f), new Vector2(0.3f, -0.05f), new Vector2(-0.35f, -1f) },
            GestureShape.Thorns => new[] { new Vector2(-1f, -0.3f), new Vector2(-0.5f, 0.45f), new Vector2(0f, -0.3f), new Vector2(0.5f, 0.45f), new Vector2(1f, -0.3f) },
            GestureShape.Shield => new[] { new Vector2(-0.75f, 0.8f), new Vector2(-0.75f, -0.1f), new Vector2(0f, -1f), new Vector2(0.75f, -0.1f), new Vector2(0.75f, 0.8f) },
            GestureShape.Wave => Curve(25, t => new Vector2(t * 2f - 1f, 0.4f * Mathf.Sin(t * Mathf.PI * 3f))),
            GestureShape.Arc => Curve(17, t => new Vector2(-Mathf.Cos(t * Mathf.PI), Mathf.Sin(t * Mathf.PI) * 0.8f - 0.3f)),
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };

        static IEnumerable<Vector2> Curve(int count, Func<float, Vector2> point)
        {
            for (int i = 0; i < count; i++)
                yield return point(i / (count - 1f));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public static class StrokeMatcher
    {
        public const int SampleCount = 64;
        static readonly float HalfDiagonal = 0.5f * Mathf.Sqrt(2f);

        public static float Score(IReadOnlyList<Vector2> stroke, IReadOnlyList<Vector2> template, bool closed)
        {
            if (stroke.Count < 2 || template.Count < 2 || Length(stroke) <= 0f) return 0f;
            var drawn = Normalize(Resample(stroke, SampleCount));
            var wanted = Normalize(Resample(template, SampleCount));
            var reversed = Enumerable.Reverse(drawn).ToList();
            float best = Mathf.Min(PathDistance(drawn, wanted), PathDistance(reversed, wanted));
            if (closed)
            {
                for (int shift = 2; shift < SampleCount; shift += 2)
                {
                    best = Mathf.Min(best, PathDistance(Shift(drawn, shift), wanted));
                    best = Mathf.Min(best, PathDistance(Shift(reversed, shift), wanted));
                }
            }
            return Mathf.Clamp01(1f - best / HalfDiagonal);
        }

        public static List<Vector2> Resample(IReadOnlyList<Vector2> points, int count)
        {
            float step = Length(points) / (count - 1);
            var result = new List<Vector2> { points[0] };
            if (step <= 0f)
            {
                while (result.Count < count) result.Add(points[0]);
                return result;
            }
            float carried = 0f;
            var previous = points[0];
            for (int i = 1; i < points.Count && result.Count < count; i++)
            {
                var current = points[i];
                float segment = Vector2.Distance(previous, current);
                while (carried + segment >= step && result.Count < count)
                {
                    float t = (step - carried) / segment;
                    previous = Vector2.Lerp(previous, current, t);
                    result.Add(previous);
                    segment = Vector2.Distance(previous, current);
                    carried = 0f;
                }
                carried += segment;
                previous = current;
            }
            while (result.Count < count) result.Add(points[points.Count - 1]);
            return result;
        }

        public static float Length(IReadOnlyList<Vector2> points)
        {
            float length = 0f;
            for (int i = 1; i < points.Count; i++)
                length += Vector2.Distance(points[i - 1], points[i]);
            return length;
        }

        public static float DistanceToPath(Vector2 point, IReadOnlyList<Vector2> path)
        {
            float best = float.MaxValue;
            for (int i = 1; i < path.Count; i++)
                best = Mathf.Min(best, DistanceToSegment(point, path[i - 1], path[i]));
            return path.Count == 1 ? Vector2.Distance(point, path[0]) : best;
        }

        public static Vector2 PointAlong(IReadOnlyList<Vector2> path, float fraction)
        {
            float target = Length(path) * Mathf.Clamp01(fraction);
            for (int i = 1; i < path.Count; i++)
            {
                float segment = Vector2.Distance(path[i - 1], path[i]);
                if (target <= segment && segment > 0f)
                    return Vector2.Lerp(path[i - 1], path[i], target / segment);
                target -= segment;
            }
            return path[path.Count - 1];
        }

        static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float lengthSquared = ab.sqrMagnitude;
            float t = lengthSquared > 0f ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSquared) : 0f;
            return Vector2.Distance(point, a + ab * t);
        }

        static List<Vector2> Normalize(List<Vector2> points)
        {
            var centroid = points.Aggregate(Vector2.zero, (sum, p) => sum + p) / points.Count;
            float minX = points.Min(p => p.x), maxX = points.Max(p => p.x);
            float minY = points.Min(p => p.y), maxY = points.Max(p => p.y);
            float size = Mathf.Max(maxX - minX, maxY - minY, 0.0001f);
            return points.Select(p => (p - centroid) / size).ToList();
        }

        static float PathDistance(IReadOnlyList<Vector2> a, IReadOnlyList<Vector2> b)
        {
            float sum = 0f;
            for (int i = 0; i < a.Count; i++)
                sum += Vector2.Distance(a[i], b[i]);
            return sum / a.Count;
        }

        static List<Vector2> Shift(IReadOnlyList<Vector2> points, int shift) =>
            Enumerable.Range(0, points.Count).Select(i => points[(i + shift) % points.Count]).ToList();
    }
}

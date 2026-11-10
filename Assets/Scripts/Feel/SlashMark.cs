using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Feel
{
    public readonly struct SlashShape
    {
        public readonly Vector3 Start;
        public readonly Vector3 End;
        public readonly Vector3 Bulge;
        public readonly float Width;
        public readonly Color Core;
        public readonly Color Edge;
        public readonly Color Outline;
        public readonly float Duration;

        public SlashShape(Vector3 start, Vector3 end, Vector3 bulge, float width, Color core, Color edge, Color outline, float duration)
        {
            Start = start;
            End = end;
            Bulge = bulge;
            Width = width;
            Core = core;
            Edge = edge;
            Outline = outline;
            Duration = duration;
        }
    }

    public class SlashMark : MonoBehaviour
    {
        const int Segments = 16;
        const float RevealShare = 0.22f;
        const float FadeStart = 0.45f;

        readonly List<Vector3> vertices = new();
        readonly List<Color> colors = new();
        readonly List<int> triangles = new();
        SlashShape shape;
        Vector3 viewForward;
        Mesh mesh;
        float age;

        public static void Spawn(in SlashShape shape, Material material, Vector3 viewForward)
        {
            var go = new GameObject("Slash");
            var mark = go.AddComponent<SlashMark>();
            mark.shape = shape;
            mark.viewForward = viewForward;
            mark.mesh = new Mesh { name = "Slash" };
            mark.mesh.MarkDynamic();
            go.AddComponent<MeshFilter>().sharedMesh = mark.mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            mark.Rebuild();
        }

        void Update()
        {
            age += Time.unscaledDeltaTime * GameClock.Scale;
            if (age >= shape.Duration)
            {
                Destroy(mesh);
                Destroy(gameObject);
                return;
            }
            Rebuild();
        }

        void Rebuild()
        {
            float reveal = Mathf.Clamp01(age / (shape.Duration * RevealShare));
            float fade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(shape.Duration * FadeStart, shape.Duration, age));

            vertices.Clear();
            colors.Clear();
            triangles.Clear();
            AddBand(reveal, shape.Width * 1.7f * fade, shape.Outline, 0f);
            AddBand(reveal, shape.Width * fade, shape.Edge, 0.01f);
            AddBand(reveal, shape.Width * 0.45f * fade, shape.Core, 0.02f);

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        void AddBand(float reveal, float width, Color color, float lift)
        {
            int first = vertices.Count;
            var control = (shape.Start + shape.End) * 0.5f + shape.Bulge;
            var offset = -viewForward * lift;
            for (int i = 0; i < Segments; i++)
            {
                float along = i / (Segments - 1f);
                float t = along * reveal;
                var point = Bezier(shape.Start, control, shape.End, t);
                var tangent = Bezier(shape.Start, control, shape.End, Mathf.Min(1f, t + 0.01f)) - Bezier(shape.Start, control, shape.End, Mathf.Max(0f, t - 0.01f));
                var side = Vector3.Cross(tangent, viewForward).normalized;
                float half = width * 0.5f * Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Mathf.PI * along)), 0.7f);
                vertices.Add(point + side * half + offset);
                vertices.Add(point - side * half + offset);
                colors.Add(color);
                colors.Add(color);
                if (i == 0) continue;
                int a = first + (i - 1) * 2;
                triangles.Add(a);
                triangles.Add(a + 2);
                triangles.Add(a + 1);
                triangles.Add(a + 1);
                triangles.Add(a + 2);
                triangles.Add(a + 3);
            }
        }

        static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }
    }
}

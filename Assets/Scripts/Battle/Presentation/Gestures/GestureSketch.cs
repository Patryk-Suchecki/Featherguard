using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public enum SketchTone { Target, Cue, Guide, Hit, Miss, Faint }

    public readonly struct SketchCircle
    {
        public readonly Vector2 Center;
        public readonly float Radius;
        public readonly SketchTone Tone;
        public readonly bool Filled;

        public SketchCircle(Vector2 center, float radius, SketchTone tone, bool filled)
        {
            Center = center;
            Radius = radius;
            Tone = tone;
            Filled = filled;
        }
    }

    public readonly struct SketchLine
    {
        public readonly Vector2 From;
        public readonly Vector2 To;
        public readonly float Width;
        public readonly SketchTone Tone;

        public SketchLine(Vector2 from, Vector2 to, float width, SketchTone tone)
        {
            From = from;
            To = to;
            Width = width;
            Tone = tone;
        }
    }

    public readonly struct SketchLabel
    {
        public readonly Vector2 Position;
        public readonly string Text;
        public readonly int Size;
        public readonly SketchTone Tone;

        public SketchLabel(Vector2 position, string text, int size, SketchTone tone)
        {
            Position = position;
            Text = text;
            Size = size;
            Tone = tone;
        }
    }

    public sealed class GestureSketch
    {
        public const float PanelWidth = 1000f;
        public const float PanelHeight = 640f;

        readonly List<SketchCircle> circles = new();
        readonly List<SketchLine> lines = new();
        readonly List<SketchLabel> labels = new();

        public IReadOnlyList<SketchCircle> Circles => circles;
        public IReadOnlyList<SketchLine> Lines => lines;
        public IReadOnlyList<SketchLabel> Labels => labels;
        public Vector3? AnchorWorld { get; private set; }
        public bool Backdrop { get; set; }

        public void Clear(Vector3? anchorWorld)
        {
            circles.Clear();
            lines.Clear();
            labels.Clear();
            AnchorWorld = anchorWorld;
            Backdrop = anchorWorld == null;
        }

        public void Circle(Vector2 center, float radius, SketchTone tone, bool filled = false) => circles.Add(new SketchCircle(center, radius, tone, filled));

        public void Line(Vector2 from, Vector2 to, float width, SketchTone tone) => lines.Add(new SketchLine(from, to, width, tone));

        public void Label(Vector2 position, string text, int size, SketchTone tone) => labels.Add(new SketchLabel(position, text, size, tone));

        public void Path(IReadOnlyList<Vector2> points, float width, SketchTone tone, int count = -1)
        {
            int last = count < 0 ? points.Count : Mathf.Min(count, points.Count);
            for (int i = 1; i < last; i++)
                Line(points[i - 1], points[i], width, tone);
        }

        public void Arrow(Vector2 from, Vector2 to, float width, SketchTone tone)
        {
            Line(from, to, width, tone);
            var back = (from - to).normalized * 60f;
            Line(to, to + (Vector2)(Quaternion.Euler(0f, 0f, 28f) * back), width, tone);
            Line(to, to + (Vector2)(Quaternion.Euler(0f, 0f, -28f) * back), width, tone);
        }

        public void Bar(Vector2 from, Vector2 to, float width, float fill, SketchTone back, SketchTone front)
        {
            Line(from, to, width, back);
            if (fill > 0f) Line(from, Vector2.Lerp(from, to, Mathf.Clamp01(fill)), width * 0.7f, front);
        }
    }
}

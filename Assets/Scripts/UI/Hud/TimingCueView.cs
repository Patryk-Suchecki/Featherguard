using System.Collections.Generic;
using Featherguard.Battle.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class TimingCueView
    {
        static readonly Color BlockColor = new(0.55f, 0.85f, 1f);
        static readonly Color BackdropColor = new(0.04f, 0.05f, 0.09f, 0.6f);

        readonly Camera view;
        readonly RectTransform root;
        readonly GameObject gesture;
        readonly GameObject backdrop;
        readonly GameObject block;
        readonly Pool<Image> lines;
        readonly Pool<Image> circles;
        readonly Pool<Text> labels;
        Sprite ring;
        Sprite disc;

        public TimingCueView(RectTransform parent, Camera view)
        {
            this.view = view;
            root = Ui.Centered("TimingCue", parent, Vector2.zero, Vector2.zero);

            var gestureRect = Ui.Centered("Gesture", root, Vector2.zero, Vector2.zero);
            gesture = gestureRect.gameObject;
            var backdropRect = Ui.Centered("Backdrop", gestureRect, Vector2.zero, new Vector2(GestureSketch.PanelWidth, GestureSketch.PanelHeight));
            Ui.Panel(backdropRect, BackdropColor);
            backdrop = backdropRect.gameObject;
            lines = new Pool<Image>(Ui.Centered("Lines", gestureRect, Vector2.zero, Vector2.zero), rt => Ui.Image(rt, Color.white));
            circles = new Pool<Image>(Ui.Centered("Circles", gestureRect, Vector2.zero, Vector2.zero), rt => Ui.Image(rt, Color.white));
            labels = new Pool<Text>(Ui.Centered("Labels", gestureRect, Vector2.zero, Vector2.zero), rt => Ui.Label(rt, "", 40, TextAnchor.MiddleCenter, Color.white));

            var blockRect = Ui.Centered("Block", root, Vector2.zero, new Vector2(520f, 140f));
            Ui.Label(blockRect, "BLOCK!", 96, TextAnchor.MiddleCenter, BlockColor);
            block = blockRect.gameObject;

            Hide();
        }

        public void ShowGesture(GestureSketch sketch)
        {
            if (sketch.AnchorWorld.HasValue) Place(sketch.AnchorWorld.Value);
            else Centre();
            gesture.SetActive(true);
            block.SetActive(false);
            backdrop.SetActive(sketch.Backdrop);
            ring ??= Ui.Sprite("Ring");
            disc ??= Ui.Sprite("Circle");

            lines.Begin();
            foreach (var line in sketch.Lines)
            {
                var image = lines.Next();
                var rt = image.rectTransform;
                var delta = line.To - line.From;
                rt.anchoredPosition = (line.From + line.To) * 0.5f;
                rt.sizeDelta = new Vector2(delta.magnitude + line.Width * 0.5f, line.Width);
                rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                image.color = ColorOf(line.Tone);
            }
            lines.End();

            circles.Begin();
            foreach (var circle in sketch.Circles)
            {
                var image = circles.Next();
                image.sprite = circle.Filled ? disc : ring;
                image.rectTransform.anchoredPosition = circle.Center;
                image.rectTransform.sizeDelta = Vector2.one * circle.Radius * 2f;
                image.color = ColorOf(circle.Tone);
            }
            circles.End();

            labels.Begin();
            foreach (var label in sketch.Labels)
            {
                var text = labels.Next();
                text.rectTransform.anchoredPosition = label.Position;
                text.text = label.Text;
                text.fontSize = label.Size;
                text.color = ColorOf(label.Tone);
            }
            labels.End();
        }

        public void ShowBlockCue(Vector3 world)
        {
            Place(world);
            gesture.SetActive(false);
            block.SetActive(true);
        }

        public void Hide() => root.gameObject.SetActive(false);

        void Place(Vector3 world)
        {
            root.gameObject.SetActive(true);
            root.position = view.WorldToScreenPoint(world);
        }

        void Centre()
        {
            root.gameObject.SetActive(true);
            root.anchoredPosition = Vector2.zero;
        }

        static Color ColorOf(SketchTone tone) => tone switch
        {
            SketchTone.Target => new Color(1f, 0.78f, 0.2f, 0.95f),
            SketchTone.Cue => new Color(1f, 1f, 1f, 0.95f),
            SketchTone.Guide => new Color(0.7f, 0.82f, 1f, 0.85f),
            SketchTone.Hit => new Color(0.45f, 1f, 0.55f, 0.95f),
            SketchTone.Miss => new Color(1f, 0.38f, 0.32f, 0.95f),
            _ => new Color(1f, 1f, 1f, 0.16f),
        };

        sealed class Pool<T> where T : Component
        {
            readonly RectTransform parent;
            readonly System.Func<RectTransform, T> create;
            readonly List<T> items = new();
            int used;

            public Pool(RectTransform parent, System.Func<RectTransform, T> create)
            {
                this.parent = parent;
                this.create = create;
            }

            public void Begin() => used = 0;

            public T Next()
            {
                if (used == items.Count)
                    items.Add(create(Ui.Centered(typeof(T).Name, parent, Vector2.zero, Vector2.one)));
                var item = items[used++];
                item.gameObject.SetActive(true);
                return item;
            }

            public void End()
            {
                for (int i = used; i < items.Count; i++)
                    items[i].gameObject.SetActive(false);
            }
        }
    }
}

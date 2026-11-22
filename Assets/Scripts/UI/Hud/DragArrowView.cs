using System.Collections.Generic;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class DragArrowView
    {
        const float DotSpacing = 38f;
        const float DotSize = 20f;
        const float HeadLength = 54f;
        const float HeadWidth = 16f;
        const float HeadAngle = 30f;
        const float StartGap = 30f;
        static readonly Color Free = new(1f, 1f, 1f, 0.75f);
        static readonly Color OnEnemy = new(1f, 0.4f, 0.3f, 0.95f);
        static readonly Color OnAlly = new(0.45f, 1f, 0.55f, 0.95f);

        readonly RectTransform root;
        readonly Camera view;
        readonly List<Image> dots = new();
        readonly Image headLeft;
        readonly Image headRight;

        public DragArrowView(RectTransform parent, Camera view)
        {
            this.view = view;
            root = Ui.Stretch("DragArrow", parent);
            headLeft = Ui.Image(Ui.Centered("HeadLeft", root, Vector2.zero, new Vector2(HeadLength, HeadWidth)), Color.white);
            headRight = Ui.Image(Ui.Centered("HeadRight", root, Vector2.zero, new Vector2(HeadLength, HeadWidth)), Color.white);
            headLeft.rectTransform.pivot = headRight.rectTransform.pivot = new Vector2(0f, 0.5f);
            root.gameObject.SetActive(false);
        }

        public void Update(IPlayerCommands commands)
        {
            bool show = commands.AcceptingInput && commands.Dragging;
            root.gameObject.SetActive(show);
            if (!show) return;

            float scale = Mathf.Max(0.0001f, root.lossyScale.x);
            Vector2 from = commands.DragScreenOrigin ?? (Vector2)view.WorldToScreenPoint(commands.DragOrigin);
            var to = commands.DragPointer;
            var delta = (to - from) / scale;
            float length = delta.magnitude;
            var color = ColorOf(commands);
            if (length < StartGap + HeadLength)
            {
                Hide(0);
                headLeft.enabled = headRight.enabled = false;
                return;
            }

            var direction = delta / length;
            int count = Mathf.FloorToInt((length - StartGap - HeadLength * 0.5f) / DotSpacing);
            for (int i = 0; i < count; i++)
            {
                var dot = Dot(i);
                dot.color = color;
                dot.rectTransform.position = from + direction * (StartGap + i * DotSpacing) * scale;
            }
            Hide(count);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Head(headLeft, to, angle + 180f - HeadAngle, color);
            Head(headRight, to, angle + 180f + HeadAngle, color);
        }

        static void Head(Image image, Vector2 tip, float angle, Color color)
        {
            image.enabled = true;
            image.color = color;
            image.rectTransform.position = tip;
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        Image Dot(int index)
        {
            while (dots.Count <= index)
            {
                var dot = Ui.Image(Ui.Centered("Dot", root, Vector2.zero, Vector2.one * DotSize), Color.white);
                dot.sprite = Ui.Sprite("Circle");
                dots.Add(dot);
            }
            dots[index].enabled = true;
            return dots[index];
        }

        void Hide(int from)
        {
            for (int i = from; i < dots.Count; i++)
                dots[i].enabled = false;
        }

        static Color ColorOf(IPlayerCommands commands)
        {
            var target = commands.DragTarget;
            if (target == null) return Free;
            return target.Team == Team.Heroes ? OnAlly : OnEnemy;
        }
    }
}

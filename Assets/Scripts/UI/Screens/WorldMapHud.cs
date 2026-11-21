using System;
using System.Collections.Generic;
using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.UI
{
    public sealed class MapLand
    {
        public string Name;
        public bool Open;
        public int StoryWon;
        public int StoryTotal;
        public Transform LabelAnchor;
        public Transform ActivitiesAnchor;
        public IReadOnlyList<MapActivity> Activities;
    }

    public sealed class MapActivity
    {
        public string Name;
        public ActivityType Type;
        public bool Story;
        public ThreatRating Rating;
        public RewardKind Reward;
        public Action Select;
    }

    public class WorldMapHud : MonoBehaviour
    {
        const float ButtonSize = 92f;
        const float ButtonGap = 16f;
        const float Border = 7f;

        static readonly Color TextColor = new(0.9f, 0.86f, 0.78f);
        static readonly Color LockedColor = new(0.55f, 0.56f, 0.6f);
        static readonly Color StoryColor = new(1f, 0.8f, 0.42f);
        static readonly Color ButtonColor = new(0.1f, 0.1f, 0.13f, 0.92f);
        static readonly Color FillColor = new(0.08f, 0.085f, 0.11f, 1f);
        static readonly Color LegendColor = new(0.05f, 0.05f, 0.08f, 0.8f);
        static readonly RewardKind[] LegendOrder = { RewardKind.Exp, RewardKind.Gold, RewardKind.Materials, RewardKind.Loot, RewardKind.Schematic, RewardKind.Unlock };

        readonly List<(Transform Anchor, RectTransform Rect)> pinned = new();
        Camera sceneCamera;
        Canvas canvas;

        public void Init(Camera camera, IReadOnlyList<MapLand> lands, int storyWon, int storyCount, Action onBack, Action onPrototype)
        {
            sceneCamera = camera;
            var root = ScreenCanvas.Create("WorldMapHUD");
            canvas = root.GetComponent<Canvas>();

            var layer = Ui.Stretch("Lands", root);
            foreach (var land in lands)
            {
                LandLabel(layer, land);
                if (land.Open) ActivityRow(layer, land);
            }

            var safe = SafeArea.Create(root);
            Button(safe, "Back", new Vector2(0f, 1f), new Vector2(24f, -24f), onBack);
            Button(safe, "Prototype run", new Vector2(0f, 0f), new Vector2(24f, 24f), onPrototype, 320f);
            Ui.Label(Ui.Rect("Story", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(900f, 50f)),
                storyWon >= storyCount ? "The story is complete" : $"Story missions {storyWon} / {storyCount}", 34, TextAnchor.MiddleCenter, StoryColor);
            Legend(safe);
            LateUpdate();
        }

        void LateUpdate()
        {
            if (sceneCamera == null) return;
            float scale = canvas.scaleFactor;
            foreach (var (anchor, rect) in pinned)
            {
                if (anchor == null) continue;
                var screen = sceneCamera.WorldToScreenPoint(anchor.position);
                rect.gameObject.SetActive(screen.z > 0f);
                rect.anchoredPosition = new Vector2(screen.x, screen.y) / scale;
            }
        }

        void LandLabel(RectTransform layer, MapLand land)
        {
            var label = Pin(layer, land.LabelAnchor, new Vector2(420f, 84f));
            var color = land.Open ? TextColor : LockedColor;
            Ui.Label(Ui.Rect("Name", label, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(420f, 48f)),
                land.Name.ToUpperInvariant(), 38, TextAnchor.MiddleCenter, color);
            Ui.Label(Ui.Rect("State", label, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(420f, 34f)),
                land.Open ? $"Story {land.StoryWon} / {land.StoryTotal}" : "Locked", 26, TextAnchor.MiddleCenter, land.Open ? StoryColor : LockedColor);
        }

        void ActivityRow(RectTransform layer, MapLand land)
        {
            int count = land.Activities.Count;
            var row = Pin(layer, land.ActivitiesAnchor, new Vector2(Mathf.Max(1, count) * (ButtonSize + ButtonGap), ButtonSize + 34f));
            float first = -(count - 1) * (ButtonSize + ButtonGap) / 2f;
            for (int i = 0; i < count; i++)
                ActivityButton(row, land.Activities[i], first + i * (ButtonSize + ButtonGap));
        }

        static void ActivityButton(RectTransform row, MapActivity activity, float x)
        {
            var color = activity.Story ? ActivityLook.StoryColor : ActivityLook.RewardColor(activity.Reward);
            var button = Ui.Rect(activity.Name, row, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(x, 0f), Vector2.one * ButtonSize);
            Ui.Button(button, color, () => activity.Select());
            Ui.Panel(Ui.Stretch("Fill", button, Border), FillColor);
            ActivityLook.TypeIcon(button, activity.Story ? null : activity.Type, color, ButtonSize * 0.55f);
            Ui.Label(Ui.Rect("Rating", row, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(x, -ButtonSize - 2f), new Vector2(ButtonSize + ButtonGap, 30f)),
                activity.Rating.ToString(), 20, TextAnchor.MiddleCenter, ActivityText.RatingColor(activity.Rating));
        }

        static void Legend(RectTransform parent)
        {
            const float rowHeight = 32f;
            var panel = Ui.Rect("Legend", parent, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(250f, rowHeight * (LegendOrder.Length + 1) + 20f));
            Ui.Panel(panel, LegendColor);
            float y = -10f;
            foreach (var reward in LegendOrder)
            {
                Entry(panel, ActivityLook.RewardColor(reward), ActivityText.RewardName(reward), y, rowHeight);
                y -= rowHeight;
            }
            Entry(panel, ActivityLook.StoryColor, "Main quest", y, rowHeight);
        }

        static void Entry(RectTransform panel, Color color, string text, float y, float height)
        {
            Ui.Panel(Ui.Rect("Swatch", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, y - 6f), new Vector2(20f, 20f)), color);
            Ui.Label(Ui.Rect(text, panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(50f, y), new Vector2(190f, height)), text, 22, TextAnchor.MiddleLeft, TextColor);
        }

        RectTransform Pin(RectTransform layer, Transform anchor, Vector2 size)
        {
            var rect = Ui.Rect("Pinned", layer, Vector2.zero, new Vector2(0.5f, 1f), Vector2.zero, size);
            pinned.Add((anchor, rect));
            return rect;
        }

        static void Button(RectTransform parent, string label, Vector2 anchor, Vector2 position, Action onClick, float width = 240f)
        {
            var rect = Ui.Rect(label, parent, anchor, anchor, position, new Vector2(width, 84f));
            Ui.Button(rect, ButtonColor, () => onClick());
            Ui.Label(Ui.Stretch("Label", rect), label, 34, TextAnchor.MiddleCenter, TextColor);
        }
    }
}

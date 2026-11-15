using System;
using System.Collections;
using System.Collections.Generic;
using Featherguard.Feedback;
using Featherguard.Meta;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public readonly struct MapLandLabel
    {
        public readonly Transform Anchor;
        public readonly string Name;
        public readonly bool Open;
        public readonly int StoryWon;
        public readonly int StoryTotal;

        public MapLandLabel(Transform anchor, string name, bool open, int storyWon, int storyTotal)
        {
            Anchor = anchor;
            Name = name;
            Open = open;
            StoryWon = storyWon;
            StoryTotal = storyTotal;
        }
    }

    public readonly struct MapMarkerLabel
    {
        public readonly Transform Anchor;
        public readonly string Name;
        public readonly bool Story;
        public readonly ThreatRating Rating;
        public readonly bool Daring;

        public MapMarkerLabel(Transform anchor, string name, bool story, ThreatRating rating, bool daring)
        {
            Anchor = anchor;
            Name = name;
            Story = story;
            Rating = rating;
            Daring = daring;
        }
    }

    public class WorldMapHud : MonoBehaviour
    {
        const float ToastHold = 1.6f;
        const float ToastFade = 0.4f;

        static readonly Color LandColor = new(0.9f, 0.86f, 0.78f);
        static readonly Color LockedColor = new(0.55f, 0.56f, 0.6f);
        static readonly Color StoryColor = new(1f, 0.8f, 0.42f);
        static readonly Color ToastColor = new(0.95f, 0.78f, 0.45f);
        static readonly Color ButtonColor = new(0.1f, 0.1f, 0.13f, 0.92f);

        readonly List<(Transform Anchor, RectTransform Label, Vector2 Offset)> labels = new();
        Camera sceneCamera;
        Canvas canvas;
        Text toast;
        Coroutine toastRoutine;

        public void Init(Camera camera, IReadOnlyList<MapLandLabel> lands, IReadOnlyList<MapMarkerLabel> markers, int storyWon, int storyCount,
            Action onBack, Action onPrototype)
        {
            sceneCamera = camera;
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var root = CreateCanvas();
            canvas = root.GetComponent<Canvas>();

            var layer = Ui.Stretch("Labels", root);
            foreach (var land in lands)
            {
                var label = Follow(layer, land.Anchor, new Vector2(520f, 80f), Vector2.zero);
                Ui.Label(Ui.Rect("Name", label, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(520f, 46f)),
                    land.Name, 40, TextAnchor.MiddleCenter, land.Open ? LandColor : LockedColor);
                Ui.Label(Ui.Rect("State", label, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(520f, 32f)),
                    land.Open ? $"Story {land.StoryWon} / {land.StoryTotal}" : "Locked", 26, TextAnchor.MiddleCenter, land.Open ? LandColor : LockedColor);
            }
            foreach (var marker in markers)
            {
                var label = Follow(layer, marker.Anchor, new Vector2(300f, 60f), new Vector2(0f, 60f));
                var color = marker.Story ? StoryColor : Color.white;
                Ui.Label(Ui.Rect("Name", label, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(300f, 32f)),
                    marker.Name, 24, TextAnchor.MiddleCenter, color);
                Ui.Label(Ui.Rect("Rating", label, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(300f, 26f)),
                    marker.Story ? $"Main quest · {marker.Rating}" : marker.Daring ? $"{marker.Rating} · Daring +50%" : marker.Rating.ToString(), 20, TextAnchor.MiddleCenter, color);
            }

            var safe = SafeArea.Create(root);
            Button(safe, "Back", new Vector2(0f, 1f), new Vector2(24f, -24f), onBack);
            Button(safe, "Prototype run", new Vector2(0f, 0f), new Vector2(24f, 24f), onPrototype, 320f);
            Ui.Label(Ui.Rect("Title", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(900f, 70f)),
                "World Map", 60, TextAnchor.MiddleCenter, StoryColor);
            Ui.Label(Ui.Rect("Story", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(900f, 44f)),
                storyWon >= storyCount ? "The story is complete" : $"Story missions {storyWon} / {storyCount}", 32, TextAnchor.MiddleCenter, Color.white);
            toast = Ui.Label(Ui.Rect("Toast", safe, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(1500f, 70f)),
                "", 40, TextAnchor.MiddleCenter, ToastColor);
        }

        public void Toast(string text)
        {
            FeedbackHub.Ui(SfxId.Click);
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ShowToast(text));
        }

        void LateUpdate()
        {
            if (sceneCamera == null) return;
            float scale = canvas.scaleFactor;
            foreach (var (anchor, label, offset) in labels)
            {
                if (anchor == null) continue;
                var screen = sceneCamera.WorldToScreenPoint(anchor.position);
                label.gameObject.SetActive(screen.z > 0f);
                label.anchoredPosition = new Vector2(screen.x, screen.y) / scale + offset;
            }
        }

        RectTransform Follow(RectTransform layer, Transform anchor, Vector2 size, Vector2 offset)
        {
            var label = Ui.Rect("Label", layer, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, size);
            labels.Add((anchor, label, offset));
            return label;
        }

        IEnumerator ShowToast(string text)
        {
            toast.text = text;
            toast.color = ToastColor;
            yield return new WaitForSecondsRealtime(ToastHold);
            yield return Tweens.Fade(toast, ToastFade);
        }

        static void Button(RectTransform parent, string label, Vector2 anchor, Vector2 position, Action onClick, float width = 240f)
        {
            var rect = Ui.Rect(label, parent, anchor, anchor, position, new Vector2(width, 84f));
            Ui.Button(rect, ButtonColor, () => onClick());
            Ui.Label(Ui.Stretch("Label", rect), label, 34, TextAnchor.MiddleCenter, LandColor);
        }

        static RectTransform CreateCanvas()
        {
            var canvasObject = new GameObject("WorldMapHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = canvasObject.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            return (RectTransform)canvasObject.transform;
        }
    }
}

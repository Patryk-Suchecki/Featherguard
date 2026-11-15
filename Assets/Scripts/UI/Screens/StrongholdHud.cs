using System;
using System.Collections;
using System.Collections.Generic;
using Featherguard.Feedback;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public class StrongholdHud : MonoBehaviour
    {
        const float ToastHold = 1.1f;
        const float ToastFade = 0.4f;

        static readonly Color PlaceNameColor = new(0.86f, 0.82f, 0.74f);
        static readonly Color ToastColor = new(0.95f, 0.78f, 0.45f);
        static readonly Color SettingsColor = new(0.1f, 0.1f, 0.13f, 0.9f);

        readonly List<(Transform Anchor, RectTransform Label)> labels = new();
        Camera sceneCamera;
        Canvas canvas;
        RectTransform labelLayer;
        RectTransform settingsButton;
        Text toast;
        Coroutine toastRoutine;

        public void Init(Camera camera, IEnumerable<(Transform Anchor, string Name)> places, Action onSettings)
        {
            sceneCamera = camera;
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var root = CreateCanvas();
            canvas = root.GetComponent<Canvas>();

            labelLayer = Ui.Stretch("Labels", root);
            foreach (var (anchor, name) in places)
            {
                var label = Ui.Rect(name, labelLayer, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(520f, 50f));
                Ui.Label(label, name, 34, TextAnchor.LowerCenter, PlaceNameColor);
                labels.Add((anchor, label));
            }

            var safe = SafeArea.Create(root);
            var settings = settingsButton = Ui.Rect("Settings", safe, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(240f, 84f));
            Ui.Button(settings, SettingsColor, () => onSettings());
            Ui.Label(Ui.Stretch("Label", settings), "Settings", 34, TextAnchor.MiddleCenter, PlaceNameColor);

            toast = Ui.Label(Ui.Rect("Toast", safe, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(1200f, 70f)),
                "", 46, TextAnchor.MiddleCenter, ToastColor);
        }

        public void ComingSoon(string place) => Toast($"{place}: coming soon");

        public void Toast(string text)
        {
            FeedbackHub.Ui(SfxId.Click);
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ShowToast(text));
        }

        public void SetOverviewVisible(bool visible)
        {
            labelLayer.gameObject.SetActive(visible);
            settingsButton.gameObject.SetActive(visible);
        }

        void LateUpdate()
        {
            if (sceneCamera == null) return;
            float scale = canvas.scaleFactor;
            foreach (var (anchor, label) in labels)
            {
                var screen = sceneCamera.WorldToScreenPoint(anchor.position);
                bool visible = screen.z > 0f;
                label.gameObject.SetActive(visible);
                if (visible) label.anchoredPosition = new Vector2(screen.x, screen.y) / scale;
            }
        }

        IEnumerator ShowToast(string text)
        {
            toast.text = text;
            toast.color = ToastColor;
            yield return new WaitForSecondsRealtime(ToastHold);
            yield return Tweens.Fade(toast, ToastFade);
        }

        static RectTransform CreateCanvas()
        {
            var canvasObject = new GameObject("StrongholdHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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

using System;
using System.Collections.Generic;
using Featherguard.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public enum SeatState { InParty, Resting, Empty }

    public class CampHud : MonoBehaviour
    {
        static readonly Color PartyColor = new(1f, 0.8f, 0.42f);
        static readonly Color RestingColor = new(0.78f, 0.76f, 0.72f);
        static readonly Color EmptyColor = new(0.55f, 0.56f, 0.6f);
        static readonly Color ButtonColor = new(0.1f, 0.1f, 0.13f, 0.92f);
        static readonly Color PanelColor = new(0.09f, 0.08f, 0.1f, 1f);
        static readonly Color SoonColor = new(0.6f, 0.6f, 0.62f);

        readonly List<(Transform Anchor, Text Name, Text State)> seats = new();
        Camera sceneCamera;
        Canvas canvas;
        RectTransform root;
        CanvasGroup group;
        Text partyCount;
        Overlay details;

        public bool DetailsOpen => details != null;

        public void Init(Camera camera, IReadOnlyList<Transform> seatAnchors, Action onBack, Action onStorage, Action onPotions)
        {
            sceneCamera = camera;
            var canvasObject = new GameObject("CampHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            root = (RectTransform)canvasObject.transform;
            group = canvasObject.AddComponent<CanvasGroup>();

            var labels = Ui.Stretch("Seats", root);
            foreach (var anchor in seatAnchors)
            {
                var label = Ui.Rect("Seat", labels, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(360f, 90f));
                var name = Ui.Label(Ui.Rect("Name", label, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(360f, 44f)),
                    "", 34, TextAnchor.MiddleCenter, RestingColor);
                var state = Ui.Label(Ui.Rect("State", label, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(360f, 36f)),
                    "", 26, TextAnchor.MiddleCenter, RestingColor);
                seats.Add((anchor, name, state));
            }

            var safe = SafeArea.Create(root);
            Button(safe, "Back", new Vector2(0f, 1f), new Vector2(24f, -24f), onBack);
            Button(safe, "Storage", new Vector2(1f, 0f), new Vector2(-24f, 24f), onStorage);
            Button(safe, "Potions", new Vector2(1f, 0f), new Vector2(-284f, 24f), onPotions);
            Ui.Label(Ui.Rect("Title", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(900f, 70f)),
                "Camp", 60, TextAnchor.MiddleCenter, PartyColor);
            partyCount = Ui.Label(Ui.Rect("Party", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(900f, 44f)),
                "", 34, TextAnchor.MiddleCenter, Color.white);
            Ui.Label(Ui.Rect("Hint", safe, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(32f, 40f), new Vector2(900f, 40f)),
                "Tap a hero: into or out of the party  ·  Hold: details", 28, TextAnchor.MiddleLeft, RestingColor);
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
            group.interactable = visible;
            if (!visible) CloseDetails();
        }

        public void Refresh(IReadOnlyList<(string Name, SeatState State)> states, int inParty, int partySize)
        {
            for (int i = 0; i < seats.Count && i < states.Count; i++)
            {
                var (name, state) = states[i];
                var color = state switch { SeatState.InParty => PartyColor, SeatState.Resting => RestingColor, _ => EmptyColor };
                seats[i].Name.text = state == SeatState.Empty ? "?" : name;
                seats[i].Name.color = color;
                seats[i].State.text = state switch { SeatState.InParty => "In party", SeatState.Resting => "Resting", _ => "Not joined yet" };
                seats[i].State.color = color;
            }
            partyCount.text = $"Party {inParty} / {partySize}";
            partyCount.color = inParty == partySize ? Color.white : PartyColor;
        }

        public void ShowDetails(UnitDefinition hero, bool inParty)
        {
            CloseDetails();
            details = Overlay.Create(root, "Hero", 0.6f);
            var close = details.Dim.gameObject.AddComponent<Button>();
            close.transition = Selectable.Transition.None;
            close.onClick.AddListener(CloseDetails);

            var panel = Ui.Centered("Panel", details.Content, Vector2.zero, new Vector2(1100f, 660f));
            Ui.Panel(panel, PanelColor, true);
            Ui.Label(Ui.Rect("Name", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -36f), new Vector2(700f, 70f)),
                hero.displayName, 60, TextAnchor.MiddleLeft, PartyColor);
            Ui.Label(Ui.Rect("Party", panel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-48f, -48f), new Vector2(360f, 44f)),
                inParty ? "In party" : "Resting", 32, TextAnchor.MiddleRight, inParty ? PartyColor : RestingColor);
            Line(panel, 0, "Level", "coming soon", SoonColor);
            Line(panel, 1, "Attack", hero.attack != null ? hero.attack.displayName : "–", Color.white);
            Line(panel, 2, "Support", hero.support != null ? hero.support.displayName : "–", Color.white);
            Line(panel, 3, "Rage", hero.special != null ? hero.special.displayName : "–", Color.white);
            Line(panel, 4, "Weapon", "coming soon", SoonColor);
            Line(panel, 5, "Artifact", "coming soon", SoonColor);
            Ui.Label(Ui.Rect("Close", panel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(900f, 36f)),
                "Tap outside to close", 26, TextAnchor.MiddleCenter, SoonColor);
        }

        public void CloseDetails()
        {
            details?.Close();
            details = null;
        }

        void LateUpdate()
        {
            if (sceneCamera == null || group.alpha < 0.5f) return;
            float scale = canvas.scaleFactor;
            foreach (var (anchor, name, _) in seats)
            {
                var label = (RectTransform)name.transform.parent;
                var screen = sceneCamera.WorldToScreenPoint(anchor.position);
                label.gameObject.SetActive(screen.z > 0f);
                label.anchoredPosition = new Vector2(screen.x, screen.y) / scale;
            }
        }

        static void Line(RectTransform panel, int row, string title, string value, Color valueColor)
        {
            float y = -150f - row * 64f;
            Ui.Label(Ui.Rect(title, panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, y), new Vector2(300f, 48f)),
                title, 34, TextAnchor.MiddleLeft, RestingColor);
            Ui.Label(Ui.Rect($"{title} Value", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(360f, y), new Vector2(690f, 48f)),
                value, 34, TextAnchor.MiddleLeft, valueColor);
        }

        static void Button(RectTransform parent, string label, Vector2 anchor, Vector2 position, Action onClick)
        {
            var rect = Ui.Rect(label, parent, anchor, anchor, position, new Vector2(240f, 84f));
            Ui.Button(rect, ButtonColor, () => onClick());
            Ui.Label(Ui.Stretch("Label", rect), label, 34, TextAnchor.MiddleCenter, RestingColor);
        }
    }
}

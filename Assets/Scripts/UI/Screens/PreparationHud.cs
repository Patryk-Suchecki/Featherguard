using System;
using System.Collections;
using System.Collections.Generic;
using Featherguard.Feedback;
using Featherguard.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class PreparationInfo
    {
        public string Name;
        public bool Story;
        public ActivityType Type;
        public int Waves;
        public string Land;
        public string Law;
        public ThreatRating Rating;
        public bool Daring;
        public RewardKind Reward;
        public IReadOnlyList<string> Party;
        public bool PartyComplete;
        public string Potions;
    }

    public class PreparationHud : MonoBehaviour
    {
        const float ToastHold = 1.6f;
        const float ToastFade = 0.4f;

        static readonly Color TextColor = new(0.9f, 0.86f, 0.78f);
        static readonly Color MutedColor = new(0.66f, 0.66f, 0.72f);
        static readonly Color StoryColor = new(1f, 0.8f, 0.42f);
        static readonly Color WarningColor = new(1f, 0.45f, 0.38f);
        static readonly Color ButtonColor = new(0.1f, 0.1f, 0.13f, 0.92f);
        static readonly Color PanelColor = new(0.05f, 0.05f, 0.08f, 0.82f);

        Text toast;
        Coroutine toastRoutine;
        RectTransform root;
        Text potions;
        Overlay picker;

        public void Init(PreparationInfo info, Action onBack, Action onChangeParty, Action onPotions, Action onFight)
        {
            root = ScreenCanvas.Create("PreparationHUD");
            var safe = SafeArea.Create(root);

            var panel = Ui.Rect("Details", safe, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -132f), new Vector2(660f, 690f));
            Ui.Panel(panel, PanelColor);
            float y = -28f;
            if (info.Story)
                y = Line(panel, "Main quest", 28, StoryColor, y, 36f);
            y = Line(panel, info.Name, 52, info.Story ? StoryColor : Color.white, y, 64f);
            y = Line(panel, ActivityText.Describe(info.Type, info.Waves), 30, TextColor, y, 44f);
            y -= 18f;
            y = Line(panel, info.Land, 34, TextColor, y, 44f);
            Ui.Wrapped(Ui.Rect("Law", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, y), new Vector2(596f, 110f)),
                string.IsNullOrEmpty(info.Law) ? "No Law" : $"Law: {info.Law}", 26, TextAnchor.UpperLeft, MutedColor);
            y -= 128f;
            y = Line(panel, info.Daring ? $"Threat: {info.Rating}   ·   Daring +50%" : $"Threat: {info.Rating}", 34, ActivityText.RatingColor(info.Rating), y, 48f);
            y = Line(panel, info.Reward == RewardKind.None ? "No reward" : $"Reward: {ActivityText.RewardName(info.Reward)}", 34, Ui.GoldColor, y, 48f);
            y -= 18f;
            y = Line(panel, "Party", 30, TextColor, y, 40f);
            y = Line(panel, info.PartyComplete ? string.Join(", ", info.Party) : "Pick 3 heroes in the Camp", 28, info.PartyComplete ? Color.white : WarningColor, y, 40f);
            potions = Ui.Label(Ui.Rect("Potions", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, y), new Vector2(596f, 40f)), "", 28, TextAnchor.MiddleLeft, TextColor);
            SetPotions(info.Potions);

            Button(safe, "Back", new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(240f, 84f), ButtonColor, onBack);
            Button(safe, "Change party", new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(320f, 96f), ButtonColor, onChangeParty);
            Button(safe, "Potions", new Vector2(0f, 0f), new Vector2(364f, 24f), new Vector2(320f, 96f), ButtonColor, onPotions);
            Button(safe, "Fight", new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(420f, 132f), Ui.GoColor, onFight, 52);
            toast = Ui.Label(Ui.Rect("Toast", safe, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(240f, 170f), new Vector2(1200f, 70f)),
                "", 40, TextAnchor.MiddleCenter, StoryColor);
        }

        public void SetPotions(string text) => potions.text = string.IsNullOrEmpty(text) ? "Potions: none" : $"Potions: {text}";

        public RectTransform OpenPicker()
        {
            ClosePicker();
            picker = Overlay.Create(root, "Potions", 0.75f);
            return picker.Content;
        }

        public void ClosePicker()
        {
            picker?.Close();
            picker = null;
        }

        public void Toast(string text)
        {
            FeedbackHub.Ui(SfxId.Click);
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ShowToast(text));
        }

        IEnumerator ShowToast(string text)
        {
            toast.text = text;
            toast.color = StoryColor;
            yield return new WaitForSecondsRealtime(ToastHold);
            yield return Tweens.Fade(toast, ToastFade);
        }

        static float Line(RectTransform panel, string text, int size, Color color, float y, float height)
        {
            Ui.Label(Ui.Rect("Line", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, y), new Vector2(596f, height)), text, size, TextAnchor.MiddleLeft, color);
            return y - height;
        }

        static void Button(RectTransform parent, string label, Vector2 anchor, Vector2 position, Vector2 size, Color color, Action onClick, int fontSize = 34)
        {
            var rect = Ui.Rect(label, parent, anchor, anchor, position, size);
            Ui.Button(rect, color, () => onClick());
            Ui.Label(Ui.Stretch("Label", rect), label, fontSize, TextAnchor.MiddleCenter, TextColor);
        }
    }
}

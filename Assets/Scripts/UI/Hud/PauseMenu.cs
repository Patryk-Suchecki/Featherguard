using System;
using Featherguard.Core;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.UI
{
    public sealed class PauseMenu
    {
        readonly RectTransform canvas;
        readonly Action onAbandon;
        readonly Func<bool> canAbandon;
        Overlay open;

        public bool IsOpen => open != null;

        public PauseMenu(RectTransform canvas, Action onAbandon, Func<bool> canAbandon)
        {
            this.canvas = canvas;
            this.onAbandon = onAbandon;
            this.canAbandon = canAbandon;
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        void Open()
        {
            FeedbackHub.Ui(SfxId.Click);
            GameClock.Paused = true;
            open = Overlay.Create(canvas, "Pause", 0.6f);
            var menu = Ui.Centered("Menu", open.Content, Vector2.zero, new Vector2(640f, 620f));
            Ui.Panel(menu, Ui.PanelColor);
            Ui.Label(Ui.Centered("Title", menu, new Vector2(0f, 220f), new Vector2(600f, 100f)), "PAUSED", 80, TextAnchor.MiddleCenter, Ui.GoldColor);
            Ui.TextButton(menu, "Resume", new Vector2(0f, 80f), new Vector2(500f, 120f), Ui.GoColor, Close, "Flag");
            bool muted = VolumeSetting.Muted;
            Ui.TextButton(menu, muted ? "Sound: off" : "Sound: on", new Vector2(0f, -55f), new Vector2(500f, 120f), Ui.NeutralColor, ToggleSound, muted ? "Mute" : "Sound");
            var abandon = Ui.TextButton(menu, "Abandon run", new Vector2(0f, -190f), new Vector2(500f, 120f), Ui.RiskColor, Abandon, "Skull");
            abandon.gameObject.SetActive(canAbandon());
        }

        void Close()
        {
            open?.Close();
            open = null;
            GameClock.Paused = false;
        }

        void ToggleSound()
        {
            VolumeSetting.ToggleMute();
            Close();
            Open();
        }

        void Abandon()
        {
            Close();
            onAbandon();
        }
    }
}

using System;
using System.Collections.Generic;
using Featherguard.Battle;
using Featherguard.Run;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public class BattleHud : MonoBehaviour, IBattleView
    {
        static readonly Color CalloutColor = new(1f, 0.45f, 0.15f);

        Camera view;
        RectTransform canvas;
        RectTransform safe;
        RectTransform barLayer;
        RectTransform popupLayer;
        TopBar topBar;
        RageButtonView rageButton;
        PauseMenu pauseMenu;
        Text banner;
        Text hint;
        Text callout;
        float bannerUntil;
        float calloutStart = -10f;
        IPlayerCommands commands;
        RageMeter rage;
        readonly Dictionary<BattleUnit, UnitBar> bars = new();
        readonly List<HeroCardView> cards = new();

        public RectTransform Canvas => canvas;
        public int OverlayIndex => safe.GetSiblingIndex() + 1;

        public void Init(Camera camera, Action onAbandon)
        {
            view = camera;
            canvas = CreateCanvas();
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            barLayer = Ui.Stretch("Bars", canvas);
            popupLayer = Ui.Stretch("Popups", canvas);
            safe = SafeArea.Create(canvas);
            topBar = new TopBar(safe);
            pauseMenu = new PauseMenu(canvas, onAbandon, () => RunSession.Current != null);
            var pause = Ui.Rect("Pause", SafeArea.Create(canvas), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -14f), new Vector2(92f, 92f));
            Ui.Button(pause, Ui.PanelColor, pauseMenu.Toggle, "Circle");
            Ui.Icon(pause, "Pause", Vector2.zero, 54f);

            banner = Ui.Label(Ui.Rect("Banner", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1200f, 90f)),
                "", 70, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.5f));
            hint = Ui.Label(Ui.Rect("Hint", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(1000f, 50f)),
                "", 32, TextAnchor.MiddleCenter, Color.white);
            callout = Ui.Label(Ui.Rect("Callout", safe, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 180f), new Vector2(1600f, 140f)),
                "", 100, TextAnchor.MiddleCenter, CalloutColor);
        }

        static RectTransform CreateCanvas()
        {
            var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = canvasObject.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 10;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            return (RectTransform)canvasObject.transform;
        }

        public void BindWallet(Wallet wallet) => topBar.Bind(wallet);

        public void BindBattle(IPlayerCommands playerCommands, RageMeter rageMeter)
        {
            commands = playerCommands;
            rage = rageMeter;
            rageButton = new RageButtonView(safe, commands.ToggleSpecial);
        }

        public void SetHudVisible(bool visible)
        {
            var group = safe.GetComponent<CanvasGroup>();
            if (group == null) group = safe.gameObject.AddComponent<CanvasGroup>();
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
            barLayer.gameObject.SetActive(visible);
        }

        public Overlay OpenOverlay(string name, float alpha = 0.72f)
        {
            hint.text = "";
            return Overlay.Create(canvas, name, alpha, OverlayIndex);
        }

        public void ShowBanner(string text)
        {
            banner.text = text;
            bannerUntil = Time.unscaledTime + 1.6f;
        }

        public void Callout(string text)
        {
            callout.text = text;
            calloutStart = Time.unscaledTime;
        }

        public void Popup(Vector3 world, string text, PopupKind kind) => UI.Popup.Spawn(popupLayer, view, world, text, kind);

        public void SetProgress(int encounter, int encounters, int wave, int waves) => topBar.SetProgress(encounter, encounters, wave, waves);

        public void UnitAdded(BattleUnit unit)
        {
            if (unit.Team == Team.Enemies)
                bars[unit] = UnitBar.Create(unit, barLayer, view);
        }

        public void UnitRemoved(BattleUnit unit)
        {
            if (bars.Remove(unit, out var bar) && bar != null)
                Destroy(bar.gameObject);
        }

        public void HeroesReady(IReadOnlyList<BattleUnit> heroes)
        {
            for (int i = 0; i < heroes.Count; i++)
                cards.Add(new HeroCardView(safe, heroes[i], i, hero => commands?.Select(hero)));
        }

        void OnDestroy()
        {
            topBar?.Unbind();
            foreach (var card in cards)
                card.Unbind();
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                pauseMenu.Toggle();

            banner.color = WithAlpha(banner.color, Mathf.Clamp01((bannerUntil - Time.unscaledTime) / 0.4f));
            float calloutAge = Time.unscaledTime - calloutStart;
            callout.color = WithAlpha(callout.color, calloutAge < 1.1f ? 1f : Mathf.Clamp01(1f - (calloutAge - 1.1f) / 0.3f));
            callout.rectTransform.localScale = Vector3.one * (calloutAge < 0.15f ? Mathf.Lerp(1.6f, 1f, calloutAge / 0.15f) : 1f);

            if (commands == null) return;
            rageButton.Update(rage, commands);
            foreach (var card in cards)
                card.Animate(commands);
            hint.text = HintText();
        }

        string HintText()
        {
            if (!commands.AcceptingInput) return "";
            var selected = commands.Selected;
            if (selected == null) return "Dotknij bohatera";
            var definition = selected.Definition;
            if (commands.SpecialArmed)
                return $"{definition.special.displayName}: dotknij celu";
            return $"Wróg: {definition.attack.displayName}  ·  Sojusznik: {definition.support.displayName}";
        }

        static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);
    }
}

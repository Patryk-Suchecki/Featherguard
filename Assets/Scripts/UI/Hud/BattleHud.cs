using System;
using System.Collections.Generic;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public class BattleHud : MonoBehaviour, IBattleView, ITimingCueView
    {
        Camera view;
        RectTransform canvas;
        RectTransform safe;
        RectTransform barLayer;
        RectTransform popupLayer;
        TopBar topBar;
        TimingCueView timingCue;
        DragArrowView dragArrow;
        RageButtonView rageButton;
        PotionBeltView potionBelt;
        PauseMenu pauseMenu;
        BannerView banners;
        Text hint;
        IPlayerCommands commands;
        RageMeter rage;
        readonly Dictionary<UnitView, UnitBar> bars = new();

        public RectTransform Canvas => canvas;
        public int OverlayIndex => safe.GetSiblingIndex() + 1;

        public BattleVisuals Visuals { get; private set; }

        public void Init(Camera camera, BattleVisuals visuals, Action onAbandon, Func<bool> canAbandon)
        {
            Visuals = visuals;
            view = camera;
            canvas = CreateCanvas();
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            barLayer = Ui.Stretch("Bars", canvas);
            dragArrow = new DragArrowView(canvas, camera);
            popupLayer = Ui.Stretch("Popups", canvas);
            timingCue = new TimingCueView(Ui.Stretch("Timing", canvas), camera);
            safe = SafeArea.Create(canvas);
            topBar = new TopBar(safe);
            pauseMenu = new PauseMenu(canvas, onAbandon, canAbandon);
            var pause = Ui.Rect("Pause", SafeArea.Create(canvas), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -14f), new Vector2(92f, 92f));
            Ui.Button(pause, Ui.PanelColor, pauseMenu.Toggle, "Circle");
            Ui.Icon(pause, "Pause", Vector2.zero, 54f);

            banners = new BannerView(safe);
            hint = Ui.Label(Ui.Rect("Hint", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(1000f, 50f)),
                "", 32, TextAnchor.MiddleCenter, Color.white);
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

        public void BindBattle(IPlayerCommands playerCommands, RageMeter rageMeter)
        {
            commands = playerCommands;
            rage = rageMeter;
            rageButton = new RageButtonView(safe, commands.ToggleSpecial);
        }

        public void BindPotions(IPotionCommands potions, PotionPouch pouch) => potionBelt = new PotionBeltView(safe, potions, pouch);

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

        public void ShowBanner(string text) => banners.ShowBanner(text);

        public void Callout(string text) => banners.Callout(text);

        public void Popup(Vector3 world, string text, PopupKind kind) => UI.Popup.Spawn(popupLayer, view, world, text, kind);

        public void SetProgress(int encounter, int encounters, int wave, int waves) => topBar.SetProgress(encounter, encounters, wave, waves);

        public void ShowGesture(GestureSketch sketch) => timingCue.ShowGesture(sketch);

        public void ShowBlockCue(Vector3 world) => timingCue.ShowBlockCue(world);

        public void HideTimingCue() => timingCue.Hide();

        public void UnitAdded(UnitView unit)
        {
            bars[unit] = UnitBar.Create(unit, barLayer, view, Visuals.Statuses);
        }

        public void UnitRemoved(UnitView unit)
        {
            if (bars.Remove(unit, out var bar) && bar != null)
                Destroy(bar.gameObject);
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                pauseMenu.Toggle();

            banners.Update();

            if (commands == null) return;
            rageButton.Update(rage, commands);
            dragArrow.Update(commands);
            potionBelt?.Update();
            hint.text = HintText();
        }

        string HintText()
        {
            if (!commands.AcceptingInput) return "";
            if (commands.HeldPotion != null)
                return $"{commands.HeldPotion.displayName}: drop on a glowing target";
            var selected = commands.Selected;
            if (selected == null)
                return commands.SpecialArmed ? "Rage: drag or tap a glowing hero" : "Drag a glowing hero: enemy = attack, ally or tap = skill";
            var definition = selected.Definition;
            if (commands.SpecialArmed)
                return definition.special.TargetsAll ? $"{definition.special.displayName}: let go" : $"{definition.special.displayName}: drop on a target";
            if (definition.support == null)
                return $"Enemy: {definition.attack.displayName}";
            return $"Enemy: {definition.attack.displayName}  ·  Ally or tap: {definition.support.displayName}";
        }
    }
}

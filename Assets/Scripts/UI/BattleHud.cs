using System.Collections.Generic;
using Featherguard.Battle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public class BattleHud : MonoBehaviour
    {
        static readonly Color CardIdle = new(0.08f, 0.1f, 0.16f, 0.88f);
        static readonly Color CardSelected = new(0.62f, 0.42f, 0.08f, 0.95f);
        static readonly Color RageColor = new(1f, 0.45f, 0.15f);

        BattleManager battle;
        Camera view;
        RectTransform canvas;
        RectTransform barLayer;
        RectTransform popupLayer;
        Text wave;
        Text gold;
        Text banner;
        Text hint;
        Text callout;
        RectTransform rageFill;
        RectTransform rageButton;
        Text rageButtonLabel;
        float bannerUntil;
        float calloutStart = -10f;
        readonly Dictionary<BattleUnit, UnitBar> bars = new();
        readonly List<(BattleUnit unit, Image background, CanvasGroup group, Text title)> cards = new();

        public void Init(BattleManager owner, Camera camera)
        {
            battle = owner;
            view = camera;

            var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = canvasObject.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 10;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvas = (RectTransform)canvasObject.transform;

            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            barLayer = Ui.Stretch("Bars", canvas);
            popupLayer = Ui.Stretch("Popups", canvas);

            wave = Ui.Label(Ui.Rect("Wave", canvas, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(400f, 50f)),
                "", 36, TextAnchor.UpperLeft, Color.white);
            gold = Ui.Label(Ui.Rect("Gold", canvas, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -30f), new Vector2(400f, 50f)),
                "", 36, TextAnchor.UpperRight, new Color(1f, 0.82f, 0.2f));
            RefreshGold();
            banner = Ui.Label(Ui.Rect("Banner", canvas, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 90f)),
                "", 68, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.5f));
            hint = Ui.Label(Ui.Rect("Hint", canvas, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -135f), new Vector2(1600f, 44f)),
                "", 28, TextAnchor.MiddleCenter, Color.white);
            callout = Ui.Label(Ui.Rect("Callout", canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(1600f, 140f)),
                "", 104, TextAnchor.MiddleCenter, RageColor);

            BuildRage();
        }

        void BuildRage()
        {
            var frame = Ui.Rect("Rage", canvas, Vector2.zero, Vector2.zero, new Vector2(40f, 40f), new Vector2(420f, 46f));
            Ui.Image(frame, new Color(0.05f, 0.05f, 0.08f, 0.85f));
            rageFill = Ui.Stretch("Fill", frame, 4f);
            Ui.Image(rageFill, RageColor);
            Ui.Label(Ui.Stretch("Label", frame), "FURIA", 26, TextAnchor.MiddleCenter, Color.white);

            rageButton = Ui.Rect("RageButton", canvas, Vector2.zero, new Vector2(0.5f, 0f), new Vector2(250f, 100f), new Vector2(420f, 70f));
            Ui.Button(rageButton, new Color(0.85f, 0.25f, 0.1f, 0.95f), battle.ToggleSpecial);
            rageButtonLabel = Ui.Label(Ui.Stretch("Label", rageButton), "", 26, TextAnchor.MiddleCenter, Color.white);
        }

        public void BuildHeroCards()
        {
            const float width = 330f;
            const float spacing = 20f;
            var heroes = battle.Heroes;
            float start = -(heroes.Count - 1) * (width + spacing) * 0.5f;
            for (int i = 0; i < heroes.Count; i++)
            {
                var hero = heroes[i];
                var rect = Ui.Rect(hero.Definition.displayName + "Card", canvas, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(start + i * (width + spacing) + 180f, 30f), new Vector2(width, 120f));
                var button = Ui.Button(rect, CardIdle, () => battle.SelectFromCard(hero));
                var group = rect.gameObject.AddComponent<CanvasGroup>();
                var title = Ui.Label(Ui.Rect("Title", rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(width - 20f, 32f)),
                    hero.Definition.displayName, 28, TextAnchor.UpperCenter, Color.white);
                var def = hero.Definition;
                Ui.Label(Ui.Rect("Body", rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(width - 20f, 66f)),
                    $"Wróg: {def.attack.displayName}\nSojusznik: {def.support.displayName}", 20, TextAnchor.LowerCenter, new Color(0.85f, 0.9f, 1f));
                cards.Add((hero, (Image)button.targetGraphic, group, title));
            }
        }

        public void AddUnitBar(BattleUnit unit) => bars[unit] = UnitBar.Create(unit, barLayer, view);

        public void RemoveUnitBar(BattleUnit unit)
        {
            if (bars.Remove(unit, out var bar) && bar != null)
                Destroy(bar.gameObject);
        }

        public void SetProgress(int encounter, int encounters, int current, int total) =>
            wave.text = total > 1 ? $"Walka {encounter}/{encounters}  ·  Fala {current}/{total}" : $"Walka {encounter}/{encounters}";

        public void RefreshGold() => gold.text = $"{RunState.Gold} zł";

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

        public void Popup(Vector3 world, string text, PopupStyle style) => UI.Popup.Spawn(popupLayer, view, world, text, style);

        public RectTransform Canvas => canvas;

        public void ShowRunEnd(bool victory, int feathers)
        {
            var panel = Overlay("RunEnd", victory ? "WYPRAWA UKOŃCZONA!" : "PORAŻKA", victory ? new Color(1f, 0.85f, 0.25f) : new Color(1f, 0.35f, 0.3f),
                $"Wygrane walki: {RunState.BattlesWon}/{RunState.EncounterCount}   ·   Pokonani wrogowie: {RunState.Kills}   ·   Zdobyte złoto: {RunState.GoldEarned}");
            Ui.Label(Ui.Rect("Feathers", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(1600f, 50f)),
                $"+{feathers} złotych piór  ·  Zebrane karty: {RunState.Taken.Count}", 34, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.3f));
            AddButton(panel, "Do obozu", new Vector2(0f, -150f), new Color(0.2f, 0.55f, 0.25f, 1f), battle.NewRun);
        }

        public RectTransform Overlay(string name, string title, Color titleColor, string subtitle)
        {
            var panel = Ui.Stretch(name, canvas);
            Ui.Image(panel, new Color(0f, 0f, 0f, 0.6f), true);
            Ui.Label(Ui.Rect("Title", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(1600f, 160f)),
                title, 120, TextAnchor.MiddleCenter, titleColor);
            Ui.Label(Ui.Rect("Subtitle", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(1600f, 60f)),
                subtitle, 34, TextAnchor.MiddleCenter, Color.white);
            hint.text = "";
            return panel;
        }

        public static RectTransform AddButton(RectTransform parent, string label, Vector2 position, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var button = Ui.Rect(label, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(420f, 90f));
            Ui.Button(button, color, onClick);
            Ui.Label(Ui.Stretch("Label", button), label, 36, TextAnchor.MiddleCenter, Color.white);
            return button;
        }

        void Update()
        {
            float bannerLeft = bannerUntil - Time.unscaledTime;
            banner.color = WithAlpha(banner.color, Mathf.Clamp01(bannerLeft / 0.4f));

            float calloutAge = Time.unscaledTime - calloutStart;
            callout.color = WithAlpha(callout.color, calloutAge < 1.1f ? 1f : Mathf.Clamp01(1f - (calloutAge - 1.1f) / 0.3f));
            callout.rectTransform.localScale = Vector3.one * (calloutAge < 0.15f ? Mathf.Lerp(1.6f, 1f, calloutAge / 0.15f) : 1f);

            rageFill.anchorMax = new Vector2(Mathf.Max(0.001f, battle.RageFraction), 1f);
            bool canUseRage = battle.AcceptingInput && battle.RageReady && battle.Selected != null;
            rageButton.gameObject.SetActive(canUseRage);
            if (canUseRage)
            {
                rageButtonLabel.text = battle.SpecialArmed ? "Anuluj furię" : $"FURIA: {battle.Selected.Definition.special.displayName}";
                rageButton.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 7f) * 0.04f);
            }

            foreach (var (unit, background, group, title) in cards)
            {
                bool ready = unit.IsAlive && !unit.HasActed;
                background.color = unit == battle.Selected ? CardSelected : CardIdle;
                group.alpha = ready ? 1f : 0.45f;
                title.text = unit.IsAlive ? unit.Definition.displayName : unit.Definition.displayName + " (pokonany)";
            }

            hint.text = HintText();
        }

        string HintText()
        {
            if (!battle.AcceptingInput) return "";
            var selected = battle.Selected;
            if (selected == null) return "Wybierz bohatera";
            var def = selected.Definition;
            if (battle.SpecialArmed)
                return $"FURIA — {def.special.displayName}: wybierz cel";
            return $"{def.displayName}: kliknij wroga ({def.attack.displayName}) lub sojusznika ({def.support.displayName})";
        }

        static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);
    }
}

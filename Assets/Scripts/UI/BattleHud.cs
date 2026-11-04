using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Feel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public class BattleHud : MonoBehaviour
    {
        static readonly Color CardIdle = new(0.07f, 0.08f, 0.13f, 0.92f);
        static readonly Color CardSelected = new(0.45f, 0.3f, 0.06f, 0.96f);
        static readonly Color RageColor = new(1f, 0.45f, 0.15f);
        static readonly Color HeroHealth = new(0.38f, 0.86f, 0.36f);

        class HeroCard
        {
            public BattleUnit Unit;
            public RectTransform Rect;
            public Image Background;
            public Image Frame;
            public CanvasGroup Group;
            public Text Name;
            public Text Health;
            public RectTransform Fill;
            public RectTransform Statuses;
        }

        BattleManager battle;
        Camera view;
        RectTransform canvas;
        RectTransform safe;
        RectTransform barLayer;
        RectTransform popupLayer;
        Text wave;
        Text gold;
        Text banner;
        Text hint;
        Text callout;
        Image rageFill;
        Image rageCore;
        Image rageRing;
        Button rageButton;
        Text rageLabel;
        RectTransform rageRoot;
        RectTransform pauseMenu;
        float bannerUntil;
        float calloutStart = -10f;
        readonly Dictionary<BattleUnit, UnitBar> bars = new();
        readonly List<HeroCard> cards = new();

        public RectTransform Canvas => canvas;
        public RectTransform Safe => safe;
        public Camera View => view;

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
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            canvas = (RectTransform)canvasObject.transform;

            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            barLayer = Ui.Stretch("Bars", canvas);
            popupLayer = Ui.Stretch("Popups", canvas);
            safe = SafeArea.Create(canvas);

            var progress = Ui.Rect("Progress", safe, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -20f), new Vector2(470f, 76f));
            Ui.Sliced(progress, "Pill", Ui.PanelColor);
            Ui.Icon(progress, "Flag", new Vector2(44f, 0f), 54f, anchor: new Vector2(0f, 0.5f));
            wave = Ui.Label(Ui.Stretch("Label", progress), "", 32, TextAnchor.MiddleLeft, Color.white);
            wave.rectTransform.offsetMin = new Vector2(84f, 0f);

            var pause = Ui.Rect("Pause", SafeArea.Create(canvas), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -14f), new Vector2(92f, 92f));
            Ui.Button(pause, Ui.PanelColor, TogglePause, "Circle");
            Ui.Icon(pause, "Pause", Vector2.zero, 54f);

            var purse = Ui.Rect("Gold", safe, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-134f, -20f), new Vector2(250f, 76f));
            Ui.Sliced(purse, "Pill", Ui.PanelColor);
            Ui.Icon(purse, "Coin", new Vector2(42f, 0f), 58f, anchor: new Vector2(0f, 0.5f));
            gold = Ui.Label(Ui.Stretch("Label", purse), "", 38, TextAnchor.MiddleCenter, Ui.GoldColor);
            gold.rectTransform.offsetMin = new Vector2(70f, 0f);
            RefreshGold();

            banner = Ui.Label(Ui.Rect("Banner", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1200f, 90f)),
                "", 70, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.5f));
            hint = Ui.Label(Ui.Rect("Hint", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(1000f, 50f)),
                "", 32, TextAnchor.MiddleCenter, Color.white);
            callout = Ui.Label(Ui.Rect("Callout", safe, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 180f), new Vector2(1600f, 140f)),
                "", 100, TextAnchor.MiddleCenter, RageColor);

            BuildRage();
        }

        void BuildRage()
        {
            rageRoot = Ui.Rect("Rage", safe, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-150f, 150f), new Vector2(230f, 230f));
            Ui.Icon(rageRoot, "Circle", Vector2.zero, 230f, new Color(0.04f, 0.04f, 0.07f, 0.92f));
            rageFill = Ui.Icon(rageRoot, "Circle", Vector2.zero, 222f, RageColor);
            rageFill.type = Image.Type.Filled;
            rageFill.fillMethod = Image.FillMethod.Radial360;
            rageFill.fillOrigin = (int)Image.Origin360.Bottom;
            var core = Ui.Centered("Core", rageRoot, Vector2.zero, new Vector2(176f, 176f));
            rageButton = Ui.Button(core, new Color(0.25f, 0.08f, 0.05f, 1f), battle.ToggleSpecial, "Circle");
            rageCore = (Image)rageButton.targetGraphic;
            Ui.Icon(core, "Rage", new Vector2(0f, 8f), 116f);
            rageRing = Ui.Icon(rageRoot, "Ring", Vector2.zero, 250f, new Color(1f, 1f, 1f, 0f));
            rageLabel = Ui.Label(Ui.Rect("Label", rageRoot, new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 8f), new Vector2(600f, 50f)),
                "", 32, TextAnchor.LowerRight, Color.white);
        }

        public void BuildHeroCards()
        {
            const float width = 370f;
            const float spacing = 16f;
            var heroes = battle.Heroes;
            for (int i = 0; i < heroes.Count; i++)
            {
                var hero = heroes[i];
                var card = new HeroCard { Unit = hero };
                card.Rect = Ui.Rect(hero.Definition.displayName + "Card", safe, Vector2.zero, Vector2.zero, new Vector2(24f + i * (width + spacing), 20f), new Vector2(width, 156f));
                var button = Ui.Button(card.Rect, CardIdle, () => battle.SelectFromCard(hero));
                card.Background = (Image)button.targetGraphic;
                card.Group = card.Rect.gameObject.AddComponent<CanvasGroup>();
                card.Frame = Ui.Sliced(Ui.Stretch("Frame", card.Rect, -4f), "Frame", Ui.GoldColor);

                var portraitFrame = Ui.Rect("Portrait", card.Rect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(128f, 128f));
                Ui.Image(portraitFrame, new Color(0.25f, 0.4f, 0.6f, 1f)).sprite = Ui.Sprite("Circle");
                portraitFrame.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                var portrait = Ui.Stretch("Image", portraitFrame);
                Ui.Image(portrait, Color.white).sprite = hero.Definition.portrait;

                card.Name = Ui.Label(Ui.Rect("Name", card.Rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(152f, -12f), new Vector2(210f, 40f)),
                    hero.Definition.displayName, 34, TextAnchor.UpperLeft, Color.white);
                Ui.Bar(card.Rect, Vector2.zero, new Vector2(200f, 30f), HeroHealth, out card.Fill);
                var barRect = (RectTransform)card.Fill.parent;
                barRect.anchorMin = barRect.anchorMax = new Vector2(0f, 0.5f);
                barRect.pivot = new Vector2(0f, 0.5f);
                barRect.anchoredPosition = new Vector2(152f, 2f);
                card.Health = Ui.Label(Ui.Stretch("Health", barRect), "", 22, TextAnchor.MiddleCenter, Color.white);
                card.Statuses = Ui.Rect("Statuses", card.Rect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(152f, 8f), new Vector2(210f, 48f));
                cards.Add(card);
                hero.Changed += () => RefreshCard(card);
                RefreshCard(card);
            }
        }

        void RefreshCard(HeroCard card)
        {
            var unit = card.Unit;
            Ui.SetFill(card.Fill, (float)unit.Health / unit.MaxHealth);
            card.Health.text = $"{unit.Health}/{unit.MaxHealth}";
            StatusIcons(card.Statuses, unit.Statuses, 46f, false);
        }

        public static void StatusIcons(RectTransform row, IReadOnlyList<StatusEffect> statuses, float size, bool centered)
        {
            for (int i = row.childCount - 1; i >= 0; i--)
                Destroy(row.GetChild(i).gameObject);
            row.DetachChildren();
            float step = size + 6f;
            float start = centered ? -(statuses.Count - 1) * step * 0.5f : size * 0.5f;
            var anchor = centered ? new Vector2(0.5f, 0.5f) : new Vector2(0f, 0.5f);
            for (int i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                var holder = Ui.Rect(status.Type.ToString(), row, anchor, new Vector2(0.5f, 0.5f), new Vector2(start + i * step, 0f), Vector2.one * size);
                Ui.Icon(holder, "Circle", Vector2.zero, size, new Color(0.04f, 0.04f, 0.07f, 0.85f));
                Ui.Icon(holder, status.Type.ToString(), Vector2.zero, size * 0.82f);
                var badge = Ui.Rect("Turns", holder, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-4f, 4f), Vector2.one * size * 0.5f);
                Ui.Image(badge, new Color(0.9f, 0.2f, 0.15f, 1f)).sprite = Ui.Sprite("Circle");
                Ui.Label(Ui.Stretch("Label", badge), status.Turns.ToString(), Mathf.RoundToInt(size * 0.38f), TextAnchor.MiddleCenter, Color.white);
            }
        }

        public void AddUnitBar(BattleUnit unit)
        {
            if (unit.Team == Team.Enemies)
                bars[unit] = UnitBar.Create(unit, barLayer, view);
        }

        public void RemoveUnitBar(BattleUnit unit)
        {
            if (bars.Remove(unit, out var bar) && bar != null)
                Destroy(bar.gameObject);
        }

        public void SetProgress(int encounter, int encounters, int current, int total) =>
            wave.text = total > 1 ? $"Walka {encounter}/{encounters} · Fala {current}/{total}" : $"Walka {encounter}/{encounters}";

        public void RefreshGold() => gold.text = RunState.Gold.ToString();

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

        public void SetHudVisible(bool visible)
        {
            var group = safe.GetComponent<CanvasGroup>();
            if (group == null) group = safe.gameObject.AddComponent<CanvasGroup>();
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
            barLayer.gameObject.SetActive(visible);
        }

        public RectTransform Overlay(string name, out RectTransform dim, float alpha = 0.72f)
        {
            dim = Ui.Stretch(name, canvas);
            Ui.Image(dim, new Color(0f, 0f, 0f, alpha), true);
            hint.text = "";
            return SafeArea.Create(dim);
        }

        public static Text Title(RectTransform parent, string title, string subtitle, Color? color = null)
        {
            var label = Ui.Label(Ui.Rect("Title", parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(1100f, 100f)),
                title, 78, TextAnchor.MiddleCenter, color ?? Ui.GoldColor);
            if (!string.IsNullOrEmpty(subtitle))
                Ui.Label(Ui.Rect("Subtitle", parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -122f), new Vector2(1100f, 44f)),
                    subtitle, 32, TextAnchor.MiddleCenter, Color.white);
            return label;
        }

        public static void PartyStrip(RectTransform parent)
        {
            var strip = Ui.Rect("Party", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(330f, 290f));
            var heroes = RunState.Heroes;
            for (int i = 0; i < heroes.Count; i++)
            {
                var hero = heroes[i];
                var row = Ui.Rect(hero.displayName, strip, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -i * 96f), new Vector2(330f, 86f));
                Ui.Sliced(row, "Pill", Ui.PanelColor);
                var frame = Ui.Rect("Portrait", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(74f, 74f));
                Ui.Image(frame, new Color(0.25f, 0.4f, 0.6f, 1f)).sprite = Ui.Sprite("Circle");
                frame.gameObject.AddComponent<Mask>();
                Ui.Image(Ui.Stretch("Image", frame), Color.white).sprite = hero.portrait;
                int health = RunState.HealthOf(hero);
                int max = RunState.MaxHealthOf(hero);
                Ui.Bar(row, new Vector2(40f, -12f), new Vector2(210f, 22f), HeroHealth, out var fill);
                Ui.SetFill(fill, (float)health / max);
                Ui.Label(Ui.Rect("Health", row, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40f, 18f), new Vector2(210f, 30f)),
                    $"{hero.displayName}  {health}/{max}", 24, TextAnchor.MiddleLeft, Color.white);
            }
            var stats = Ui.Rect("Stats", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-134f, -20f), new Vector2(250f, 160f));
            var goldPill = Ui.Rect("Gold", stats, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(250f, 72f));
            Ui.Sliced(goldPill, "Pill", Ui.PanelColor);
            Ui.Icon(goldPill, "Coin", new Vector2(40f, 0f), 54f, anchor: new Vector2(0f, 0.5f));
            Ui.Label(Ui.Stretch("Label", goldPill), RunState.Gold.ToString(), 36, TextAnchor.MiddleCenter, Ui.GoldColor).rectTransform.offsetMin = new Vector2(66f, 0f);
            var luckPill = Ui.Rect("Luck", stats, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0f, -84f), new Vector2(250f, 72f));
            Ui.Sliced(luckPill, "Pill", Ui.PanelColor);
            Ui.Icon(luckPill, "Clover", new Vector2(40f, 0f), 54f, anchor: new Vector2(0f, 0.5f));
            Ui.Label(Ui.Stretch("Label", luckPill), $"Szczęście {RunState.Luck}", 28, TextAnchor.MiddleCenter, new Color(0.6f, 1f, 0.5f)).rectTransform.offsetMin = new Vector2(66f, 0f);
        }

        public void ShowRunEnd(bool victory, int feathers)
        {
            SetHudVisible(false);
            var panel = Overlay("RunEnd", out _);
            var box = Ui.Centered("Box", panel, Vector2.zero, new Vector2(1100f, 640f));
            Ui.Panel(box, Ui.PanelColor);
            Ui.Icon(box, victory ? "Crown" : "Skull", new Vector2(0f, 230f), 150f);
            Ui.Label(Ui.Centered("Title", box, new Vector2(0f, 100f), new Vector2(1000f, 110f)),
                victory ? "WYPRAWA UKOŃCZONA!" : "PORAŻKA", 86, TextAnchor.MiddleCenter, victory ? Ui.GoldColor : new Color(1f, 0.38f, 0.32f));
            Ui.Label(Ui.Centered("Stats", box, new Vector2(0f, 10f), new Vector2(1000f, 50f)),
                $"Walki: {RunState.BattlesWon}/{RunState.EncounterCount}   ·   Wrogowie: {RunState.Kills}   ·   Złoto: {RunState.GoldEarned}   ·   Karty: {RunState.Taken.Count}",
                32, TextAnchor.MiddleCenter, Color.white);
            var reward = Ui.Centered("Feathers", box, new Vector2(0f, -70f), new Vector2(420f, 80f));
            Ui.Icon(reward, "Feather", new Vector2(-150f, 0f), 72f);
            Ui.Label(Ui.Stretch("Label", reward), $"+{feathers} piór", 48, TextAnchor.MiddleCenter, Ui.GoldColor);
            Ui.TextButton(box, "Do obozu", new Vector2(0f, -210f), new Vector2(480f, 120f), Ui.GoColor, battle.NewRun, "Campfire");
        }

        public void TogglePause()
        {
            if (pauseMenu != null)
            {
                Destroy(pauseMenu.parent.parent.gameObject);
                pauseMenu = null;
                GameClock.Paused = false;
                return;
            }
            HitFeedback.Instance.Play(SfxId.Click);
            GameClock.Paused = true;
            var content = Overlay("Pause", out var dim, 0.6f);
            dim.SetAsLastSibling();
            pauseMenu = Ui.Centered("Menu", content, Vector2.zero, new Vector2(640f, 620f));
            Ui.Panel(pauseMenu, Ui.PanelColor);
            Ui.Label(Ui.Centered("Title", pauseMenu, new Vector2(0f, 220f), new Vector2(600f, 100f)), "PAUZA", 80, TextAnchor.MiddleCenter, Ui.GoldColor);
            Ui.TextButton(pauseMenu, "Wznów", new Vector2(0f, 80f), new Vector2(500f, 120f), Ui.GoColor, TogglePause, "Flag");
            bool muted = AudioListener.volume < 0.5f;
            Ui.TextButton(pauseMenu, muted ? "Dźwięk: wył." : "Dźwięk: wł.", new Vector2(0f, -55f), new Vector2(500f, 120f), Ui.NeutralColor, () =>
            {
                AudioListener.volume = AudioListener.volume < 0.5f ? 1f : 0f;
                PlayerPrefs.SetFloat("volume", AudioListener.volume);
                TogglePause();
                TogglePause();
            }, muted ? "Mute" : "Sound");
            var abandon = Ui.TextButton(pauseMenu, "Porzuć wyprawę", new Vector2(0f, -190f), new Vector2(500f, 120f), Ui.RiskColor, () =>
            {
                GameClock.Paused = false;
                battle.AbandonRun();
            }, "Skull");
            abandon.gameObject.SetActive(RunState.Active);
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                TogglePause();

            float bannerLeft = bannerUntil - Time.unscaledTime;
            banner.color = WithAlpha(banner.color, Mathf.Clamp01(bannerLeft / 0.4f));

            float calloutAge = Time.unscaledTime - calloutStart;
            callout.color = WithAlpha(callout.color, calloutAge < 1.1f ? 1f : Mathf.Clamp01(1f - (calloutAge - 1.1f) / 0.3f));
            callout.rectTransform.localScale = Vector3.one * (calloutAge < 0.15f ? Mathf.Lerp(1.6f, 1f, calloutAge / 0.15f) : 1f);

            UpdateRage();

            foreach (var card in cards)
            {
                var unit = card.Unit;
                bool selected = unit == battle.Selected;
                bool ready = unit.IsAlive && !unit.HasActed;
                card.Background.color = selected ? CardSelected : CardIdle;
                card.Frame.enabled = selected;
                card.Group.alpha = !unit.IsAlive ? 0.35f : ready || !battle.AcceptingInput ? 1f : 0.5f;
                var position = card.Rect.anchoredPosition;
                position.y = Mathf.Lerp(position.y, selected ? 36f : 20f, Time.unscaledDeltaTime * 14f);
                card.Rect.anchoredPosition = position;
                card.Name.text = unit.IsAlive ? unit.Definition.displayName : "Pokonany";
            }

            hint.text = HintText();
        }

        void UpdateRage()
        {
            rageFill.fillAmount = Mathf.MoveTowards(rageFill.fillAmount, battle.RageFraction, Time.unscaledDeltaTime * 1.5f);
            bool ready = battle.RageReady;
            bool usable = ready && battle.AcceptingInput && battle.Selected != null;
            rageButton.interactable = usable;
            rageCore.color = usable ? new Color(0.85f, 0.25f, 0.1f, 1f) : new Color(0.25f, 0.08f, 0.05f, 1f);
            float pulse = ready ? 1f + Mathf.Sin(Time.unscaledTime * 7f) * 0.05f : 1f;
            rageRoot.localScale = Vector3.one * pulse;
            rageRing.color = new Color(1f, 0.95f, 0.7f, battle.SpecialArmed ? 0.6f + Mathf.Sin(Time.unscaledTime * 10f) * 0.4f : 0f);
            if (battle.SpecialArmed)
                rageLabel.text = "Wybierz cel!";
            else if (usable)
                rageLabel.text = battle.Selected.Definition.special.displayName;
            else
                rageLabel.text = ready ? "FURIA gotowa" : $"Furia {Mathf.FloorToInt(battle.RageFraction * 100f)}%";
            rageLabel.color = ready ? RageColor : Color.white;
        }

        string HintText()
        {
            if (!battle.AcceptingInput) return "";
            var selected = battle.Selected;
            if (selected == null) return "Dotknij bohatera";
            var def = selected.Definition;
            if (battle.SpecialArmed)
                return $"{def.special.displayName}: dotknij celu";
            return $"Wróg: {def.attack.displayName}  ·  Sojusznik: {def.support.displayName}";
        }

        static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);
    }
}

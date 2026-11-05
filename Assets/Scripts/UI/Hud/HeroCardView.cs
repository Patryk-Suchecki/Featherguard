using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class HeroCardView
    {
        public const float Width = 370f;
        const float Spacing = 16f;
        const float RestY = 20f;
        const float SelectedY = 36f;

        static readonly Color Idle = new(0.07f, 0.08f, 0.13f, 0.92f);
        static readonly Color Selected = new(0.45f, 0.3f, 0.06f, 0.96f);
        static readonly Color HealthFill = new(0.38f, 0.86f, 0.36f);

        readonly UnitView unit;
        readonly RectTransform rect;
        readonly Image background;
        readonly Image frame;
        readonly CanvasGroup group;
        readonly Text title;
        readonly Text health;
        readonly RectTransform fill;
        readonly RectTransform statuses;

        public HeroCardView(RectTransform parent, UnitView hero, int index, System.Action<BattleUnit> onTap)
        {
            unit = hero;
            rect = Ui.Rect(hero.Definition.displayName + "Card", parent, Vector2.zero, Vector2.zero, new Vector2(24f + index * (Width + Spacing), RestY), new Vector2(Width, 156f));
            background = (Image)Ui.Button(rect, Idle, () => onTap(hero.Unit)).targetGraphic;
            group = rect.gameObject.AddComponent<CanvasGroup>();
            frame = Ui.Sliced(Ui.Stretch("Frame", rect, -4f), "Frame", Ui.GoldColor);

            Portrait.Create(rect, hero.Definition.portrait, new Vector2(12f, 0f), 128f, new Vector2(0f, 0.5f));
            title = Ui.Label(Ui.Rect("Name", rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(152f, -12f), new Vector2(210f, 40f)),
                hero.Definition.displayName, 34, TextAnchor.UpperLeft, Color.white);
            Ui.Bar(rect, Vector2.zero, new Vector2(200f, 30f), HealthFill, out fill);
            var bar = (RectTransform)fill.parent;
            bar.anchorMin = bar.anchorMax = new Vector2(0f, 0.5f);
            bar.pivot = new Vector2(0f, 0.5f);
            bar.anchoredPosition = new Vector2(152f, 2f);
            health = Ui.Label(Ui.Stretch("Health", bar), "", 22, TextAnchor.MiddleCenter, Color.white);
            statuses = Ui.Rect("Statuses", rect, Vector2.zero, Vector2.zero, new Vector2(152f, 8f), new Vector2(210f, 48f));

            hero.Changed += Refresh;
            Refresh();
        }

        public void Unbind() => unit.Changed -= Refresh;

        void Refresh()
        {
            if (rect == null) return;
            Ui.SetFill(fill, unit.HealthFraction);
            health.text = $"{unit.Health}/{unit.MaxHealth}";
            StatusIconRow.Build(statuses, unit.Statuses, 46f, false);
        }

        public void Animate(IPlayerCommands commands)
        {
            bool isSelected = unit.Unit == commands.Selected;
            bool ready = unit.IsAlive && !unit.Unit.HasActed;
            background.color = isSelected ? Selected : Idle;
            frame.enabled = isSelected;
            group.alpha = !unit.IsAlive ? 0.35f : ready || !commands.AcceptingInput ? 1f : 0.5f;
            var position = rect.anchoredPosition;
            position.y = Mathf.Lerp(position.y, isSelected ? SelectedY : RestY, Time.unscaledDeltaTime * 14f);
            rect.anchoredPosition = position;
            title.text = unit.IsAlive ? unit.Definition.displayName : "Fallen";
        }
    }
}

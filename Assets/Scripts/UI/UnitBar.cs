using Featherguard.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public class UnitBar : MonoBehaviour
    {
        static readonly Color HeroFill = new(0.38f, 0.86f, 0.36f);
        static readonly Color EnemyFill = new(0.92f, 0.32f, 0.26f);

        BattleUnit unit;
        Camera view;
        RectTransform rect;
        RectTransform fill;
        Text health;
        RectTransform statusRow;
        CanvasGroup group;

        public static UnitBar Create(BattleUnit unit, RectTransform parent, Camera view)
        {
            var rect = Ui.Rect(unit.Definition.displayName, parent, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(170f, 64f));
            var bar = rect.gameObject.AddComponent<UnitBar>();
            bar.unit = unit;
            bar.view = view;
            bar.rect = rect;
            bar.group = rect.gameObject.AddComponent<CanvasGroup>();
            bar.group.blocksRaycasts = false;

            var name = Ui.Rect("Name", rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(170f, 24f));
            Ui.Label(name, unit.Definition.displayName, 20, TextAnchor.LowerCenter, Color.white);

            var background = Ui.Rect("Background", rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(150f, 20f));
            Ui.Image(background, new Color(0.05f, 0.05f, 0.08f, 0.85f));
            bar.fill = Ui.Stretch("Fill", background, 3f);
            bar.fill.pivot = new Vector2(0f, 0.5f);
            Ui.Image(bar.fill, unit.Team == Team.Heroes ? HeroFill : EnemyFill);
            bar.health = Ui.Label(Ui.Stretch("Health", background), "", 15, TextAnchor.MiddleCenter, Color.white);

            bar.statusRow = Ui.Rect("Status", rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(170f, 22f));

            unit.Changed += bar.Refresh;
            bar.Refresh();
            return bar;
        }

        void OnDestroy()
        {
            if (unit != null)
                unit.Changed -= Refresh;
        }

        void LateUpdate()
        {
            if (unit == null)
            {
                Destroy(gameObject);
                return;
            }
            rect.position = view.WorldToScreenPoint(unit.Top);
            group.alpha = !unit.IsAlive ? 0f : unit.Team == Team.Heroes && unit.HasActed ? 0.55f : 1f;
        }

        void Refresh()
        {
            float fraction = (float)unit.Health / unit.MaxHealth;
            fill.anchorMax = new Vector2(Mathf.Max(0.001f, fraction), 1f);
            fill.offsetMax = new Vector2(fraction > 0f ? -3f : 0f, -3f);
            health.text = $"{unit.Health}/{unit.MaxHealth}";
            RefreshStatuses();
        }

        void RefreshStatuses()
        {
            const float width = 52f;
            const float spacing = 4f;
            for (int i = statusRow.childCount - 1; i >= 0; i--)
                Destroy(statusRow.GetChild(i).gameObject);
            statusRow.DetachChildren();

            var statuses = unit.Statuses;
            float start = -(statuses.Count - 1) * (width + spacing) * 0.5f;
            for (int i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                var chip = Ui.Rect(status.Type.ToString(), statusRow, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(start + i * (width + spacing), 0f), new Vector2(width, 22f));
                Ui.Image(chip, StatusEffect.Tint(status.Type) * new Color(0.75f, 0.75f, 0.75f, 0.95f));
                Ui.Label(Ui.Stretch("Label", chip), $"{StatusEffect.Label(status.Type)}{status.Turns}", 15, TextAnchor.MiddleCenter, Color.white);
            }
        }
    }
}

using Featherguard.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public class UnitBar : MonoBehaviour
    {
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
            var rect = Ui.Rect(unit.Definition.displayName, parent, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(220f, 110f));
            var bar = rect.gameObject.AddComponent<UnitBar>();
            bar.unit = unit;
            bar.view = view;
            bar.rect = rect;
            bar.group = rect.gameObject.AddComponent<CanvasGroup>();
            bar.group.blocksRaycasts = false;

            var name = Ui.Rect("Name", rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(220f, 30f));
            Ui.Label(name, unit.Definition.displayName, 26, TextAnchor.LowerCenter, Color.white);
            Ui.Bar(rect, Vector2.zero, new Vector2(200f, 30f), EnemyFill, out bar.fill);
            var background = (RectTransform)bar.fill.parent;
            background.anchorMin = background.anchorMax = new Vector2(0.5f, 0f);
            background.pivot = new Vector2(0.5f, 0f);
            background.anchoredPosition = Vector2.zero;
            bar.health = Ui.Label(Ui.Stretch("Health", background), "", 22, TextAnchor.MiddleCenter, Color.white);
            bar.statusRow = Ui.Rect("Status", rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 68f), new Vector2(220f, 52f));

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
            group.alpha = unit.IsAlive ? 1f : 0f;
        }

        void Refresh()
        {
            Ui.SetFill(fill, (float)unit.Health / unit.MaxHealth);
            health.text = $"{unit.Health}/{unit.MaxHealth}";
            BattleHud.StatusIcons(statusRow, unit.Statuses, 50f, true);
        }
    }
}

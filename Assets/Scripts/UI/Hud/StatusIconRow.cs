using System.Collections.Generic;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using UnityEngine;

namespace Featherguard.UI
{
    public static class StatusIconRow
    {
        static readonly Color Backdrop = new(0.04f, 0.04f, 0.07f, 0.85f);
        static readonly Color Badge = new(0.9f, 0.2f, 0.15f, 1f);

        public static void Build(RectTransform row, IReadOnlyList<StatusSnapshot> statuses, VisualsLibrary<StatusDefinition, StatusVisuals> visuals, float size, bool centered)
        {
            for (int i = row.childCount - 1; i >= 0; i--)
                Object.Destroy(row.GetChild(i).gameObject);
            row.DetachChildren();

            float step = size + 6f;
            float start = centered ? -(statuses.Count - 1) * step * 0.5f : size * 0.5f;
            var anchor = centered ? new Vector2(0.5f, 0.5f) : new Vector2(0f, 0.5f);
            for (int i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                var holder = Ui.Rect(status.Definition.name, row, anchor, new Vector2(0.5f, 0.5f), new Vector2(start + i * step, 0f), Vector2.one * size);
                Ui.Icon(holder, "Circle", Vector2.zero, size, Backdrop);
                var icon = Ui.Icon(holder, "Circle", Vector2.zero, size * 0.82f);
                icon.sprite = visuals.Of(status.Definition).icon;
                var badge = Ui.Rect("Turns", holder, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-4f, 4f), Vector2.one * size * 0.5f);
                Ui.Image(badge, Badge).sprite = Ui.Sprite("Circle");
                Ui.Label(Ui.Stretch("Label", badge), status.Turns.ToString(), Mathf.RoundToInt(size * 0.38f), TextAnchor.MiddleCenter, Color.white);
            }
        }
    }
}

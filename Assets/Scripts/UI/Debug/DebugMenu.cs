using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class DebugAction
    {
        public readonly string Label;
        public readonly Action Run;
        public readonly bool Confirm;
        public readonly bool KeepOpen;
        public readonly Sprite Icon;
        public readonly IReadOnlyList<DebugAction> Children;
        readonly Func<bool> available;

        public DebugAction(string label, Action run, bool confirm = false, Func<bool> available = null, bool keepOpen = false, Sprite icon = null)
        {
            Label = label;
            Run = run;
            Confirm = confirm;
            KeepOpen = keepOpen;
            Icon = icon;
            this.available = available;
        }

        public DebugAction(string label, IReadOnlyList<DebugAction> children)
        {
            Label = label;
            Children = children;
        }

        public bool IsAvailable => available == null || available();
    }

    public class DebugMenu : MonoBehaviour
    {
        const int SortingOrder = 1000;
        const float IconSize = 30f;
        const float RowHeight = 84f;
        const float GridRowHeight = 64f;
        const float RowGap = 12f;
        const int GridFrom = 9;

        static readonly Color IconColor = new(1f, 1f, 1f, 0.12f);
        static readonly Color IconTextColor = new(1f, 1f, 1f, 0.35f);
        static readonly Color PanelColor = new(0.04f, 0.04f, 0.06f, 0.95f);
        static readonly Color ButtonColor = new(0.16f, 0.17f, 0.22f, 1f);
        static readonly Color ConfirmColor = new(0.62f, 0.18f, 0.14f, 1f);
        static readonly Color TextColor = new(0.9f, 0.86f, 0.78f);
        static readonly Color UnavailableColor = new(0.5f, 0.5f, 0.55f);
        static readonly Color StatusColor = new(0.6f, 0.86f, 0.5f);

        readonly List<(DebugAction Action, Button Button, Text Label)> rows = new();
        readonly DebugAction back = new("< Back", (Action)null);
        IReadOnlyList<DebugAction> rootActions;
        RectTransform safe;
        GameObject panel;
        Text status;
        DebugAction pending;

        public static DebugMenu Create(IReadOnlyList<DebugAction> actions)
        {
            var root = ScreenCanvas.Create("Debug Menu");
            root.GetComponent<Canvas>().sortingOrder = SortingOrder;
            if (Application.isPlaying) DontDestroyOnLoad(root.gameObject);
            var menu = root.gameObject.AddComponent<DebugMenu>();
            menu.Build(root, actions);
            return menu;
        }

        void Build(RectTransform root, IReadOnlyList<DebugAction> actions)
        {
            rootActions = actions;
            safe = SafeArea.Create(root);
            var icon = Ui.Rect("Debug", safe, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(2f, -2f), Vector2.one * IconSize);
            Ui.Sliced(icon, "RoundRect", IconColor, raycast: true);
            icon.gameObject.AddComponent<Button>().onClick.AddListener(Toggle);
            Ui.Label(Ui.Stretch("Label", icon), "dbg", 12, TextAnchor.MiddleCenter, IconTextColor);
        }

        void Toggle()
        {
            if (panel != null) Close();
            else Open(null);
        }

        void Close()
        {
            if (panel != null) Destroy(panel);
            panel = null;
            pending = null;
            rows.Clear();
        }

        void Open(DebugAction group)
        {
            Close();
            var actions = group?.Children ?? rootActions;
            bool grid = actions.Count >= GridFrom;
            int columns = grid ? 2 : 1;
            float rowHeight = grid ? GridRowHeight : RowHeight;
            float width = grid ? 780f : 560f;
            int lines = (actions.Count + columns - 1) / columns + (group != null ? 1 : 0);

            var rect = Ui.Rect("Panel", safe, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(IconSize + 10f, -2f),
                new Vector2(width, 112f + lines * (rowHeight + RowGap)));
            Ui.Panel(rect, PanelColor, raycast: true);
            Ui.Label(Ui.Rect("Title", rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(width - 40f, 50f)),
                group?.Label ?? "Debug", 36, TextAnchor.MiddleCenter, TextColor);
            status = Ui.Label(Ui.Rect("Status", rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(width - 40f, 30f)),
                "", 22, TextAnchor.MiddleCenter, StatusColor);
            panel = rect.gameObject;

            float y = -100f;
            float inner = width - 40f;
            if (group != null)
            {
                Row(rect, back, 0f, y, inner, rowHeight);
                y -= rowHeight + RowGap;
            }
            float cell = (inner - (columns - 1) * RowGap) / columns;
            for (int i = 0; i < actions.Count; i++)
            {
                float x = -inner / 2f + cell / 2f + i % columns * (cell + RowGap);
                Row(rect, actions[i], x, y - i / columns * (rowHeight + RowGap), cell, rowHeight);
            }
            Refresh();
        }

        void Row(RectTransform parent, DebugAction action, float x, float y, float width, float height)
        {
            var rect = Ui.Rect(action.Label, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(x, y), new Vector2(width, height));
            var button = Ui.Button(rect, ButtonColor, () => Press(action));
            var label = Ui.Label(Ui.Stretch("Label", rect), action.Children != null ? $"{action.Label} >" : action.Label,
                height < RowHeight ? 24 : 30, action.Icon != null ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter, TextColor);
            if (action.Icon != null)
            {
                var icon = Ui.Image(Ui.Rect("Icon", rect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(6f, 0f), Vector2.one * (height - 8f)), Color.white);
                icon.sprite = action.Icon;
                icon.preserveAspect = true;
                label.rectTransform.offsetMin = new Vector2(height + 6f, 0f);
            }
            rows.Add((action, button, label));
        }

        void Refresh()
        {
            foreach (var row in rows)
            {
                bool available = row.Action.IsAvailable;
                row.Button.interactable = available;
                row.Label.color = available ? TextColor : UnavailableColor;
            }
        }

        void Press(DebugAction action)
        {
            if (action == back)
            {
                Open(null);
                return;
            }
            if (!action.IsAvailable) return;
            if (action.Children != null)
            {
                Open(action);
                return;
            }
            if (action.Confirm && pending != action)
            {
                Disarm();
                pending = action;
                Show(action, $"Tap again: {action.Label}", ConfirmColor);
                return;
            }
            Disarm();
            if (action.KeepOpen) status.text = $"Done: {action.Label}";
            else Close();
            action.Run();
        }

        void Disarm()
        {
            if (pending != null) Show(pending, pending.Label, ButtonColor);
            pending = null;
        }

        void Show(DebugAction action, string text, Color color)
        {
            foreach (var row in rows)
            {
                if (row.Action != action) continue;
                row.Label.text = text;
                row.Button.image.color = color;
            }
        }
    }
}

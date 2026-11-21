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

        public DebugAction(string label, Action run, bool confirm = false)
        {
            Label = label;
            Run = run;
            Confirm = confirm;
        }
    }

    public class DebugMenu : MonoBehaviour
    {
        const int SortingOrder = 1000;
        const float IconSize = 30f;
        const float RowHeight = 84f;
        const float RowGap = 12f;

        static readonly Color IconColor = new(1f, 1f, 1f, 0.12f);
        static readonly Color IconTextColor = new(1f, 1f, 1f, 0.35f);
        static readonly Color PanelColor = new(0.04f, 0.04f, 0.06f, 0.95f);
        static readonly Color ButtonColor = new(0.16f, 0.17f, 0.22f, 1f);
        static readonly Color ConfirmColor = new(0.62f, 0.18f, 0.14f, 1f);
        static readonly Color TextColor = new(0.9f, 0.86f, 0.78f);

        readonly List<(DebugAction Action, Image Background, Text Label)> rows = new();
        GameObject panel;
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
            var safe = SafeArea.Create(root);
            var icon = Ui.Rect("Debug", safe, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(2f, -2f), Vector2.one * IconSize);
            Ui.Sliced(icon, "RoundRect", IconColor, raycast: true);
            icon.gameObject.AddComponent<Button>().onClick.AddListener(Toggle);
            Ui.Label(Ui.Stretch("Label", icon), "dbg", 12, TextAnchor.MiddleCenter, IconTextColor);

            var rect = Ui.Rect("Panel", safe, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(IconSize + 10f, -2f),
                new Vector2(560f, 84f + actions.Count * (RowHeight + RowGap)));
            Ui.Panel(rect, PanelColor, raycast: true);
            Ui.Label(Ui.Rect("Title", rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(520f, 50f)),
                "Debug", 36, TextAnchor.MiddleCenter, TextColor);
            for (int i = 0; i < actions.Count; i++)
                Row(rect, actions[i], -72f - i * (RowHeight + RowGap));
            panel = rect.gameObject;
            panel.SetActive(false);
        }

        void Row(RectTransform parent, DebugAction action, float y)
        {
            var rect = Ui.Rect(action.Label, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(520f, RowHeight));
            var button = Ui.Button(rect, ButtonColor, () => Press(action));
            var label = Ui.Label(Ui.Stretch("Label", rect), action.Label, 30, TextAnchor.MiddleCenter, TextColor);
            rows.Add((action, button.GetComponent<Image>(), label));
        }

        void Toggle()
        {
            Disarm();
            panel.SetActive(!panel.activeSelf);
        }

        void Press(DebugAction action)
        {
            if (action.Confirm && pending != action)
            {
                Disarm();
                pending = action;
                Show(action, $"Tap again: {action.Label}", ConfirmColor);
                return;
            }
            Disarm();
            panel.SetActive(false);
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
                row.Background.color = color;
            }
        }
    }
}

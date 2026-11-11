using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class RageButtonView
    {
        static readonly Color RageColor = new(1f, 0.45f, 0.15f);
        static readonly Color Dormant = new(0.25f, 0.08f, 0.05f, 1f);
        static readonly Color Active = new(0.85f, 0.25f, 0.1f, 1f);

        readonly RectTransform root;
        readonly Image fill;
        readonly Image core;
        readonly Image ring;
        readonly Button button;
        readonly Text label;

        public RageButtonView(RectTransform safe, UnityEngine.Events.UnityAction onTap)
        {
            root = Ui.Rect("Rage", safe, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-150f, 150f), new Vector2(230f, 230f));
            Ui.Icon(root, "Circle", Vector2.zero, 230f, new Color(0.04f, 0.04f, 0.07f, 0.92f));
            fill = Ui.Icon(root, "Circle", Vector2.zero, 222f, RageColor);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = (int)Image.Origin360.Bottom;
            var center = Ui.Centered("Core", root, Vector2.zero, new Vector2(176f, 176f));
            button = Ui.Button(center, Dormant, onTap, "Circle");
            core = (Image)button.targetGraphic;
            Ui.Icon(center, "Rage", new Vector2(0f, 8f), 116f);
            ring = Ui.Icon(root, "Ring", Vector2.zero, 250f, new Color(1f, 1f, 1f, 0f));
            label = Ui.Label(Ui.Rect("Label", root, new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 8f), new Vector2(600f, 50f)),
                "", 32, TextAnchor.LowerRight, Color.white);
        }

        public void Update(RageMeter rage, IPlayerCommands commands)
        {
            fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, rage.Fraction, Time.unscaledDeltaTime * 1.5f);
            bool usable = commands.SpecialAvailable;
            button.interactable = usable;
            core.color = usable ? Active : Dormant;
            root.localScale = Vector3.one * (rage.Ready ? 1f + Mathf.Sin(Time.unscaledTime * 7f) * 0.05f : 1f);
            ring.color = new Color(1f, 0.95f, 0.7f, commands.SpecialArmed ? 0.6f + Mathf.Sin(Time.unscaledTime * 10f) * 0.4f : 0f);
            label.text = commands.SpecialArmed ? "Pick a target!"
                : usable ? commands.Selected.Definition.special.displayName
                : rage.Ready ? "RAGE ready"
                : $"Rage {Mathf.FloorToInt(rage.Fraction * 100f)}%";
            label.color = rage.Ready ? RageColor : Color.white;
        }
    }
}

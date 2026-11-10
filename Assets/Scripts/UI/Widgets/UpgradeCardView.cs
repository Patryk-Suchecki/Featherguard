using Featherguard.Feedback;
using Featherguard.Run;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public static class UpgradeCardView
    {
        public static readonly Vector2 Size = new(390f, 520f);
        static readonly Color CurseColor = new(0.6f, 0.12f, 0.18f);
        const string CurseLabel = "CURSE";

        public static RectTransform Create(RectTransform parent, UpgradeOffer offer, RarityTable rarity, Vector2 position, string footer, System.Action onTap)
        {
            var upgrade = offer.Upgrade;
            var tier = rarity.Tier(upgrade.rarity);
            var tint = upgrade.curse ? CurseColor : tier.color;
            var rect = Ui.Centered(upgrade.displayName, parent, position, Size);
            var button = Ui.Button(rect, Ui.PanelColor, () =>
            {
                FeedbackHub.Ui(SfxId.Click);
                onTap?.Invoke();
            });
            button.interactable = onTap != null;
            var colors = button.colors;
            colors.disabledColor = Color.white;
            button.colors = colors;
            Ui.Sliced(Ui.Stretch("Frame", rect, -2f), "Frame", tint);

            Ui.Icon(rect, "Circle", new Vector2(0f, 140f), 190f, new Color(tint.r, tint.g, tint.b, 0.25f));
            var icon = Ui.Icon(rect, "Card", new Vector2(0f, 140f), 150f);
            if (upgrade.icon != null) icon.sprite = upgrade.icon;
            Ui.Wrapped(Ui.Centered("Name", rect, new Vector2(0f, 20f), new Vector2(360f, 90f)), upgrade.displayName, 38, TextAnchor.MiddleCenter, Color.white);
            Ui.Label(Ui.Centered("Target", rect, new Vector2(0f, -40f), new Vector2(360f, 36f)), offer.Target, 28, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.45f));
            Ui.Wrapped(Ui.Centered("Description", rect, new Vector2(0f, -120f), new Vector2(340f, 110f)), upgrade.description, 30, TextAnchor.MiddleCenter, new Color(0.88f, 0.92f, 1f));
            var tag = Ui.Centered("Rarity", rect, new Vector2(0f, -222f), new Vector2(240f, 48f));
            Ui.Sliced(tag, "Pill", tint);
            string label = !string.IsNullOrEmpty(footer) ? footer : upgrade.curse ? CurseLabel : tier.label;
            Ui.Label(Ui.Stretch("Label", tag), label, 26, TextAnchor.MiddleCenter, Color.white);
            return rect;
        }
    }
}

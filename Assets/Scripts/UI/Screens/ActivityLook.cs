using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.UI
{
    public static class ActivityLook
    {
        public static readonly Color StoryColor = new(0.78f, 0.5f, 0.24f);

        public static Color RewardColor(RewardKind reward) => reward switch
        {
            RewardKind.Exp => new Color(0.3f, 0.58f, 1f),
            RewardKind.Gold => new Color(1f, 0.84f, 0.2f),
            RewardKind.Materials => new Color(0.38f, 0.82f, 0.32f),
            RewardKind.Loot => new Color(0.92f, 0.27f, 0.22f),
            RewardKind.Schematic => new Color(0.3f, 0.86f, 0.9f),
            RewardKind.Unlock => new Color(0.68f, 0.38f, 0.96f),
            _ => new Color(0.6f, 0.6f, 0.64f),
        };

        public static void TypeIcon(RectTransform parent, ActivityType? type, Color color, float size)
        {
            if (type == ActivityType.WavesFight)
            {
                Ui.Icon(parent, "Circle", new Vector2(-size * 0.18f, size * 0.12f), size * 0.6f, color);
                Ui.Icon(parent, "Circle", new Vector2(size * 0.18f, -size * 0.12f), size * 0.6f, color);
                return;
            }
            Ui.Icon(parent, type switch
            {
                ActivityType.SingleFight => "Circle",
                ActivityType.PuzzleFight => "Clover",
                ActivityType.EliteFight => "Elite",
                ActivityType.BossFight => "Crown",
                _ => "Flag",
            }, Vector2.zero, size, color);
        }
    }
}

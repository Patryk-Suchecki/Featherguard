using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.UI
{
    public static class ActivityText
    {
        public static string Describe(ActivityType type, int waves) => type switch
        {
            ActivityType.SingleFight => "Single fight",
            ActivityType.WavesFight => $"Waves fight   ·   {waves} waves",
            ActivityType.PuzzleFight => "Puzzle fight",
            ActivityType.EliteFight => "Elite fight",
            _ => "Boss fight",
        };

        public static string RewardName(RewardKind reward) => reward switch
        {
            RewardKind.Exp => "EXP",
            RewardKind.Gold => "Gold",
            RewardKind.Materials => "Materials",
            RewardKind.Loot => "Loot",
            RewardKind.Schematic => "Schematic",
            RewardKind.Unlock => "Unlock",
            _ => "",
        };

        public static Color RatingColor(ThreatRating rating) => rating switch
        {
            ThreatRating.Easy => new Color(0.55f, 0.85f, 0.5f),
            ThreatRating.Even => Color.white,
            ThreatRating.Dangerous => new Color(1f, 0.62f, 0.3f),
            _ => new Color(1f, 0.35f, 0.3f),
        };
    }
}

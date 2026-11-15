using UnityEngine;

namespace Featherguard.Meta
{
    public enum ThreatRating { Easy, Even, Dangerous, Madness }

    [CreateAssetMenu(menuName = "Featherguard/World")]
    public class WorldDefinition : ScriptableObject
    {
        [Tooltip("Lands in map order.")]
        public LandDefinition[] lands = new LandDefinition[0];
        [Tooltip("The story order: only the next one is available, and a land opens when the next mission is in it.")]
        public ActivityDefinition[] storyMissions = new ActivityDefinition[0];

        [Header("Activities on the map")]
        [Min(1)] public int minActivitiesPerLand = 1;
        [Min(1)] public int maxActivitiesPerLand = 3;

        [Header("Threat rating (threat level minus party strength)")]
        public int easyAtMost = -2;
        public int dangerousFrom = 2;
        public int madnessFrom = 4;
        [Tooltip("Extra share of gold, materials and EXP for a win at Dangerous or Madness.")]
        [Min(0f)] public float daringBonus = 0.5f;
        [Tooltip("The party strength ratings use until heroes have levels.")]
        [Min(0f)] public float partyStrength = 1f;

        public ThreatRating Rate(float threatAboveStrength) =>
            threatAboveStrength <= easyAtMost ? ThreatRating.Easy
            : threatAboveStrength >= madnessFrom ? ThreatRating.Madness
            : threatAboveStrength >= dangerousFrom ? ThreatRating.Dangerous
            : ThreatRating.Even;
    }
}

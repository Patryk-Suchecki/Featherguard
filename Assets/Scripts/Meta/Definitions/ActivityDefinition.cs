using Featherguard.Battle;
using UnityEngine;

namespace Featherguard.Meta
{
    public enum ActivityType { SingleFight, WavesFight, PuzzleFight, EliteFight, BossFight }

    [CreateAssetMenu(menuName = "Featherguard/Activity")]
    public class ActivityDefinition : ScriptableObject
    {
        public string displayName;
        public ActivityType type;
        [Tooltip("The land this activity takes place in.")]
        public LandDefinition land;
        public EncounterDefinition encounter;
        [Tooltip("How hard it is, compared with the party strength to give the threat rating.")]
        [Range(1, 10)] public int threat = 1;
        [Tooltip("Side activities only: how many story missions must be won before it can be drawn.")]
        [Min(0)] public int availableAfter;
    }
}

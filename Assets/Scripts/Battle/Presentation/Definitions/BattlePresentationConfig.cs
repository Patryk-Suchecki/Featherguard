using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    [CreateAssetMenu(menuName = "Featherguard/Battle Presentation")]
    public class BattlePresentationConfig : ScriptableObject
    {
        [Header("Visuals")]
        [Tooltip("How each ability looks and sounds; abilities without an entry use plain defaults.")]
        public AbilityVisuals[] abilities = new AbilityVisuals[0];
        [Tooltip("Model, portrait and animator of each unit.")]
        public UnitVisuals[] units = new UnitVisuals[0];
        [Tooltip("Icon and colour of each status.")]
        public StatusVisuals[] statuses = new StatusVisuals[0];

        [Header("Formation")]
        public Vector3[] heroSlots = { new(-2.3f, 0f, 0.45f), new(-3.7f, 0f, -0.55f), new(-5.0f, 0f, 0.6f) };
        public Vector3[] enemySlots = { new(2.3f, 0f, 0.45f), new(3.7f, 0f, -0.55f), new(5.0f, 0f, 0.6f) };
        public float spawnOffset = 6f;
        public float heroYaw = 115f;

        [Header("Hits")]
        public float lightKnockback = 0.22f;
        public float heavyKnockback = 0.4f;

        [Header("Timing")]
        public float enemyTurnDelay = 0.8f;
        public float betweenEnemyActions = 0.2f;
        public float afterAction = 0.35f;
        public float specialWindup = 0.5f;
        public float waveDelay = 0.6f;
        public float spawnInterval = 0.18f;
        public float summonInterval = 0.25f;
        public float afterSpawn = 0.8f;
        public float statusTickInterval = 0.35f;
        public float stunnedInterval = 0.25f;
        public float enrageHold = 0.9f;
        public float endDelay = 0.6f;
        [Tooltip("Seconds between a won battle and the rewards.")]
        [Min(0f)] public float afterVictory = 0.8f;

        [Header("Timing prompts")]
        [Tooltip("Temporary attack ring and BLOCK! cue. Off: every attack and block resolves as Miss and enemy turns don't wait.")]
        public bool timingPrompts = true;
        [Tooltip("Seconds from the ring appearing to the moment it meets the target circle.")]
        [Min(0.1f)] public float attackRingTime = 0.8f;
        [Tooltip("Shortest and longest hold of the enemy wind-up before BLOCK!, in seconds.")]
        [Min(0f)] public float windupMin = 0.4f;
        [Min(0f)] public float windupMax = 1.2f;
        [Tooltip("Seconds between the BLOCK! flash and the hit.")]
        [Min(0f)] public float blockLead = 0.25f;

        [Header("Selection")]
        [Tooltip("Ring under every unit by class; a hero shows it only while it can still act this turn.")]
        public Color warriorBase = new(1f, 0.82f, 0.15f, 1f);
        public Color defenderBase = new(0.3f, 0.55f, 1f, 1f);
        public Color mageBase = new(0.68f, 0.38f, 1f, 1f);
        public Color supportBase = new(0.35f, 0.9f, 0.4f, 1f);
        public Color enemyBase = new(1f, 0.25f, 0.2f, 1f);
        [Tooltip("Base for a hero without a class.")]
        public Color noClassBase = new(0.85f, 0.85f, 0.85f, 1f);
        [Tooltip("Ring under the target the dragged hero is over.")]
        public Color hoverRing = new(1f, 1f, 1f, 1f);
        [Tooltip("Tap tolerance around units as a fraction of screen height.")]
        [Range(0f, 0.3f)] public float tapTolerance = 0.12f;
        [Tooltip("How long a unit or potion is held still before its info shows, in seconds.")]
        [Range(0.1f, 2f)] public float holdToInspect = 0.45f;

        public Color BaseColor(UnitDefinition unit) => unit.team == Team.Enemies ? enemyBase : unit.heroClass switch
        {
            HeroClass.Warrior => warriorBase,
            HeroClass.Defender => defenderBase,
            HeroClass.Mage => mageBase,
            HeroClass.Support => supportBase,
            _ => noClassBase,
        };

        public Vector3 SlotPosition(Team team, int slot)
        {
            var slots = team == Team.Heroes ? heroSlots : enemySlots;
            return slots.Length == 0 ? Vector3.zero : slots[Mathf.Clamp(slot, 0, slots.Length - 1)];
        }
    }
}

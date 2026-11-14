using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using Featherguard.Feedback;
using Featherguard.Meta;
using Featherguard.Run;
using Featherguard.UI;
using UnityEngine;

namespace Featherguard.Game
{
    [CreateAssetMenu(menuName = "Featherguard/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Content")]
        [Tooltip("Heroes who have joined, in the order of their seats at the campfire.")]
        public UnitDefinition[] heroes;
        [Tooltip("Fixed seats at the campfire; seats without a hero stay empty until one joins.")]
        [Min(3)] public int campSeats = 6;
        public BattleRules rules;
        public RunDefinition run;
        public MetaConfig meta;

        [Header("Presentation")]
        public BattlePresentationConfig presentation;
        public Material flashMaterial;
        public Material particleMaterial;
        public Material ringMaterial;
        [Tooltip("Template for unit materials: toon bands, shadow colour and outline. Each unit keeps its own texture and colour.")]
        public Material toonMaterial;
        public AudioClip[] sounds;
        public UiSprites ui;
        public FeedbackTuning feedback = new();
    }
}

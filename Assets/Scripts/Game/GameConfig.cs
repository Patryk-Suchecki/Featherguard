using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using Featherguard.Feel;
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
        public UnitDefinition[] heroes;
        public BattleRules rules;
        public RunDefinition run;
        public MetaConfig meta;

        [Header("Presentation")]
        public BattlePresentationConfig presentation;
        public Material flashMaterial;
        public Material particleMaterial;
        public Material ringMaterial;
        public AudioClip[] sounds;
        public UiSprites ui;
        public FeedbackTuning feedback = new();

        [Header("Development")]
        [Tooltip("Editor only: every battle uses this encounter instead of the run's. Leave empty in commits.")]
        public EncounterDefinition testEncounter;
    }
}

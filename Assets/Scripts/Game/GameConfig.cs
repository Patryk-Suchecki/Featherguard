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
    }
}

using Featherguard.Core;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    [CreateAssetMenu(menuName = "Featherguard/Ability Visuals")]
    public class AbilityVisuals : ScriptableObject
    {
        [Tooltip("The ability these visuals belong to.")]
        public AbilityDefinition ability;
        [Tooltip("Animator state the actor plays.")]
        public string animationState = "Attack";
        [Tooltip("Stronger hitstop, shake and knockback.")]
        public bool heavy;
        public Color fxColor = Color.white;
        public SfxId castSound;
        public SfxId impactSound;
        [SerializeReference, SubclassSelector] public AbilityDelivery delivery = new InstantDelivery();
    }
}

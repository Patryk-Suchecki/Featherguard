using Featherguard.Core;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    [CreateAssetMenu(menuName = "Featherguard/Ability Visuals")]
    public class AbilityVisuals : ScriptableObject, IVisualsFor<AbilityDefinition>
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
        [Tooltip("Optional effect drawn on the target when a hit lands.")]
        [SerializeReference, SubclassSelector] public ImpactEffect impactEffect;

        public AbilityDefinition Definition => ability;
    }
}

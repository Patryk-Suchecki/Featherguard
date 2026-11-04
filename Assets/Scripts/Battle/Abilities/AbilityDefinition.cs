using System.Linq;
using Featherguard.Battle.Presentation;
using Featherguard.Core;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle
{
    public enum AbilityTarget { Enemy, AllEnemies, Ally, AllAllies }

    [CreateAssetMenu(menuName = "Featherguard/Ability")]
    public class AbilityDefinition : ScriptableObject
    {
        public string displayName;
        public AbilityTarget target;
        [Min(1)] public int hits = 1;
        [SerializeReference, SubclassSelector] public AbilityEffect[] effects = new AbilityEffect[0];

        [Header("Presentation")]
        public string animationState = "Attack";
        public bool heavy;
        public Color fxColor = Color.white;
        public SfxId castSound;
        public SfxId impactSound;
        [SerializeReference, SubclassSelector] public AbilityDelivery delivery = new InstantDelivery();

        public bool TargetsEnemies => target is AbilityTarget.Enemy or AbilityTarget.AllEnemies;
        public bool TargetsAll => target is AbilityTarget.AllEnemies or AbilityTarget.AllAllies;
        public bool IsDamaging => effects.Any(e => e is DamageEffect);
        public bool HasDirectImpact => effects.Any(e => e is DamageEffect or HealEffect);

        public T Effect<T>() where T : AbilityEffect => effects.OfType<T>().FirstOrDefault();
    }
}

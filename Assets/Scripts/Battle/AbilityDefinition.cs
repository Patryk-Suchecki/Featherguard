using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle
{
    public enum AbilityEffect { Damage, Heal, Status }
    public enum AbilityTarget { Enemy, AllEnemies, Ally, AllAllies }
    public enum AbilityDelivery { Melee, Arrow, Magic, Instant }

    [CreateAssetMenu(menuName = "Featherguard/Ability")]
    public class AbilityDefinition : ScriptableObject
    {
        public string displayName;
        public AbilityEffect effect;
        public AbilityTarget target;
        public AbilityDelivery delivery;
        public string animationState = "Attack";
        public int power = 20;
        public int hits = 1;
        public bool heavy;
        public StatusApplication[] statuses = new StatusApplication[0];
        public Color fxColor = Color.white;
        public SfxId castSound;
        public SfxId impactSound;

        public bool TargetsEnemies => target is AbilityTarget.Enemy or AbilityTarget.AllEnemies;
        public bool TargetsAll => target is AbilityTarget.AllEnemies or AbilityTarget.AllAllies;
    }
}

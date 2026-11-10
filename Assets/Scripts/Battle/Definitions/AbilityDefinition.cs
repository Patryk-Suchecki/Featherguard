using System.Collections.Generic;
using System.Linq;
using Featherguard.Core;
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

        public bool TargetsEnemies => target is AbilityTarget.Enemy or AbilityTarget.AllEnemies;
        public bool TargetsAll => target is AbilityTarget.AllEnemies or AbilityTarget.AllAllies;
        public bool IsDamaging => effects.Any(e => e.Damages);
        public bool Heals => effects.Any(e => e.Heals);
        public bool HasDirectImpact => effects.Any(e => e.Damages || e.Heals);
        public int ExpectedDamage => effects.Sum(e => e.ExpectedDamage);
        public IEnumerable<StatusDefinition> StatusesOnTarget => effects.Select(e => e.StatusOnTarget).Where(s => s != null);
    }
}

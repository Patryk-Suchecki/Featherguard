using System.Linq;
using Featherguard.Core;
using UnityEngine;

namespace Featherguard.Battle
{
    public enum PotionTarget { Hero, FallenHero, AllHeroes, Enemy, AllEnemies }

    [CreateAssetMenu(menuName = "Featherguard/Potion")]
    public class PotionDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public PotionTarget target;
        [SerializeReference, SubclassSelector] public PotionEffect[] effects = new PotionEffect[0];

        public bool TargetsEnemies => target is PotionTarget.Enemy or PotionTarget.AllEnemies;
        public bool TargetsAll => target is PotionTarget.AllHeroes or PotionTarget.AllEnemies;
        public bool TargetsFallen => target == PotionTarget.FallenHero || effects.Any(e => e != null && e.Revives);
    }
}

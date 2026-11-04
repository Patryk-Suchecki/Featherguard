using UnityEngine;

namespace Featherguard.Meta
{
    [CreateAssetMenu(menuName = "Featherguard/Meta Config")]
    public class MetaConfig : ScriptableObject
    {
        public PerkDefinition[] perks = new PerkDefinition[0];
        public int feathersPerBattle = 1;
        [Min(1)] public int goldPerFeather = 50;
        public int completionBonus = 10;
        public string saveFileName = "featherguard_save.json";
    }
}

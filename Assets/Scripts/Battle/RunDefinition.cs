using UnityEngine;

namespace Featherguard.Battle
{
    [CreateAssetMenu(menuName = "Featherguard/Run")]
    public class RunDefinition : ScriptableObject
    {
        public EncounterDefinition[] encounters;
        [Range(0f, 1f)] public float restHeal = 0.2f;
        [Range(0f, 1f)] public float reviveHealth = 0.3f;
        public int goldPerKill = 3;
    }
}

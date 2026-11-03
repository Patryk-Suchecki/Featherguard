using System;
using UnityEngine;

namespace Featherguard.Battle
{
    [CreateAssetMenu(menuName = "Featherguard/Encounter")]
    public class EncounterDefinition : ScriptableObject
    {
        public string displayName;
        public Wave[] waves;

        [Serializable]
        public class Wave
        {
            public UnitDefinition[] enemies;
        }
    }
}

using System;
using UnityEngine;

namespace Featherguard.Battle
{
    [CreateAssetMenu(menuName = "Featherguard/Battle Config")]
    public class BattleConfig : ScriptableObject
    {
        public UnitDefinition[] heroes;
        public Wave[] waves;
        public GameObject arrowPrefab;
        public Material flashMaterial;
        public Material particleMaterial;
        public Material ringMaterial;
        public AudioClip[] sounds;

        [Serializable]
        public class Wave
        {
            public UnitDefinition[] enemies;
        }
    }
}

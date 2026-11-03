using UnityEngine;

namespace Featherguard.Battle
{
    [CreateAssetMenu(menuName = "Featherguard/Battle Config")]
    public class BattleConfig : ScriptableObject
    {
        public UnitDefinition[] heroes;
        public EncounterDefinition encounter;
        public GameObject arrowPrefab;
        public Material flashMaterial;
        public Material particleMaterial;
        public Material ringMaterial;
        public AudioClip[] sounds;
    }
}

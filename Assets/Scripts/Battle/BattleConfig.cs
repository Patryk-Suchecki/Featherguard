using UnityEngine;

namespace Featherguard.Battle
{
    [CreateAssetMenu(menuName = "Featherguard/Battle Config")]
    public class BattleConfig : ScriptableObject
    {
        public UnitDefinition[] heroes;
        public RunDefinition run;
        public GameObject arrowPrefab;
        public Material flashMaterial;
        public Material particleMaterial;
        public Material ringMaterial;
        public AudioClip[] sounds;
        public UI.UiSprites ui;
    }
}

using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    [CreateAssetMenu(menuName = "Featherguard/Unit Visuals")]
    public class UnitVisuals : ScriptableObject, IVisualsFor<UnitDefinition>
    {
        [Tooltip("The unit these visuals belong to.")]
        public UnitDefinition unit;
        public GameObject model;
        public Sprite portrait;
        public RuntimeAnimatorController animator;
        [Tooltip("Scale applied to the model.")]
        [Min(0.01f)] public float modelScale = 1f;
        [Tooltip("Model height in metres before scaling; places health bars, popups and the hit point.")]
        [Min(0.1f)] public float height = 1.4f;

        public UnitDefinition Definition => unit;
        public float ScaledHeight => height * modelScale;
        public float Radius => 0.45f * modelScale;
    }
}

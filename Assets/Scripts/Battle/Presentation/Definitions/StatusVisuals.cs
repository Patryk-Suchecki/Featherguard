using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    [CreateAssetMenu(menuName = "Featherguard/Status Visuals")]
    public class StatusVisuals : ScriptableObject, IVisualsFor<StatusDefinition>
    {
        [Tooltip("The status these visuals belong to.")]
        public StatusDefinition status;
        public Sprite icon;
        [Tooltip("Colour of the effect when the status ticks.")]
        public Color tint = Color.white;

        public StatusDefinition Definition => status;
    }
}

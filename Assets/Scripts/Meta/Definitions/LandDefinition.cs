using UnityEngine;

namespace Featherguard.Meta
{
    [CreateAssetMenu(menuName = "Featherguard/Land")]
    public class LandDefinition : ScriptableObject
    {
        public string displayName;
        [Tooltip("Activities that can be drawn as side activities. A Boss fight is only drawn after the land's boss story mission is won.")]
        public ActivityDefinition[] sideActivities = new ActivityDefinition[0];
    }
}

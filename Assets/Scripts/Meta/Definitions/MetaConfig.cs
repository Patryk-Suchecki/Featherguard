using UnityEngine;

namespace Featherguard.Meta
{
    [CreateAssetMenu(menuName = "Featherguard/Meta Config")]
    public class MetaConfig : ScriptableObject
    {
        public string saveFileName = "featherguard_save.json";
    }
}

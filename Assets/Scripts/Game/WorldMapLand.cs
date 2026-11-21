using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.Game
{
    public class WorldMapLand : MonoBehaviour
    {
        [SerializeField] LandDefinition land;
        [SerializeField] Transform labelAnchor;
        [SerializeField] Transform activitiesAnchor;
        [SerializeField] GameObject lockedCover;

        public LandDefinition Land => land;
        public Transform LabelAnchor => labelAnchor != null ? labelAnchor : transform;
        public Transform ActivitiesAnchor => activitiesAnchor != null ? activitiesAnchor : transform;

        public void SetOpen(bool open)
        {
            if (lockedCover != null) lockedCover.SetActive(!open);
        }
    }
}

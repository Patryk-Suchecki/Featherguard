using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.Game
{
    public class WorldMapLand : MonoBehaviour
    {
        [SerializeField] LandDefinition land;
        [SerializeField] Transform[] slots;
        [SerializeField] Transform labelAnchor;
        [SerializeField] GameObject lockedCover;

        public LandDefinition Land => land;
        public Transform LabelAnchor => labelAnchor != null ? labelAnchor : transform;
        public int SlotCount => slots.Length;

        public Transform Slot(int index) => slots[Mathf.Clamp(index, 0, slots.Length - 1)];

        public void SetOpen(bool open)
        {
            if (lockedCover != null) lockedCover.SetActive(!open);
        }
    }
}

using UnityEngine;

namespace Featherguard.Game
{
    public class CampSeat : MonoBehaviour
    {
        [SerializeField] int index;
        [SerializeField] Transform labelAnchor;

        public int Index => index;
        public Transform LabelAnchor => labelAnchor != null ? labelAnchor : transform;
        public GameObject Hero { get; set; }
    }
}

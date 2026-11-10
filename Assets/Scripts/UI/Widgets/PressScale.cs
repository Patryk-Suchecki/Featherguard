using UnityEngine;
using UnityEngine.EventSystems;

namespace Featherguard.UI
{
    public class PressScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        float target = 1f;
        float scale = 1f;

        public void OnPointerDown(PointerEventData eventData) => target = 0.92f;
        public void OnPointerUp(PointerEventData eventData) => target = 1f;
        public void OnPointerExit(PointerEventData eventData) => target = 1f;

        void Update()
        {
            if (Mathf.Approximately(scale, target)) return;
            scale = Mathf.MoveTowards(scale, target, Time.unscaledDeltaTime * 2.5f);
            transform.localScale = Vector3.one * scale;
        }
    }
}

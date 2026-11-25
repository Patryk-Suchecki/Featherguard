using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Featherguard.UI
{
    public sealed class PotionSlot : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        const float StillDistance = 0.035f;

        IPotionCommands commands;
        PotionDefinition potion;
        float holdTime;
        bool pressing;
        bool moved;
        float pressedTime;
        Vector2 pressedAt;

        public void Init(IPotionCommands potionCommands, PotionDefinition definition, float hold)
        {
            commands = potionCommands;
            potion = definition;
            holdTime = hold;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressing = true;
            moved = false;
            pressedTime = Time.unscaledTime;
            pressedAt = eventData.position;
            commands.GrabPotion(potion, transform.TransformPoint(((RectTransform)transform).rect.center));
        }

        public void OnDrag(PointerEventData eventData)
        {
            moved |= Vector2.Distance(eventData.position, pressedAt) > Screen.height * StillDistance;
            if (moved) commands.EndInspectPotion();
            commands.MovePotion(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressing = false;
            commands.EndInspectPotion();
            commands.DropPotion(eventData.position);
        }

        void Update()
        {
            if (pressing && !moved && Time.unscaledTime - pressedTime >= holdTime)
                commands.InspectPotion(potion);
        }
    }
}

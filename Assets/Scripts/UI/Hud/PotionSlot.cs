using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Featherguard.UI
{
    public sealed class PotionSlot : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        IPotionCommands commands;
        PotionDefinition potion;

        public void Init(IPotionCommands potionCommands, PotionDefinition definition)
        {
            commands = potionCommands;
            potion = definition;
        }

        public void OnPointerDown(PointerEventData eventData) => commands.GrabPotion(potion, transform.TransformPoint(((RectTransform)transform).rect.center));

        public void OnDrag(PointerEventData eventData) => commands.MovePotion(eventData.position);

        public void OnPointerUp(PointerEventData eventData) => commands.DropPotion(eventData.position);
    }
}

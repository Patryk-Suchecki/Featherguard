using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Featherguard.Game
{
    public sealed class PointerGestures : IScreenGestures
    {
        readonly float holdTime;
        readonly List<RaycastResult> uiHits = new();
        bool pressing;
        bool held;
        float pressStart;
        Vector2 pressPosition;

        public PointerGestures(float holdTime) => this.holdTime = holdTime;

        public ScreenGesture Read(bool holds)
        {
            var pointer = Pointer.current;
            if (pointer == null) return ScreenGesture.None;
            var position = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                pressing = !OverUi(position);
                held = false;
                pressStart = Time.unscaledTime;
                pressPosition = position;
            }
            if (!pressing) return ScreenGesture.None;

            if (holds && !held && Time.unscaledTime - pressStart >= holdTime)
            {
                held = true;
                return new ScreenGesture(GestureKind.Hold, pressPosition);
            }
            if (!pointer.press.wasReleasedThisFrame) return ScreenGesture.None;
            pressing = false;
            return held ? ScreenGesture.None : new ScreenGesture(GestureKind.Tap, position);
        }

        bool OverUi(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            uiHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, uiHits);
            return uiHits.Count > 0;
        }
    }
}

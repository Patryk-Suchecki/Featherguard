using Featherguard.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Featherguard.Battle.Presentation
{
    public sealed class PointerTimingInput : ITimingInput
    {
        public float FrameTime => Time.unscaledDeltaTime * GameClock.Scale;

        public bool Pressed()
        {
            if (GameClock.Paused) return false;
            bool tap = Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
            bool space = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            return tap || space;
        }

        public GestureSample Sample()
        {
            var pointer = Pointer.current;
            var space = Keyboard.current?.spaceKey;
            var position = pointer != null ? GestureSpace.FromScreen(pointer.position.ReadValue(), new Vector2(Screen.width, Screen.height)) : Vector2.zero;
            if (GameClock.Paused) return GestureSample.Idle(position);

            bool pointerHeld = pointer != null && pointer.press.isPressed;
            bool pointerPressed = pointer != null && pointer.press.wasPressedThisFrame;
            bool pointerReleased = pointer != null && pointer.press.wasReleasedThisFrame;
            bool keyHeld = space != null && space.isPressed;
            bool keyPressed = space != null && space.wasPressedThisFrame;
            bool keyReleased = space != null && space.wasReleasedThisFrame;
            bool fromKey = (keyPressed || keyHeld || keyReleased) && !(pointerPressed || pointerHeld || pointerReleased);
            return new GestureSample(pointerPressed || keyPressed, pointerHeld || keyHeld, pointerReleased || keyReleased, position, fromKey);
        }
    }
}

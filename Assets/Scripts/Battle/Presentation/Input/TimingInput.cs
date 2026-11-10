using Featherguard.Feel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Featherguard.Battle.Presentation
{
    public interface ITimingInput
    {
        float FrameTime { get; }
        bool Pressed();
    }

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
    }
}

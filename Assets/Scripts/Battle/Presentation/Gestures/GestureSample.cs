using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public static class GestureSpace
    {
        public const float Height = 1080f;

        public static Vector2 FromScreen(Vector2 screen, Vector2 screenSize) =>
            (screen - screenSize * 0.5f) * (Height / Mathf.Max(1f, screenSize.y));
    }

    public readonly struct GestureSample
    {
        public readonly bool Pressed;
        public readonly bool Held;
        public readonly bool Released;
        public readonly bool FromKey;
        public readonly Vector2 Position;

        public GestureSample(bool pressed, bool held, bool released, Vector2 position, bool fromKey = false)
        {
            Pressed = pressed;
            Held = held;
            Released = released;
            Position = position;
            FromKey = fromKey;
        }

        public static GestureSample Idle(Vector2 position) => new(false, false, false, position);
        public static GestureSample Press(Vector2 position) => new(true, true, false, position);
        public static GestureSample Hold(Vector2 position) => new(false, true, false, position);
        public static GestureSample Release(Vector2 position) => new(false, false, true, position);
        public static GestureSample Key() => new(true, true, false, Vector2.zero, true);
    }
}

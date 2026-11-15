using UnityEngine;

namespace Featherguard.Game
{
    public enum GestureKind
    {
        None,
        Tap,
        Hold,
    }

    public readonly struct ScreenGesture
    {
        public static readonly ScreenGesture None = default;

        public readonly GestureKind Kind;
        public readonly Vector2 Position;

        public ScreenGesture(GestureKind kind, Vector2 position)
        {
            Kind = kind;
            Position = position;
        }
    }

    public interface IScreenGestures
    {
        ScreenGesture Read(bool holds);
    }
}

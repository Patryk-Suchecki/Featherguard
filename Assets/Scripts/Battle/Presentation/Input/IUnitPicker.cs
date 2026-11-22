using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public readonly struct UnitPointer
    {
        public readonly bool Pressed;
        public readonly bool Held;
        public readonly bool Released;
        public readonly bool Moved;
        public readonly UnitView Unit;
        public readonly Vector2 Screen;
        public readonly float HeldFor;

        public UnitPointer(bool pressed, bool held, bool released, bool moved, UnitView unit, Vector2 screen, float heldFor = 0f)
        {
            Pressed = pressed;
            Held = held;
            Released = released;
            Moved = moved;
            Unit = unit;
            Screen = screen;
            HeldFor = heldFor;
        }
    }

    public interface IUnitPicker
    {
        UnitPointer Read(IEnumerable<UnitView> candidates);
        UnitView At(Vector2 screen, IEnumerable<UnitView> candidates);
    }
}

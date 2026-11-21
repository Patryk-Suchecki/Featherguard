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

        public UnitPointer(bool pressed, bool held, bool released, bool moved, UnitView unit, Vector2 screen)
        {
            Pressed = pressed;
            Held = held;
            Released = released;
            Moved = moved;
            Unit = unit;
            Screen = screen;
        }
    }

    public interface IUnitPicker
    {
        UnitPointer Read(IEnumerable<UnitView> candidates);
    }
}

using System;
using UnityEngine;

namespace Featherguard.Battle
{
    public enum StatusType { Shield, Empower, Poison, Stun, Weaken, Taunt }

    [Serializable]
    public class StatusApplication
    {
        public StatusType type;
        public int turns = 1;
        public int potency;
        [Range(0f, 1f)] public float chance = 1f;
        public bool onSelf;
    }

    public class StatusEffect
    {
        public StatusType Type;
        public int Turns;
        public int Potency;
        public bool Fresh;

        public static string Label(StatusType type) => type switch
        {
            StatusType.Shield => "TAR",
            StatusType.Empower => "MOC",
            StatusType.Poison => "TRU",
            StatusType.Stun => "OGŁ",
            StatusType.Weaken => "SŁA",
            _ => "PRW",
        };

        public static string Announcement(StatusType type) => type switch
        {
            StatusType.Shield => "Tarcza",
            StatusType.Empower => "Moc!",
            StatusType.Poison => "Trucizna",
            StatusType.Stun => "Ogłuszenie!",
            StatusType.Weaken => "Osłabienie",
            _ => "Prowokacja",
        };

        public static Color Tint(StatusType type) => type switch
        {
            StatusType.Shield => new Color(0.3f, 0.6f, 1f),
            StatusType.Empower => new Color(1f, 0.55f, 0.15f),
            StatusType.Poison => new Color(0.4f, 0.85f, 0.25f),
            StatusType.Stun => new Color(1f, 0.85f, 0.2f),
            StatusType.Weaken => new Color(0.65f, 0.4f, 0.9f),
            _ => new Color(0.95f, 0.3f, 0.3f),
        };

        public static bool IsDebuff(StatusType type) => type is StatusType.Poison or StatusType.Stun or StatusType.Weaken;
    }
}

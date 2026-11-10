using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public enum PopupKind { Damage, Crit, Heal, Buff, Gold }

    public interface IBattleView
    {
        void ShowBanner(string text);
        void Callout(string text);
        void Popup(Vector3 world, string text, PopupKind kind);
        void SetProgress(int encounter, int encounters, int wave, int waves);
        void UnitAdded(UnitView unit);
        void UnitRemoved(UnitView unit);
        void ShowAttackRing(Vector3 world, float closing);
        void ShowBlockCue(Vector3 world);
        void HideTimingCue();
    }
}

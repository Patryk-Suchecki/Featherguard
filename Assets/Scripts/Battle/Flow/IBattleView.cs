using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle
{
    public enum PopupKind { Damage, Crit, Heal, Buff, Gold }

    public interface IBattleView
    {
        void ShowBanner(string text);
        void Callout(string text);
        void Popup(Vector3 world, string text, PopupKind kind);
        void SetProgress(int encounter, int encounters, int wave, int waves);
        void UnitAdded(BattleUnit unit);
        void UnitRemoved(BattleUnit unit);
        void HeroesReady(IReadOnlyList<BattleUnit> heroes);
    }
}

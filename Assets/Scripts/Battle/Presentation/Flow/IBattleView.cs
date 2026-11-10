using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public enum PopupKind { Damage, Crit, Heal, Buff, Gold }

    public interface IPopupView
    {
        void Popup(Vector3 world, string text, PopupKind kind);
    }

    public interface IBannerView
    {
        void ShowBanner(string text);
        void Callout(string text);
        void SetProgress(int encounter, int encounters, int wave, int waves);
    }

    public interface IUnitListView
    {
        void UnitAdded(UnitView unit);
        void UnitRemoved(UnitView unit);
    }

    public interface IBattleView : IPopupView, IBannerView, IUnitListView
    {
    }
}

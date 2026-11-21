using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public interface ITimingCueView
    {
        void ShowGesture(GestureSketch sketch);
        void ShowBlockCue(Vector3 world);
        void HideTimingCue();
    }
}

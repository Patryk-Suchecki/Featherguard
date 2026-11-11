using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public interface ITimingCueView
    {
        void ShowAttackRing(Vector3 world, float closing);
        void ShowBlockCue(Vector3 world);
        void HideTimingCue();
    }
}

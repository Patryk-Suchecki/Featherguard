using Featherguard.Core;
using UnityEngine;

namespace Featherguard.Game
{
    public static class AppStartup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Apply()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            VolumeSetting.Load();
        }
    }
}

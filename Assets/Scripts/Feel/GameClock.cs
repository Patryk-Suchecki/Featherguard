using UnityEngine;

namespace Featherguard.Feel
{
    public static class GameClock
    {
        static bool paused;

        public static bool Paused
        {
            get => paused;
            set
            {
                paused = value;
                Time.timeScale = value ? 0f : 1f;
            }
        }

        public static float Scale => paused ? 0f : 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            paused = false;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            AudioListener.volume = PlayerPrefs.GetFloat("volume", 1f);
        }
    }
}

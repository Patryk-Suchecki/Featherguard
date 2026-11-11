using UnityEngine;

namespace Featherguard.Core
{
    public static class VolumeSetting
    {
        const string Key = "volume";

        public static bool Muted => AudioListener.volume < 0.5f;

        public static void Load() => AudioListener.volume = PlayerPrefs.GetFloat(Key, 1f);

        public static void ToggleMute()
        {
            AudioListener.volume = Muted ? 1f : 0f;
            PlayerPrefs.SetFloat(Key, AudioListener.volume);
        }
    }
}

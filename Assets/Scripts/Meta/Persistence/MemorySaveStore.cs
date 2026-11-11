using UnityEngine;

namespace Featherguard.Meta
{
    public sealed class MemorySaveStore : ISaveStore
    {
        string json;

        public SaveData Load() => string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
        public void Save(SaveData data) => json = JsonUtility.ToJson(data);
    }
}

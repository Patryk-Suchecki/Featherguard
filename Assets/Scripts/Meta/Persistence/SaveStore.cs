using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Featherguard.Meta
{
    [Serializable]
    public class PerkLevel
    {
        public string id;
        public int level;
    }

    [Serializable]
    public class SaveData
    {
        public int feathers;
        public int runs;
        public int wins;
        public int bestBattles;
        public List<PerkLevel> perks = new();
    }

    public interface ISaveStore
    {
        SaveData Load();
        void Save(SaveData data);
    }

    public sealed class JsonFileSaveStore : ISaveStore
    {
        readonly string path;

        public JsonFileSaveStore(string path) => this.path = path;

        public SaveData Load()
        {
            try
            {
                return File.Exists(path) ? JsonUtility.FromJson<SaveData>(File.ReadAllText(path)) ?? new SaveData() : new SaveData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Featherguard: could not read save ({e.Message}).");
                return new SaveData();
            }
        }

        public void Save(SaveData data)
        {
            try
            {
                File.WriteAllText(path, JsonUtility.ToJson(data, true));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Featherguard: could not write save ({e.Message}).");
            }
        }
    }

    public sealed class MemorySaveStore : ISaveStore
    {
        string json;

        public SaveData Load() => string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
        public void Save(SaveData data) => json = JsonUtility.ToJson(data);
    }
}

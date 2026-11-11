using System;
using System.IO;
using UnityEngine;

namespace Featherguard.Meta
{
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

}

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public sealed class Seeder
    {
        public bool ResetToDefaults { get; }

        public Seeder(bool resetToDefaults) => ResetToDefaults = resetToDefaults;

        public T Seed<T>(string path, Action<T> defaults, Action<T> link = null) where T : ScriptableObject
        {
            EnsureFolder(Path.GetDirectoryName(path));
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            bool created = asset == null;
            if (created)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            if (created || ResetToDefaults)
                defaults?.Invoke(asset);
            link?.Invoke(asset);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        public static void EnsureFolder(string folder)
        {
            folder = folder.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder)) return;
            EnsureFolder(Path.GetDirectoryName(folder));
            AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\', '/'), Path.GetFileName(folder));
        }
    }
}

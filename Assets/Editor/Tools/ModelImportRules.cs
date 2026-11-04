using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public class ModelImportRules : AssetPostprocessor
    {
        const string ArtRoot = "Assets/Art/";
        const string CharacterRoot = "Assets/Art/Characters/";

        [Serializable]
        class ClipMeta
        {
            public ClipInfo[] clips;
        }

        [Serializable]
        class ClipInfo
        {
            public string name;
            public int start;
            public int end;
            public bool loop;
            public EventInfo[] events;
        }

        [Serializable]
        class EventInfo
        {
            public string name;
            public int frame;
        }

        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(ArtRoot)) return;
            var importer = (ModelImporter)assetImporter;
            bool character = assetPath.StartsWith(CharacterRoot);
            importer.bakeAxisConversion = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importVisibility = false;
            importer.importBlendShapes = false;
            importer.addCollider = false;
            importer.animationType = character ? ModelImporterAnimationType.Generic : ModelImporterAnimationType.None;
            importer.importAnimation = character;
        }

        void OnPreprocessAnimation()
        {
            if (!assetPath.StartsWith(CharacterRoot)) return;
            string metaPath = Path.ChangeExtension(assetPath, null) + ".clips.json";
            if (!File.Exists(metaPath)) return;

            var meta = JsonUtility.FromJson<ClipMeta>(File.ReadAllText(metaPath));
            var importer = (ModelImporter)assetImporter;
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                string take = clip.takeName;
                string shortName = take.Contains('|') ? take[(take.LastIndexOf('|') + 1)..] : take;
                clip.name = shortName;
                var info = meta.clips.FirstOrDefault(c => c.name == shortName);
                if (info == null) continue;
                clip.loopTime = info.loop;
                float length = Mathf.Max(1, info.end - info.start);
                clip.events = info.events
                    .Select(e => new AnimationEvent { functionName = e.name, time = (e.frame - info.start) / length })
                    .ToArray();
            }
            importer.clipAnimations = clips;
        }
    }
}

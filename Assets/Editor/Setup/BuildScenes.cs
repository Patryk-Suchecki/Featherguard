using System.IO;
using System.Linq;
using UnityEditor;

namespace Featherguard.EditorTools
{
    public static class BuildScenes
    {
        static readonly string[] Order = { ContentPaths.StrongholdScene, ContentPaths.Scene };

        public static void Register() =>
            EditorBuildSettings.scenes = Order.Where(File.Exists).Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
    }
}

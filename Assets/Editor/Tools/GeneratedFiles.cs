using System.IO;
using System.Linq;

namespace Featherguard.EditorTools
{
    public static class GeneratedFiles
    {
        public static bool Write(string path, byte[] bytes)
        {
            if (File.Exists(path))
            {
                if (File.ReadAllBytes(path).SequenceEqual(bytes)) return false;
                if (new FileInfo(path).IsReadOnly)
                    throw new IOException($"Featherguard: {path} changed but is read-only. Run 'git lfs lock \"{path}\"' before regenerating it.");
            }
            File.WriteAllBytes(path, bytes);
            return true;
        }
    }
}

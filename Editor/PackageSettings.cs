using UnityEditor;

namespace Gameframe.Packages.Editor
{
    public static class PackageSettings
    {
        public static string SourcePath
        {
            get => EditorPrefs.GetString("PackageSourcePath");
            set => EditorPrefs.SetString("PackageSourcePath", value);
        }

        private static PackageManifest _manifest = new PackageManifest();
        public static PackageManifest Manifest
        {
            get => _manifest ??= new PackageManifest();
            set => _manifest = value;
        }

        public static void Load()
        {

        }

        public static void Save()
        {

        }

    }
}

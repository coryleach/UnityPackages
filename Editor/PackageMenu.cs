using UnityEditor;
using UnityEngine;

namespace Gameframe.Packages.Editor
{
  public static class PackageMenu
  {
    [MenuItem(itemName: "Window/Package Maintainer/Create")]
    public static void CreateWindow()
    {
      var window = (PackageMaintainerWindow)EditorWindow.GetWindow(typeof(PackageMaintainerWindow), false, "Package Maintainer");
      window.autoRepaintOnSceneChange = true;
      window.tab = 2;
    }

    [MenuItem("Window/Package Maintainer/Maintain")]
    public static void MaintainWindow()
    {
      var window = (PackageMaintainerWindow)EditorWindow.GetWindow(typeof(PackageMaintainerWindow), false, "Package Maintainer");
      window.autoRepaintOnSceneChange = true;
      window.tab = 0;
    }

    [MenuItem("Window/Package Maintainer/Embed")]
    public static void EmbedWindow()
    {
      var window = (PackageMaintainerWindow)EditorWindow.GetWindow(typeof(PackageMaintainerWindow), false, "Package Maintainer");
      window.autoRepaintOnSceneChange = true;
      window.tab = 1;
    }

    [MenuItem( "Window/Package Maintainer/Documentation/Custom Packages" )]
    public static void DocumentationCustomPackages()
    {
      Application.OpenURL("https://docs.unity3d.com/Manual/CustomPackages.html");
    }

    [MenuItem("Window/Package Maintainer/Documentation/Readme Markdown")]
    public static void ReadmeMarkdown()
    {
      Application.OpenURL("https://help.github.com/en/github/writing-on-github/basic-writing-and-formatting-syntax");
    }

    [MenuItem("Window/Package Maintainer/Documentation/Layout Convention")]
    public static void DocumentationPackageLayout()
    {
      Application.OpenURL("https://docs.unity3d.com/Manual/cus-layout.html");
    }

    [MenuItem( "Window/Package Maintainer/Documentation/Package Manifest" )]
    public static void DocumentationPackageManifest()
    {
      Application.OpenURL("https://docs.unity3d.com/Manual/upm-manifestPkg.html");
    }
  }
}

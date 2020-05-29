
using UnityEditor;
using UnityEngine;

namespace Gameframe.Packages.Editor
{

  public static class PackageMenu
  {
    [MenuItem("Gameframe/Packages/Create")]
    public static void CreateWindow()
    {
      var window = (PackageMaintainerWindow)EditorWindow.GetWindow(typeof(PackageMaintainerWindow), false, "Package Maintainer");
      window.autoRepaintOnSceneChange = true;
      window.tab = 2;
    }
    
    [MenuItem("Gameframe/Packages/Maintain")]
    public static void MaintainWindow()
    {
      var window = (PackageMaintainerWindow)EditorWindow.GetWindow(typeof(PackageMaintainerWindow), false, "Package Maintainer");
      window.autoRepaintOnSceneChange = true;
      window.tab = 0;
    }
    
    [MenuItem("Gameframe/Packages/Embed")]
    public static void EmbedWindow()
    {
      var window = (PackageMaintainerWindow)EditorWindow.GetWindow(typeof(PackageMaintainerWindow), false, "Package Maintainer");
      window.autoRepaintOnSceneChange = true;
      window.tab = 1;
    }

    [MenuItem( "Gameframe/Packages/Documentation/Custom Packages" )]
    public static void DocumentationCustomPackages()
    {
      Application.OpenURL("https://docs.unity3d.com/Manual/CustomPackages.html");
    }

    [MenuItem("Gameframe/Packages/Documentation/Readme Markdown")]
    public static void ReadmeMarkdown()
    {
      Application.OpenURL("https://help.github.com/en/github/writing-on-github/basic-writing-and-formatting-syntax");
    }
    

    [MenuItem("Gameframe/Packages/Documentation/Layout Convention")]
    public static void DocumentationPackageLayout()
    {
      Application.OpenURL("https://docs.unity3d.com/Manual/cus-layout.html");
    }

    [MenuItem( "Gameframe/Packages/Documentation/Package Manifest" )]
    public static void DocumentationPackageManifest()
    {
      Application.OpenURL("https://docs.unity3d.com/Manual/upm-manifestPkg.html");
    }
  }

}

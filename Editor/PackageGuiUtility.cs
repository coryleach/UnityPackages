using UnityEditor;
using UnityEngine;

namespace Gameframe.Packages.Editor
{
  public static class PackageGuiUtility
  {
    /// <summary>
    /// Draws the UI for and updates packages source path
    /// </summary>
    /// <returns>true if the source path location was modified. Otherwise false.</returns>
    public static bool SourcePathGui()
    {
      EditorGUILayout.BeginVertical("box");
      EditorGUILayout.LabelField("Source Path");
      EditorGUILayout.BeginHorizontal("box");
      EditorGUILayout.LabelField(PackageSettings.SourcePath);
      if (GUILayout.Button("Browse"))
      {
        PackageSettings.SourcePath = EditorUtility.OpenFolderPanel("Package Source", PackageSettings.SourcePath, "GitHub");
        return true;
      }
      if (GUILayout.Button("Open"))
      {
        EditorUtility.RevealInFinder(PackageSettings.SourcePath);
        return true;
      }
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.EndVertical();
      return false;
    }
    
  }

}

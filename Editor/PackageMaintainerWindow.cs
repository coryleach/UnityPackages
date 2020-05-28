using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gameframe.Packages.Editor;
using Gameframe.Packages.Utility;
using Gameframe.Shell;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Gameframe.Packages
{
  public class PackageMaintainerWindow : EditorWindow
  {
    public PackageManifest packageManifest = new PackageManifest();
    public List<PackageInfo> embededPackages = new List<PackageInfo>();
    public int selectedPackageIndex = 0;
    public string[] packageNames = new string[0];
    public List<SourcePackageInfo> sourcePackages = new List<SourcePackageInfo>();

    private ScriptableObject target = null;
    private SerializedObject serializedObject = null;
    private Vector2 scrollPt;
    private string[] toolbar = {"Manage", "Embed", "Create"};
    private int tab = 0;
    
    private void OnEnable()
    {
      target = this;
      serializedObject = new SerializedObject(target);
      packageManifest = PackageSettings.Manifest;
      Refresh();
    }

    private void OnProjectChange()
    {
      Refresh();
    }

    private void UpdateSourcePackages()
    {
      var sourcePath = PackageSettings.SourcePath;
      if (string.IsNullOrEmpty(sourcePath))
      {
        return;
      }

      sourcePackages.Clear();

      var directories = Directory.GetDirectories(sourcePath);
      foreach (var directory in directories)
      {
        //Check each directory for a package manifest
        if (File.Exists($"{directory}/package.json"))
        {
          var pkgInfo = new SourcePackageInfo(directory);
          sourcePackages.Add(pkgInfo);
        }
      }

      CheckEmbededStatus();
    }

    private void CheckEmbededStatus()
    {
      foreach (var sourcePackage in sourcePackages)
      {
        if (sourcePackage.status == SourcePackageInfo.Status.Error)
        {
          continue;
        }

        sourcePackage.status = embededPackages.Any(x => x.name == sourcePackage.packageInfo.name)
          ? SourcePackageInfo.Status.Embeded
          : SourcePackageInfo.Status.NotEmbeded;
      }
    }

    private async void UpdateEmbeddedPackages()
    {
      var request = Client.List();

      await request;

      if (request.Status == StatusCode.InProgress)
      {
        Debug.LogError("Failed to await package list response.");
        return;
      }

      if (request.Status == StatusCode.Failure)
      {
        Debug.LogError($"Get Packages Failed: {request.Error.errorCode} {request.Error.message}");
        return;
      }

      //Get All Embeded Packages
      embededPackages = request.Result.Where(x => x.source == PackageSource.Embedded).ToList();
      packageNames = embededPackages.Select(x => x.displayName).ToArray();
      CheckEmbededStatus();
      EditorUtility.SetDirty(this);

      
    }

    private async Task<PackageInfo> GetMyPackageInfoAsync()
    {
      var request = Client.List();
      await request;
      var result = request.Result.First(x => x.name == "com.gameframe.packages");
      return result;
    }

    private void Refresh()
    {
      UpdateEmbeddedPackages();
      UpdateSourcePackages();
    }

    private void OnGUI()
    {
      tab = GUILayout.Toolbar(tab, toolbar);
      switch (tab)
      {
        case 0:
          ManagePackageGUI();
          break;
        case 1:
          EmbedPackageGUI();
          break;
        case 2:
          CreatePackageGUI();
          break;
      }
    }

    private void EmbedPackageGUI()
    {
      if (PackageGuiUtility.SourcePathGui())
      {
        Refresh();
      }

      scrollPt = EditorGUILayout.BeginScrollView(scrollPt, GUILayout.ExpandHeight(false));
      foreach (var sourcePkg in sourcePackages)
      {
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField(sourcePkg.packageInfo?.displayName);
        if (sourcePkg.status == SourcePackageInfo.Status.Error)
        {
          EditorGUILayout.LabelField("Error", GUILayout.Width(60));
        }
        else if (sourcePkg.status == SourcePackageInfo.Status.Embeded)
        {
          EditorGUILayout.LabelField("Embeded", GUILayout.Width(60));
        }
        else if (GUILayout.Button("Embed", GUILayout.Width(60)))
        {
          //Create a softlink to the source package in our local package directory
          string source = sourcePkg.directoryInfo.FullName;
          string dest = $"{Application.dataPath}/../Packages/{sourcePkg.directoryInfo.Name}";
          if (!ShellUtility.CreateSymbolicLink(source, dest))
          {
            Debug.LogError("Create Sym Link Failed");
          }
        }

        EditorGUILayout.EndHorizontal();
      }

      EditorGUILayout.EndScrollView();

      RefreshGUI();
    }

    private void ManagePackageGUI()
    {
      selectedPackageIndex = EditorGUILayout.Popup("Package", selectedPackageIndex, packageNames);
      //Validate index
      if (selectedPackageIndex < 0 || selectedPackageIndex >= embededPackages.Count)
      {
        return;
      }
      
      var package = embededPackages[selectedPackageIndex];
      EditorGUILayout.BeginVertical("box");
      EditorGUILayout.LabelField(package.displayName);
      EditorGUILayout.LabelField(package.source.ToString());
      EditorGUILayout.LabelField(package.assetPath);
      EditorGUILayout.LabelField(package.resolvedPath);
      EditorGUILayout.LabelField(package.type);
      EditorGUILayout.LabelField(package.version);
      EditorGUILayout.LabelField(package.status.ToString());
      EditorGUILayout.EndVertical();

      if (GUILayout.Button("Update Readme"))
      {
        UpdateReadme(package);
      }
      
      RefreshGUI();
    }

    private const string StartDocTag = "<!-- DOC-START -->";
    private const string EndDocTag = "<!-- DOC-END -->";
    
    private static string ExtractString(string text)
    {
      var startIndex = text.IndexOf(StartDocTag, StringComparison.Ordinal) + StartDocTag.Length;
      var endIndex = text.IndexOf(EndDocTag, startIndex, StringComparison.Ordinal);
      
      if (startIndex == -1 || endIndex == -1)
      {
        return string.Empty;
      }
      
      return text.Substring(startIndex, endIndex - startIndex);
    }
    
    private async void UpdateReadme(PackageInfo packageInfo)
    {
      var myPkg = await GetMyPackageInfoAsync();
      var readmeTemplatePath = $"{myPkg.assetPath}/Template/README_TEMPLATE.md";

      var readmePath = $"{packageInfo.assetPath}/README";
      if (!File.Exists(readmePath))
      {
        readmePath = $"{packageInfo.assetPath}/README.md";
        if (!File.Exists(readmePath))
        {
          Debug.LogError("Unable to find README or README.md at package asset path");
          return;
        }
      }

      var manifestPath = $"{packageInfo.assetPath}/package.json";
      if (!File.Exists(manifestPath))
      {
        Debug.LogError($"Unable to find package.json at {packageInfo.assetPath}");
        return;
      }

      PackageManifest packageManifest = null;

      try
      {
        packageManifest = JsonUtility.FromJson<PackageManifest>(File.ReadAllText(manifestPath));
        if (packageManifest == null)
        {
          Debug.LogError("Failed to read package manifest. FromJson returned null on file text.");
          return;
        }
      }
      catch (Exception e)
      {
        Debug.LogError("Failed to read package manifest format.");
        Debug.LogException(e);
        return;
      }

      if (!ValidatePackageManifest(packageManifest))
      {
        Debug.LogError("Update package manifest with required values before updating the readme");
        return;
      }
      
      var oldText = File.ReadAllText(readmePath);
      oldText = ExtractString(oldText);

      var templateText = File.ReadAllText(readmeTemplatePath);
      var replaceText = ExtractString(templateText);
      templateText = templateText.Replace(replaceText, oldText);
      
      var readmeText = new StringBuilder(templateText);
      readmeText.Replace("{TWITTER.USERNAME}",packageManifest.author.twitter);
      readmeText.Replace("{AUTHOR.NAME}",packageManifest.author.name);
      readmeText.Replace("{GITHUB.USERNAME}",packageManifest.author.github);
      readmeText.Replace("{PACKAGE.VERSION}",packageManifest.version);
      readmeText.Replace("{PACKAGE.DESCRIPTION}",packageManifest.description);
      readmeText.Replace("{PACKAGE.DISPLAYNAME}",packageManifest.displayName);
      readmeText.Replace("{PACKAGE.NAME}",packageManifest.name);
      readmeText.Replace("{PACKAGE.USAGE}","TODO: Write Usage Documentation Here");
      readmeText.Replace("{PACKAGE.URL}",$"https://github.com/{packageManifest.author.github}/{packageManifest.repositoryName}.git#{packageManifest.version}");

      var social = new StringBuilder();
      if (!string.IsNullOrEmpty(packageManifest.author.twitter))
      {
        social.AppendLine($"* Twitter: [@{packageManifest.author.twitter}](https://twitter.com/{packageManifest.author.twitter})");
      }
      if (!string.IsNullOrEmpty(packageManifest.author.github))
      {
        social.AppendLine($"* Github: [@{packageManifest.author.github}](https://github.com/{packageManifest.author.github})");
      }
      readmeText.Replace("{AUTHOR.SOCIAL}", social.ToString());
      
      File.WriteAllText(readmePath,readmeText.ToString());

      EditorUtility.DisplayDialog("Update Readme", "Done", "OK");
    }
    
    private void RefreshGUI()
    {
      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if (GUILayout.Button("Refresh"))
      {
        Refresh();
      }
      EditorGUILayout.EndHorizontal();
    }

    private void CreatePackageGUI()
    {
      if (PackageGuiUtility.SourcePathGui())
      {
        Refresh();
      }
      
      var packageName = serializedObject.FindProperty("packageManifest.name");
      var packageDisplayName = serializedObject.FindProperty("packageManifest.displayName");
      var packageVersion = serializedObject.FindProperty("packageManifest.version");
      var packageDescription = serializedObject.FindProperty("packageManifest.description");
      var packageUnity = serializedObject.FindProperty("packageManifest.unity");
      var packageUnityRelease = serializedObject.FindProperty("packageManifest.unityRelease");
      var packageRepositoryName = serializedObject.FindProperty("packageManifest.repositoryName");

      var packageAuthorName = serializedObject.FindProperty("packageManifest.author.name");
      var packageAuthorEmail = serializedObject.FindProperty("packageManifest.author.email");
      var packageAuthorUrl = serializedObject.FindProperty("packageManifest.author.url");
      var packageAuthorGithub = serializedObject.FindProperty("packageManifest.author.twitter");
      var packageAuthorTwitter = serializedObject.FindProperty("packageManifest.author.github");

      GUILayout.BeginVertical("box");

      EditorGUILayout.LabelField("Package", EditorStyles.boldLabel);

      EditorGUI.BeginChangeCheck();
      EditorGUILayout.PropertyField(packageName);
      if (EditorGUI.EndChangeCheck())
      {
        //Enforce lowercase constraint
        packageName.stringValue = packageName.stringValue.ToLowerInvariant();
      }

      EditorGUILayout.PropertyField(packageRepositoryName);
      EditorGUILayout.PropertyField(packageDisplayName);
      EditorGUILayout.PropertyField(packageVersion);
      EditorGUILayout.PropertyField(packageDescription);
      EditorGUILayout.PropertyField(packageUnity);
      EditorGUILayout.PropertyField(packageUnityRelease);

      EditorGUILayout.LabelField("Author", EditorStyles.boldLabel);

      EditorGUILayout.PropertyField(packageAuthorName);
      EditorGUILayout.PropertyField(packageAuthorEmail);
      EditorGUILayout.PropertyField(packageAuthorUrl);
      EditorGUILayout.PropertyField(packageAuthorTwitter);
      EditorGUILayout.PropertyField(packageAuthorGithub);
      
      GUILayout.EndVertical();

      serializedObject.ApplyModifiedProperties();

      EditorGUILayout.Space();

      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Create Embeded"))
      {
        CreateEmbeded();
      }

      if (GUILayout.Button("Create at Source Path"))
      {
        CreateInSources();
      }

      EditorGUILayout.EndHorizontal();

      EditorGUILayout.Space();
    }

    private void CreateEmbeded()
    {
      if (ValidatePackageManifest(packageManifest))
      {
        CreateAt($"{Directory.GetCurrentDirectory()}/Packages");
      }
    }

    private void CreateInSources()
    {
      if (ValidatePackageManifest(packageManifest))
      {
        CreateAt(PackageSettings.SourcePath);
      }
    }

    private bool ValidatePackageManifest(PackageManifest manifest)
    {
      string error = null;
      if (string.IsNullOrEmpty(manifest.description))
      {
        error = "Package Description Required";
      }
      else if (string.IsNullOrEmpty(manifest.version))
      {
        error = "Package Version Required";
      }
      else if (string.IsNullOrEmpty(manifest.repositoryName))
      {
        error ="Package Repository Name Required";
      }
      else if (string.IsNullOrEmpty(manifest.displayName))
      {
        error = "Package display Name Required";
      }
      else if (string.IsNullOrEmpty(manifest.name))
      {
        error = "Package Name Required";
      }
      else if (string.IsNullOrEmpty(manifest.author.github))
      {
        error = "Github username required to build github links";
      }

      if (!string.IsNullOrEmpty(error))
      {
        EditorUtility.DisplayDialog("Error", error, "OK");
        EditorApplication.Beep();
        return false;
      }

      return true;
    }
    

    private async void CreateAt(string path)
    {
      var myPkgInfo = await GetMyPackageInfoAsync();

      var readmeTemplatePath = $"{myPkgInfo.assetPath}/Template/README_TEMPLATE.md";
      var licenseTemplatePath = $"{myPkgInfo.assetPath}/Template/Licenses/MIT LICENSE";
      
      if (!Directory.Exists(path))
      {
        EditorUtility.DisplayDialog("Error", "Unable to locate packages directory.", "OK");
        return;
      }

      var packagePath = $"{path}/{packageManifest.repositoryName}";
      if (Directory.Exists(packagePath))
      {
        EditorUtility.DisplayDialog("Error", "A Package with that name already exists.", "OK");
        return;
      }

      var json = EditorJsonUtility.ToJson(packageManifest, true);

      Directory.CreateDirectory(packagePath);
      Directory.CreateDirectory($"{packagePath}/Editor");
      Directory.CreateDirectory($"{packagePath}/Runtime");
      Directory.CreateDirectory($"{packagePath}/Tests/Editor");
      Directory.CreateDirectory($"{packagePath}/Tests/Runtime");

      var manifestPath = $"{packagePath}/package.json";
      var readmePath = $"{packagePath}/README.md";
      var licensePath = $"{packagePath}/LICENSE.md";

      var readmeText = new StringBuilder(File.ReadAllText(readmeTemplatePath));
      readmeText.Replace("{TWITTER.USERNAME}",packageManifest.author.twitter);
      readmeText.Replace("{AUTHOR.NAME}",packageManifest.author.name);
      readmeText.Replace("{GITHUB.USERNAME}",packageManifest.author.github);
      readmeText.Replace("{PACKAGE.VERSION}",packageManifest.version);
      readmeText.Replace("{PACKAGE.DESCRIPTION}",packageManifest.description);
      readmeText.Replace("{PACKAGE.DISPLAYNAME}",packageManifest.displayName);
      readmeText.Replace("{PACKAGE.NAME}",packageManifest.name);
      readmeText.Replace("{PACKAGE.USAGE}","TODO: Write Usage Documentation Here");
      readmeText.Replace("{PACKAGE.URL}",$"https://github.com/{packageManifest.author.github}/{packageManifest.repositoryName}.git#{packageManifest.version}");

      var social = new StringBuilder();
      if (!string.IsNullOrEmpty(packageManifest.author.twitter))
      {
        social.AppendLine($"* Twitter: [@{packageManifest.author.twitter}](https://twitter.com/{packageManifest.author.twitter})");
      }
      if (!string.IsNullOrEmpty(packageManifest.author.github))
      {
        social.AppendLine($"* Github: [@{packageManifest.author.github}](https://github.com/{packageManifest.author.github})");
      }
      readmeText.Replace("{AUTHOR.SOCIAL}", social.ToString());
      
      var licenseText = new StringBuilder(File.ReadAllText(licenseTemplatePath));
      licenseText.Replace("{DATE.YEAR}",DateTime.Now.Year.ToString());
      licenseText.Replace("{AUTHOR.NAME}",packageManifest.author.name);
      
      File.WriteAllText(manifestPath, json);
      File.WriteAllText(readmePath, readmeText.ToString());
      File.WriteAllText(licensePath, licenseText.ToString());

      var assemblyName = packageManifest.name;
      var editorAssemblyName = $"{packageManifest.name}.Editor";
      var testAssemblyName = $"{assemblyName}.Tests";
      var testEditorAssemblyName = $"{editorAssemblyName}.Tests";

      var assemblyDefPath = $"{packagePath}/Editor/{editorAssemblyName}.asmdef";
      var editorAssemblyDefPath = $"{packagePath}/Runtime/{assemblyName}.asmdef";
      var testEditorAssemblyDefPath = $"{packagePath}/Tests/Editor/{testEditorAssemblyName}.asmdef";
      var testAssemblyDefPath = $"{packagePath}/Tests/Runtime/{testAssemblyName}.asmdef";

      File.WriteAllText(assemblyDefPath,
        $"{{ \"name\": \"{editorAssemblyName}\", \"references\": [ \"{assemblyName}\" ], \"optionalUnityReferences\": [], \"includePlatforms\": [ \"Editor\" ], \"excludePlatforms\": [] }}");
      File.WriteAllText(editorAssemblyDefPath, $"{{ \"name\": \"{assemblyName}\" }}");

      File.WriteAllText(testEditorAssemblyDefPath,
        $"{{ \"name\": \"{testEditorAssemblyName}\", \"references\": [ \"{assemblyName}\" ], \"optionalUnityReferences\": [\"TestAssemblies\"], \"includePlatforms\": [ \"Editor\" ], \"excludePlatforms\": [] }}");
      File.WriteAllText(testAssemblyDefPath,
        $"{{ \"name\": \"{testAssemblyName}\", \"references\": [ \"{assemblyName}\" ], \"optionalUnityReferences\": [\"TestAssemblies\"], \"includePlatforms\": [], \"excludePlatforms\": [] }}");

      AssetDatabase.Refresh();
      EditorUtility.DisplayDialog("Package Created", "Done!", "Ok");
      Refresh();
    }
  }
}
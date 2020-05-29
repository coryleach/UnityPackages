using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private Vector2 scrollPt = Vector2.zero;
    private string[] toolbar = {"Maintain", "Embed", "Create"};
    public int tab = 0;
    
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
          MaintainPackageGUI();
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
          try
          {
            //Create a softlink to the source package in our local package directory
            string source = sourcePkg.directoryInfo.FullName;
            string dest = $"{Application.dataPath}/../Packages/{sourcePkg.directoryInfo.Name}";
            if (!ShellUtility.CreateSymbolicLink(source, dest))
            {
              Debug.LogError("Create Sym Link Failed");
            }
          }
          catch ( Exception e )
          {
            Debug.LogException(e);
          }
        }
        EditorGUILayout.EndHorizontal();
      }
      EditorGUILayout.EndScrollView();

      RefreshGUI();
    }

    private PackageManifest maintainPackageManifest = null;
    
    private void MaintainPackageGUI()
    {
      EditorGUI.BeginChangeCheck();
      
      selectedPackageIndex = EditorGUILayout.Popup("Package", selectedPackageIndex, packageNames);
      if (EditorGUI.EndChangeCheck())
      {
        //Index Changed
        maintainPackageManifest = null;
      }
      
      //Validate index
      if (selectedPackageIndex < 0 || selectedPackageIndex >= embededPackages.Count)
      {
        return;
      }
      
      var package = embededPackages[selectedPackageIndex];
      
      //Need to get the json for the package
      if (maintainPackageManifest == null)
      {
        var json = File.ReadAllText($"{package.assetPath}/package.json");
        maintainPackageManifest = JsonUtility.FromJson<PackageManifest>(json);
      }
      
      var rect = EditorGUILayout.BeginVertical("box");
      EditorGUILayout.LabelField("Name",package.name);
      EditorGUILayout.LabelField("DisplayName",package.displayName);
      EditorGUILayout.LabelField("Source",package.source.ToString());
      EditorGUILayout.LabelField("Asset Path",package.assetPath);
      EditorGUILayout.LabelField("Resolved Path",package.resolvedPath);
      EditorGUILayout.LabelField("Type",package.type);
      EditorGUILayout.LabelField("Version",package.version);
      EditorGUILayout.LabelField("Status",package.status.ToString());
      EditorGUILayout.EndVertical();
      
      if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
      {
        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>($"{package.assetPath}/package.json");
        Selection.activeObject = asset;
      }
      
      EditorGUILayout.BeginVertical("box");
      maintainPackageManifest.repositoryName = EditorGUILayout.TextField("RepositoryName", maintainPackageManifest.repositoryName);
      maintainPackageManifest.author.name = EditorGUILayout.TextField("Author Name",maintainPackageManifest.author.name);
      maintainPackageManifest.author.email = EditorGUILayout.TextField("Author E-Mail",maintainPackageManifest.author.email);
      maintainPackageManifest.author.url = EditorGUILayout.TextField("Author URL",maintainPackageManifest.author.url);

      maintainPackageManifest.author.twitter = EditorGUILayout.TextField("Twitter",maintainPackageManifest.author.twitter);
      maintainPackageManifest.author.github = EditorGUILayout.TextField("GitHub",maintainPackageManifest.author.github);
      
      var linkStyle = new GUIStyle(EditorStyles.label);
      linkStyle.wordWrap = false;
      linkStyle.hover.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
      linkStyle.normal.textColor = new Color(0,0,1);
      
      if (!string.IsNullOrEmpty(maintainPackageManifest.author.twitter))
      {
        var twitterUrl = PackageUtility.TwitterUrl(maintainPackageManifest.author.twitter);
        if (GUILayout.Button(twitterUrl,linkStyle))
        {
          Application.OpenURL(twitterUrl);
        }
      }
      
      if (!string.IsNullOrEmpty(maintainPackageManifest.author.github))
      {
        var githubUrl = PackageUtility.GithubUrl(maintainPackageManifest.author.github);
        if (GUILayout.Button(githubUrl, linkStyle))
        {
          Application.OpenURL(githubUrl);
        }
        var packageUrl = PackageUtility.PackageUrl(maintainPackageManifest.author.github,maintainPackageManifest.repositoryName,maintainPackageManifest.version);
        if (GUILayout.Button(packageUrl, linkStyle))
        {
          Application.OpenURL(packageUrl);
        }
      }
      
      EditorGUILayout.EndVertical();
      
      if (GUILayout.Button("Update package.json"))
      {
        var path = $"{package.assetPath}/package.json";
        var json = File.ReadAllText(path);
        var jsonNode = SimpleJSON.JSON.Parse(json);
        jsonNode["repositoryName"] = maintainPackageManifest.repositoryName;
        jsonNode["author"]["name"] = maintainPackageManifest.author.name;
        jsonNode["author"]["email"] = maintainPackageManifest.author.email;
        jsonNode["author"]["url"] = maintainPackageManifest.author.url;
        jsonNode["author"]["github"] = maintainPackageManifest.author.github;
        jsonNode["author"]["twitter"] = maintainPackageManifest.author.twitter;
        File.WriteAllText(path,jsonNode.ToString());
        maintainPackageManifest = null;
      }
      
      if (GUILayout.Button("Update Readme"))
      {
        UpdateReadme(package);
      }
      
      RefreshGUI();
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
      var templateText = File.ReadAllText(readmeTemplatePath);
      templateText = PackageUtility.PatchReadmeText(oldText, templateText);

      var readmeText = PackageUtility.CreateReadmeText(templateText, packageManifest);
      
      File.WriteAllText(readmePath,readmeText);

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

      var readmeText = PackageUtility.CreateReadmeText(readmeTemplatePath, packageManifest);
      var licenseText = PackageUtility.CreateLicenseText(File.ReadAllText(licenseTemplatePath),packageManifest);
      
      File.WriteAllText(manifestPath, json);
      File.WriteAllText(readmePath, readmeText);
      File.WriteAllText(licensePath, licenseText);

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
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Gameframe.Packages.Editor
{
    public class PackageCreatorWindow : EditorWindow
    {
        [MenuItem("Gameframe/Packages/Create")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(PackageCreatorWindow), false, "Create Package");
            window.autoRepaintOnSceneChange = true;
        }

        [MenuItem( "Gameframe/Packages/Documentation/Custom Packages" )]
        public static void DocumentationCustomPackages()
        {
            Application.OpenURL("https://docs.unity3d.com/Manual/CustomPackages.html");
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

        [Serializable]
        public class PackageManifest
        {
            [Serializable]
            public class PackageAuthor
            {
                public string name = "Cory Leach";
                public string email = "";
                public string url = "https://www.coryleach.com/";
            }

            public string name = "com.coryleach.mypackagename";
            public string displayName = "My Package Name";
            public string version = "0.0.1";
            public string description = "";
            public string unity = "2018.2";
            public string unityRelease = "13f1";
            public string[] keywords = new string[0];
            public PackageAuthor author = new PackageAuthor();
        }

        public PackageManifest packageManifest = new PackageManifest();

        private ScriptableObject target = null;
        private SerializedObject serializedObject = null;

        private const string PrefsKey = "PackageCreatorWindowData";
        private static string PackagesPath = "./Packages";

        private void OnEnable()
        {
            target = this;
            serializedObject = new SerializedObject(target);

            if (!PlayerPrefs.HasKey(PrefsKey))
            {
                return;
            }

            var json = PlayerPrefs.GetString(PrefsKey);
            packageManifest = JsonUtility.FromJson<PackageManifest>(json);
        }

        private void OnDisable()
        {
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(packageManifest));
        }

        private void OnGUI()
        {
            serializedObject.Update();

            var packageName = serializedObject.FindProperty("packageManifest.name");
            var packageDisplayName = serializedObject.FindProperty("packageManifest.displayName");
            var packageVersion = serializedObject.FindProperty("packageManifest.version");
            var packageDescription = serializedObject.FindProperty("packageManifest.description");
            var packageUnity = serializedObject.FindProperty("packageManifest.unity");
            var packageUnityRelease = serializedObject.FindProperty("packageManifest.unityRelease");

            var packageAuthorName = serializedObject.FindProperty("packageManifest.author.name");
            var packageAuthorEmail = serializedObject.FindProperty("packageManifest.author.email");
            var packageAuthorUrl = serializedObject.FindProperty("packageManifest.author.url");

            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Package",EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(packageName);
            if (EditorGUI.EndChangeCheck())
            {
                //Enforce lowercase constraint
                packageName.stringValue = packageName.stringValue.ToLowerInvariant();
            }

            EditorGUILayout.PropertyField(packageDisplayName);
            EditorGUILayout.PropertyField(packageVersion);
            EditorGUILayout.PropertyField(packageDescription);
            EditorGUILayout.PropertyField(packageUnity);
            EditorGUILayout.PropertyField(packageUnityRelease);

            EditorGUILayout.LabelField("Author",EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(packageAuthorName);
            EditorGUILayout.PropertyField(packageAuthorEmail);
            EditorGUILayout.PropertyField(packageAuthorUrl);

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (GUILayout.Button("Create"))
            {
                Create();
            }

            EditorGUILayout.Space();
        }

        private void Create()
        {
            if (!Directory.Exists(PackagesPath))
            {
                EditorUtility.DisplayDialog("Error", "Unable to locate packages directory.", "OK");
                return;
            }

            var packagePath = $"{PackagesPath}/{packageManifest.name}";
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
            Directory.CreateDirectory($"{packagePath}/Documentation~");

            var manifestPath = $"{packagePath}/package.json";
            var readmePath = $"{packagePath}/README.md";
            var changelogPath = $"{packagePath}/CHANGELOG.md";
            var licensePath = $"{packagePath}/LICENSE.md";
            var documentationPath = $"{packagePath}/Documentation~/{packageManifest.name}.md";

            File.WriteAllText(manifestPath, json);
            File.WriteAllText(readmePath, $"{packageManifest.name}\n\n{packageManifest.description}");
            File.WriteAllText(changelogPath, $"Created {DateTime.Now.ToShortDateString()}");
            File.WriteAllText(licensePath, $"Copyright {DateTime.Now.Year}");
            File.WriteAllText(documentationPath, $"{packageManifest.description}");

            var assemblyName = packageManifest.name;
            var editorAssemblyName = $"{packageManifest.name}.Editor";
            var testAssemblyName = $"{assemblyName}.Tests";
            var testEditorAssemblyName = $"{editorAssemblyName}.Tests";

            var assemblyDefPath = $"{packagePath}/Editor/{editorAssemblyName}.asmdef";
            var editorAssemblyDefPath = $"{packagePath}/Runtime/{assemblyName}.asmdef";
            var testEditorAssemblyDefPath = $"{packagePath}/Tests/Editor/{testEditorAssemblyName}.asmdef";
            var testAssemblyDefPath = $"{packagePath}/Tests/Runtime/{testAssemblyName}.asmdef";

            File.WriteAllText(assemblyDefPath, $"{{ \"name\": \"{editorAssemblyName}\", \"references\": [ \"{assemblyName}\" ], \"optionalUnityReferences\": [], \"includePlatforms\": [ \"Editor\" ], \"excludePlatforms\": [] }}");
            File.WriteAllText(editorAssemblyDefPath, $"{{ \"name\": \"{assemblyName}\" }}");

            File.WriteAllText(testEditorAssemblyDefPath, $"{{ \"name\": \"{testEditorAssemblyName}\", \"references\": [ \"{assemblyName}\" ], \"optionalUnityReferences\": [\"TestAssemblies\"], \"includePlatforms\": [ \"Editor\" ], \"excludePlatforms\": [] }}");
            File.WriteAllText(testAssemblyDefPath, $"{{ \"name\": \"{testAssemblyName}\", \"references\": [ \"{assemblyName}\" ], \"optionalUnityReferences\": [\"TestAssemblies\"], \"includePlatforms\": [], \"excludePlatforms\": [] }}");

            AssetDatabase.Refresh();

            if (!Provider.isActive)
            {
                return;
            }
            
            var assetList = new List<string>();
            assetList.Add(manifestPath);
            assetList.Add(readmePath);
            assetList.Add(changelogPath);
            assetList.Add(licensePath);
            assetList.Add(documentationPath);

            assetList.Add(assemblyDefPath);
            assetList.Add(editorAssemblyDefPath);
            assetList.Add(testEditorAssemblyDefPath);
            assetList.Add(testAssemblyDefPath);

            Provider.Checkout(assetList.ToArray(), CheckoutMode.Both);

        }
    }
}

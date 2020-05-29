using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Gameframe.Shell;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Gameframe.Packages.Editor
{
    public class PackageNpmPublisher : EditorWindow
    {
        public string address = "npm.coryleach.info:4873";
        public string username = "coryleach";
        public string email = "cory.leach@gmail.com";
        public string password = "";
        
        private List<PackageManifest> selectedPackageList = new List<PackageManifest>();
        private ScrollView packageScrollList;
        private Label loginStatusLabel;
        
        private static string ResourcePath = "Packages/com.gameframe.packages/Editor/PackagePublisherWindow/";

        private InstallStatus installStatus = InstallStatus.Unknown;

        private const string StyleSheetFilename = "PackageNpmPublisher.uss";
        
        public enum InstallStatus
        {
            Unknown,
            Installed,
            NotFound
        }
        
        //Commenting this out.
        //I have no plans to continue to support this for now
        /*[MenuItem("Gameframe/Packages/Publisher")]
        public static void Open()
        {
            PackageNpmPublisher wnd = GetWindow<PackageNpmPublisher>();
            wnd.titleContent = new GUIContent("Package Publisher");
        }*/

        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheetPath = ResourcePath + StyleSheetFilename;
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            if ( styleSheet != null )
            {
              root.styleSheets.Add(styleSheet);
            }
            else
            {
              Debug.LogError($"Failed to Load Style Sheet. Check uss file exists or syntax error.");
            }

            // Import UXML
            //var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.gameframe.packages/Editor/PackagePublisherWindow/PackagePublisher.uxml");
            //VisualElement labelFromUXML = visualTree.CloneTree();
            //root.Add(labelFromUXML);
            SerializedObject so = new SerializedObject(this);

            var fieldContainer = new VisualElement()
            {
                name = "FieldContainer"
            };
            fieldContainer.AddToClassList("BorderedContainer");
            root.Add(fieldContainer);

            var serverField = new TextField
            {
                label = "Server",
                bindingPath = nameof(address)
            };
            serverField.Bind(so);
            fieldContainer.Add(serverField);

            var emailField = new TextField
            {
                label = "email",
                bindingPath = nameof(email)
            };
            emailField.Bind(so);
            fieldContainer.Add(emailField);

            var usernameField = new TextField
            {
                label = "Username",
                bindingPath = nameof(username),
            };
            usernameField.Bind(so);
            fieldContainer.Add(usernameField);

            var passwordField = new TextField()
            {
                label = "Password",
                bindingPath = nameof(password),
                isPasswordField = true
            };
            passwordField.Bind(so);
            fieldContainer.Add(passwordField);

            packageScrollList = new ScrollView();
            packageScrollList.AddToClassList("BorderedContainer");
            root.Add(packageScrollList);

            var buttonContainer = new VisualElement()
            {
                name = "ButtonContainer",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center,
                }
            };
            root.Add(buttonContainer);

            loginStatusLabel = new Label("Waiting...")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleRight
                }
            };
            fieldContainer.Add(loginStatusLabel);

            var loginButton = new Button(Login)
            {
                name = "ButtonLogin",
                text = "Login",
                style =
                {
                    marginTop = 5,
                    marginLeft = 5,
                    marginRight = 5,
                    paddingTop = 4
                }
            };
            fieldContainer.Add(loginButton);

            var publishButton = new Button(Publish)
            {
                name = "ButtonPublish",
                text = "Publish",
                style =
                {
                    marginLeft = 5,
                    marginRight = 5
                }
            };
            buttonContainer.Add(publishButton);

            var refreshButton = new Button(Refresh)
            {
                name = "ButtonRefresh",
                text = "Refresh",
                style =
                {
                    marginLeft = 5,
                    marginRight = 5
                }
            };
            buttonContainer.Add(refreshButton);

            /*var installButton = new Button(InstallCli)
            {
                name = "ButtonInstall",
                style =
                {
                    marginLeft = 5,
                    marginRight = 5
                }
            };
            installButton.Add(new Label("Install"));
            buttonContainer.Add(installButton);*/

            selectedPackageList = new List<PackageManifest>();
            PopulateScrollViewWithPackages(packageScrollList);

            CheckLogin();
            CheckInstall();
        }

        private async void CheckInstall()
        {
            installStatus = InstallStatus.Unknown;
            var checkTask = await ShellUtility.ExecuteCommandAsync("npm version");
            installStatus = !checkTask ? InstallStatus.NotFound : InstallStatus.Installed;
        }

        private async void CheckLogin()
        {
            loginStatusLabel.text = "Waiting...";
            var checkTask = ShellUtility.ExecuteCommandAsync("npm whoami");
            await checkTask;
            loginStatusLabel.text = !checkTask.Result ? "Disconnected" : "Connected";
        }

        /*private void InstallCli()
        {
            Debug.Log("Installing...");
            ExecuteShellCommand("npm install -g npm-cli-login publish");
        }

        private void UpdateVersions()
        {
            var directories = Directory.GetDirectories("Packages/");
            foreach (var directory in directories)
            {
                var filename = $"{directory}/package.json";
                var json = File.ReadAllText(filename);

                var match = Regex.Match(json, @"""version"": ""(\d+\.)(\d+\.)(\d+)""");
                match = Regex.Match(match.Value, @"(\d+\.)(\d+\.)(\d+)");
                var split = match.Value.Split('.');
                var buildNumber = int.Parse(split[split.Length - 1]);
                buildNumber += 1;

                var newVersion = $"\"version\": \"{split[0]}.{split[1]}.{buildNumber}\"";
                var updatedJson = Regex.Replace(json, @"""version"": ""(\d+\.)(\d+\.)(\d+)""", newVersion);

                File.WriteAllText(filename, updatedJson);
            }
        }*/

        #region Commands

        private void Refresh()
        {
            packageScrollList.Clear();
            PopulateScrollViewWithPackages(packageScrollList);
        }

        private async void Publish()
        {
            if (selectedPackageList.Count == 0)
            {
                Debug.Log("No selected packages to publish");
                return;
            }

            foreach (var package in selectedPackageList)
            {
                //publishing
                Debug.Log($"Publishing {package.displayName} {package.version}");
                var cmd = $"npm publish Packages/{package.name} --registry http://{address}";
                var task = ShellUtility.ExecuteCommandAsync(cmd);
                await task;
                if (!task.Result)
                {
                    Debug.LogError($"Failed to publish {package.displayName}");
                }
                else
                {
                    Debug.Log($"Published {package.displayName}");
                }
            }

            Refresh();
        }

        private async void Login()
        {
            loginStatusLabel.text = "Waiting...";

            var cmd = $"npm-cli-login -u {username} -p {password} -e {email} -r http://{address}";
            var task = ShellUtility.ExecuteCommandAsync(cmd);
            await task;
            if (!task.Result)
            {
                Debug.Log("Failed login");
            }

            cmd = $"npm config set registry http://{address}";
            task = ShellUtility.ExecuteCommandAsync(cmd);
            await task;
            if (!task.Result)
            {
                Debug.Log("Failed to set registry address");
            }

            CheckLogin();
        }

        #endregion

        private static List<PackageManifest> GetLocalPackageList()
        {
            var packageList = new List<PackageManifest>();
            var directories = Directory.GetDirectories("Packages/");
            foreach (var directory in directories)
            {
                var packageManifestPath = $"{directory}/package.json";
                //publishing
                if (!File.Exists(packageManifestPath))
                {
                    continue;
                }

                var json = File.ReadAllText(packageManifestPath);
                var packageManifest = JsonUtility.FromJson<PackageManifest>(json);
                if (packageManifest != null)
                {
                    packageList.Add(packageManifest);
                }
            }
            return packageList;
        }

        private async Task<List<PackageManifest>> GetLocalPackageListAsync()
        {
            var task = Task.Run(GetLocalPackageListAsync);
            await task;
            return task.Result;
        }

        private async void PopulateScrollViewWithPackages(ScrollView scrollView)
        {
            var packages = GetLocalPackageListAsync();
            await packages;
            int row = 0;
            foreach (var package in packages.Result)
            {
                var currentPackage = package;
                var toggle = new Toggle
                {
                    label = $"{currentPackage.displayName}:{currentPackage.version}",
                };
                toggle.AddToClassList(row % 2 == 0 ? "rowEven" : "rowOdd");
                toggle.labelElement.style.flexGrow = 100;
                toggle.RegisterValueChangedCallback((value) =>
                {
                    if (value.newValue)
                    {
                        selectedPackageList.Add(currentPackage);
                    }
                    else
                    {
                        selectedPackageList.Remove(currentPackage);
                    }
                });
                scrollView.Add(toggle);
                row += 1;

                SetToggleRemoteVersionAsync(toggle,package);
            }
        }

        private static async void SetToggleRemoteVersionAsync(Toggle toggle, PackageManifest package)
        {
            PackageManifest remotePackage = null;

            var task = ShellUtility.GetCommandResultAsync($"npm view {package.name} --json");
            await task;
            var json = task.Result;

            if (json != null)
            {
                remotePackage = JsonUtility.FromJson<PackageManifest>(json);
            }

            toggle.label = remotePackage == null ? $"{toggle.label}  (not found)" : $"{toggle.label}  ({remotePackage.version})";
            toggle.SetEnabled(remotePackage == null || remotePackage.version != package.version);
            toggle.value = false;
        }
        
    }
}

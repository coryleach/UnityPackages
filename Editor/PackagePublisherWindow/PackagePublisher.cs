using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Gameframe.Packages.Editor
{
    public class PackagePublisher : EditorWindow
    {
        public string address = "npm.coryleach.info:4873";
        public string username = "coryleach";
        public string email = "cory.leach@gmail.com";
        public string password = "";
        
        private List<PackageManifest> selectedPackageList = new List<PackageManifest>();
        private ScrollView packageScrollList;
        private Label loginStatusLabel;
        
        private static string ResourcePath = "Packages/com.gameframe.packages/Editor/PackagePublisherWindow/";
        
        [MenuItem("Gameframe/Packages/Publisher")]
        public static void Open()
        {
            PackagePublisher wnd = GetWindow<PackagePublisher>();
            wnd.titleContent = new GUIContent("Package Publisher");
        }

        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheetPath = ResourcePath + "PackagePublisher.uss";
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

            CheckLoginAsync();

            Task.Run(CheckInstall);
        }

        private void CheckInstall()
        {
            var checkTask = ExecuteShellCommandAsync("npm version", false);
            if (!checkTask.Result)
            {
                Debug.Log("You need to install npm!");
            }
        }

        private async void CheckLoginAsync()
        {
            loginStatusLabel.text = "Waiting...";
            var checkTask = ExecuteShellCommandAsync("npm whoami", false);
            await checkTask;
            if (!checkTask.Result)
            {
                loginStatusLabel.text = "Disconnected";
            }
            else
            {
                loginStatusLabel.text = "Connected";
            }
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
                var task = ExecuteShellCommandAsync(cmd);
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
            var task = ExecuteShellCommandAsync(cmd);
            await task;
            if (!task.Result)
            {
                Debug.Log("Failed login");
            }

            cmd = $"npm config set registry http://{address}";
            task = ExecuteShellCommandAsync(cmd);
            await task;
            if (!task.Result)
            {
                Debug.Log("Failed to set registry address");
            }

            CheckLoginAsync();
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
            var task = Task.Run(GetLocalPackageList);
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

            var task = GetShellCommandResultAsync($"npm view {package.name} --json");
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

        #region Shell Helpers

        private static bool ExecuteShellCommand(string command, bool useShell = true)
        {
#if UNITY_EDITOR_WIN
            var commandBytes = System.Text.Encoding.Unicode.GetBytes(command);
            var encodedCommand = Convert.ToBase64String(commandBytes);
            var processInfo = new ProcessStartInfo("powershell.exe", $"-EncodedCommand {encodedCommand}")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
#else
            var processInfo = new ProcessStartInfo("/bin/bash", command.Replace("\\","\\\\"))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
#endif

            var process = Process.Start(processInfo);
            if (process == null)
            {
                Debug.LogError("Failed to execute command");
                return false;
            }

            process.WaitForExit();
            int exitCode = process.ExitCode;
            process.Close();

            return exitCode == 0;
        }

        private static string GetShellCommandResult(string command)
        {
#if UNITY_EDITOR_WIN
            var commandBytes = System.Text.Encoding.Unicode.GetBytes(command);
            var encodedCommand = Convert.ToBase64String(commandBytes);
            var processInfo = new ProcessStartInfo("powershell.exe", $"-EncodedCommand {encodedCommand}")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
#else
            var processInfo = new ProcessStartInfo("/bin/bash", command.Replace("\\","\\\\"))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
#endif

            var process = Process.Start(processInfo);
            if (process == null)
            {
                Debug.LogError("Failed to execute command");
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            int exitCode = process.ExitCode;
            process.Close();

            if (exitCode != 0)
            {
                return null;
            }

            return output;
        }

        private static async Task<string> GetShellCommandResultAsync(string command)
        {
            var task = Task.Run(() => GetShellCommandResult(command));
            await task;
            return task.Result;
        }

        private static async Task<bool> ExecuteShellCommandAsync(string command, bool useShell = true)
        {
            var task = Task.Run(() => ExecuteShellCommand(command,useShell));
            await task;
            return task.Result;
        }

        #endregion

        [Serializable]
        public class PackageManifest
        {
            public string name;
            public string version;
            public string displayName;
        }

    }
}

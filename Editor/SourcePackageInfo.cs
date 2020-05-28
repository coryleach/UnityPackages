using System;
using System.IO;
using UnityEngine;

namespace Gameframe.Packages
{
  public class SourcePackageInfo
  {
    public SourcePackageInfo(string path)
    {
      directoryInfo = new DirectoryInfo(path);
      try
      {
        status = Status.NotChecked;
        var packageJson = File.ReadAllText($"{path}/package.json");
        packageInfo = JsonUtility.FromJson<PackageManifest>(packageJson);
        if (packageInfo == null)
        {
          status = Status.Error;
          error = "Failed to get manifest from json";
        }
      }
      catch (Exception e)
      {
        status = Status.Error;
        error = e.Message;
      }
    }
    
    public DirectoryInfo directoryInfo;
    
    public PackageManifest packageInfo;

    public Status status = Status.NotChecked;

    public string error = string.Empty;
    
    public enum Status
    {
      NotChecked,
      Error,
      NotEmbeded,
      Embeded
    }
  }
}

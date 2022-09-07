using System;

namespace Gameframe.Packages
{
  [Serializable]
  public class PackageManifest
  {
    [Serializable]
    public class PackageAuthor
    {
      public string name = "";
      public string email = "";
      public string url = "";
      public string twitter = "";
      public string github = "";
      public string kofi = "";
    }

    public string githubUrl = "";
    public string name = "";
    public string displayName = "";
    public string repositoryName = "";
    public string version = "";
    public string description = "";
    public string type = ""; //tool, module, tests, sample, template, library
    public string unity = "";
    public string unityRelease = "";
    public string[] keywords = new string[0];
    public PackageAuthor author = new PackageAuthor();
  }
}

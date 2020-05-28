using System;

namespace Gameframe.Packages
{
  [Serializable]
  public class PackageManifest
  {
    [Serializable]
    public class PackageAuthor
    {
      public string name = "Cory Leach";
      public string email = "cory.leach@gmail.com";
      public string url = "https://github.com/coryleach";
      public string twitter = "coryleach";
      public string github = "coryleach";
    }

    
    public string githubUrl = "";
    public string name = "com.gameframe.mypackagename";
    public string displayName = "My Package Name";
    public string repositoryName = "RepositoryName";
    public string version = "1.0.0";
    public string description = "";
    public string type = "library"; //tool, module, tests, sample, template, library 
    public string unity = "";
    public string unityRelease = "";
    public string[] keywords = new string[0];
    public PackageAuthor author = new PackageAuthor();
  }
}

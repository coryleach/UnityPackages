using System;
using System.Text;

namespace Gameframe.Packages
{
    public static class PackageUtility
    {
        private const string StartDocTag = "<!-- DOC-START -->";
        private const string EndDocTag = "<!-- DOC-END -->";
    
        public static string ExtractText(string text)
        {
            var startIndex = text.IndexOf(StartDocTag, StringComparison.Ordinal) + StartDocTag.Length;
            var endIndex = text.IndexOf(EndDocTag, startIndex, StringComparison.Ordinal);
      
            if (startIndex == -1 || endIndex == -1)
            {
                return string.Empty;
            }
      
            return text.Substring(startIndex, endIndex - startIndex);
        }

        public static string PatchReadmeText(string oldReadmeText, string templateText)
        {
            //Extract Documenation from old readme
            oldReadmeText = ExtractText(oldReadmeText);
            //Find the replace-able text in the template
            var replaceableText = ExtractText(templateText);
            //Insert old readme text into the template
            return templateText.Replace(replaceableText, oldReadmeText);
        }

        public static string CreateLicenseText(string licenseText, PackageManifest packageManifest)
        {
            var licenseBuilder = new StringBuilder(licenseText);
            licenseBuilder.Replace("{DATE.YEAR}",DateTime.Now.Year.ToString());
            licenseBuilder.Replace("{AUTHOR.NAME}",packageManifest.author.name);
            return licenseBuilder.ToString();
        }
        
        public static string CreateReadmeText(string text, PackageManifest packageManifest)
        {
            var description = packageManifest.description;
      
            //Replace line endings so that text in the readme line breaks properly
            description = description.Replace("\r\n", "\n");
            description = description.Replace("\n", "  \n");
      
            var readmeText = new StringBuilder(text);
            readmeText.Replace("{TWITTER.USERNAME}",packageManifest.author.twitter);
            readmeText.Replace("{AUTHOR.NAME}",packageManifest.author.name);
            readmeText.Replace("{GITHUB.USERNAME}",packageManifest.author.github);
            readmeText.Replace("{PACKAGE.VERSION}",packageManifest.version);
            readmeText.Replace("{PACKAGE.DESCRIPTION}",description);
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

            return readmeText.ToString();
        }

    }
}



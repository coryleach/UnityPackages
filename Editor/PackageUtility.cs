using System;
using System.Text;

namespace Gameframe.Packages
{
    public static class PackageUtility
    {
        private const string StartDocTag = "<!-- DOC-START -->";
        private const string EndDocTag = "<!-- DOC-END -->";

        private const string StartBadgeTag = "<!-- BADGE-START -->";
        private const string EndBadgeTag = "<!-- BADGE-END -->";

        public static string ExtractDocText(string text)
        {
            return ExtractText(text, StartDocTag, EndDocTag);
        }

        public static string ExtractBadgeText(string text)
        {
            return ExtractText(text, StartBadgeTag, EndBadgeTag);
        }

        private static string ExtractText(string text, string startTag, string endTag)
        {
            var startIndex = text.IndexOf(startTag, StringComparison.Ordinal) + StartDocTag.Length;
            var endIndex = text.IndexOf(endTag, startIndex, StringComparison.Ordinal);

            if (startIndex == -1 || endIndex == -1)
            {
                return string.Empty;
            }

            return text.Substring(startIndex, endIndex - startIndex);
        }

        public static string PatchReadmeText(string oldReadmeText, string templateText)
        {
            var readmeText = new StringBuilder(templateText);

            //Extract Documenation from current readme
            var currentDocText = ExtractDocText(oldReadmeText);
            //Find the replace-able documentation in the template
            var replaceableDocText = ExtractDocText(templateText);

            //Extract Badge Text from current readme
            var currentBadgeText = ExtractBadgeText(oldReadmeText);
            //Find the repalce-able badge text in the template
            var replaceableBadgeText = ExtractBadgeText(templateText);

            //Do the replacing
            readmeText.Replace(replaceableDocText, currentDocText);
            readmeText.Replace(replaceableBadgeText, currentBadgeText);

            return readmeText.ToString();
        }

        public static string CreateLicenseText(string licenseText, PackageManifest packageManifest)
        {
            var licenseBuilder = new StringBuilder(licenseText);
            licenseBuilder.Replace("{DATE.YEAR}",DateTime.Now.Year.ToString());
            licenseBuilder.Replace("{AUTHOR.NAME}",packageManifest.author.name);
            return licenseBuilder.ToString();
        }

        public static string GithubUrl(string username)
        {
            return $"https://github.com/{username}";
        }

        public static string TwitterUrl(string username)
        {
            return $"https://twitter.com/{username}";
        }

        public static string PackageUrl(string github, string repository, string version)
        {
           return $"https://github.com/{github}/{repository}.git#{version}";
        }

        public static string CreateReadmeText(string text, PackageManifest packageManifest)
        {
            var description = packageManifest.description;

            //Replace line endings so that text in the readme line breaks properly
            description = description.Replace("\r\n", "\n");
            description = description.Replace("\n", "  \n");

            var readmeText = new StringBuilder(text);
            readmeText.Replace("{TWITTER.USERNAME}",packageManifest.author.twitter);
            readmeText.Replace("{AUTHOR.TWITTER}",packageManifest.author.name);
            readmeText.Replace("{AUTHOR.NAME}",packageManifest.author.name);
            readmeText.Replace("{GITHUB.USERNAME}",packageManifest.author.github);
            readmeText.Replace("{PACKAGE.VERSION}",packageManifest.version);
            readmeText.Replace("{PACKAGE.REPOSITORYNAME}",packageManifest.repositoryName);
            readmeText.Replace("{PACKAGE.DESCRIPTION}",description);
            readmeText.Replace("{PACKAGE.DISPLAYNAME}",packageManifest.displayName);
            readmeText.Replace("{PACKAGE.NAME}",packageManifest.name);
            readmeText.Replace("{PACKAGE.USAGE}","TODO: Write Usage Documentation Here");
            readmeText.Replace("{PACKAGE.URL}", PackageUrl(packageManifest.author.github, packageManifest.repositoryName, packageManifest.version));

            var social = new StringBuilder();
            if (!string.IsNullOrEmpty(packageManifest.author.twitter))
            {
                social.AppendLine($"* Twitter: [@{packageManifest.author.twitter}]({TwitterUrl(packageManifest.author.twitter)})");
            }
            if (!string.IsNullOrEmpty(packageManifest.author.github))
            {
                social.AppendLine($"* Github: [@{packageManifest.author.github}]({GithubUrl(packageManifest.author.github)})");
            }
            readmeText.Replace("{AUTHOR.SOCIAL}", social.ToString());

            var support = new StringBuilder();
            if (!string.IsNullOrEmpty(packageManifest.author.kofi))
            {
                support.AppendLine("<br />");
                support.AppendLine("If this is useful to you and/or you’d like to see future development and more tools in the future, please consider supporting it either by contributing to the Github projects (submitting bug reports or features and/or creating pull requests) or by buying me coffee using any of the links below. Every little bit helps!");
                support.AppendLine("<br />");
                support.AppendLine($"[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)]({packageManifest.author.kofi})");
            }
            readmeText.Replace("{AUTHOR.KOFI}", support.ToString());

            return readmeText.ToString();
        }

    }
}

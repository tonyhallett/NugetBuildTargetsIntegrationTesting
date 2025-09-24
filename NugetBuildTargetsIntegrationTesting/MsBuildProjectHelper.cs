using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting
{
    internal class MsBuildProjectHelper : IMsBuildProjectHelper
    {
        public static readonly MsBuildProjectHelper Instance = new MsBuildProjectHelper();

        public void AddPackageReference(XDocument project, string nupkgPath)
        {
            var (packageId, version) = GetPackageIdAndVersionFromNupkgPath(nupkgPath);
            var itemGroup = new XElement("ItemGroup",
                new XElement("PackageReference",
                    new XAttribute("Include", packageId),
                    new XAttribute("Version", version)
                )
            );
            project.Root!.Add(itemGroup);
        }

        private static (string packageId, string version) GetPackageIdAndVersionFromNupkgPath(string nupkgPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(nupkgPath);
            var packageNameParts = new List<string>();
            var versionParts = new List<string>();
            var foundMajor = false;
            var parts = fileName.Split('.');
            foreach (var part in parts)
            {
                if (foundMajor)
                {
                    versionParts.Add(part);
                }
                else
                {
                    if (int.TryParse(part, out _))
                    {
                        foundMajor = true;
                        versionParts.Add(part);
                    }
                    else
                    {
                        packageNameParts.Add(part);
                    }
                }
            }
            
            var packageId = string.Join(".", [.. packageNameParts]);
            var version = string.Join(".", [.. versionParts]);
            return (packageId, version);
        }

        public XElement InsertPropertyGroup(XDocument project)
        {
            var propertyGroup = new XElement("PropertyGroup");
            project.Root!.AddFirst(propertyGroup);
            return propertyGroup;
        }

        public void AddProperty(XElement propertyGroup, string name, string value)
        {
            propertyGroup.Add(new XElement(name, value));
        }
    }
}
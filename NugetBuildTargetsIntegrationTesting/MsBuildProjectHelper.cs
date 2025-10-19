using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting
{
    internal class MsBuildProjectHelper : IMsBuildProjectHelper
    {
        public static readonly MsBuildProjectHelper Instance = new MsBuildProjectHelper();

        public void AddPackageReference(XDocument project, string nupkgPath)
        {
            var (packageId, version) = NuPkgHelper.GetPackageIdAndVersionFromNupkgPath(nupkgPath);
            var itemGroup = new XElement("ItemGroup",
                new XElement("PackageReference",
                    new XAttribute("Include", packageId),
                    new XAttribute("Version", version)
                )
            );
            project.Root!.Add(itemGroup);
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
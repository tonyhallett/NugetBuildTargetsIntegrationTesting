using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting
{
    internal interface IMsBuildProjectHelper
    {
        void AddPackageReference(XDocument project, string nupkgPath);

        XElement InsertPropertyGroup(XDocument project);

        void AddProperty(XElement propertyGroup, string name, string value);
    }
}
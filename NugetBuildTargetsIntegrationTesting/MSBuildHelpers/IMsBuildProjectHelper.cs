using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting.MSBuildHelpers
{
    internal interface IMsBuildProjectHelper
    {
        void AddPackageReference(XDocument project, string nupkgPath);

        XElement InsertPropertyGroup(XDocument project);

        void AddProperty(XElement propertyGroup, string name, string value);

        bool IsSDKStyleProject(string projectFilePath);
    }
}
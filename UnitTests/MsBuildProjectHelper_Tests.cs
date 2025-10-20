using System.Xml.Linq;
using NugetBuildTargetsIntegrationTesting.MSBuildHelpers;

namespace UnitTests
{
    public class MsBuildProjectHelper_Tests
    {
        [TestCase("C:\\packages\\Package.1.0.0.nupkg", "Package","1.0.0")]
        [TestCase("C:\\packages\\Part1.Part2.2.0.0.nupkg", "Part1.Part2", "2.0.0")]
        public void Test(string nupkgPath, string expectedPackageId, string expectedVersion)
        {
            (string packageId, string version)  = GetAttributes();

            Assert.Multiple(() =>
            {
                Assert.That(packageId, Is.EqualTo(expectedPackageId));
                Assert.That(version, Is.EqualTo(expectedVersion));
            });

            (string packageId, string version) GetAttributes()
            {
                var project = XDocument.Parse("<Project Sdk=\"Microsoft.NET.Sdk\"/>");
                new MsBuildProjectHelper().AddPackageReference(project, nupkgPath);
                var itemGroup = project.Root!.FirstNode as XElement;
                var packageReference = itemGroup!.FirstNode as XElement;
                var packageId = packageReference!.Attribute("Include")!.Value;
                var version = packageReference.Attribute("Version")!.Value;
                return (packageId, version);
            }
        }

    }
}
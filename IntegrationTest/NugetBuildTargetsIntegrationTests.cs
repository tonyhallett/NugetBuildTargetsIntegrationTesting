using System.Diagnostics;
using NugetBuildTargetsIntegrationTesting;
using NugetBuildTargetsIntegrationTesting.Builder;

namespace IntegrationTest
{
    [TestFixture(true, "DependentProject.csproj")]
    [TestFixture(false, "DependentProject.csproj")]
    [TestFixture(false, "./DependentProject.csproj")]
    [TestFixture(false, ".\\DependentProject.csproj")]
    [TestFixture(false, "sub dir/DependentProject.csproj")]
    public class DependentProjectBuilderIntegrationTests(bool buildWithMSBuild, string projectFileRelativePath)
    {
        private readonly DependentProjectBuilder _dependentProjectBuilder = new();
        private string tempDir;

        [SetUp]
        public void SetUp() => tempDir = Directory.CreateTempSubdirectory().FullName;

        private static void RunCommand(string command, string args, string workingDir)
        {
            var proc = Process.Start(new ProcessStartInfo(command, args)
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            proc!.WaitForExit();
            if (proc.ExitCode != 0)
            {
                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                throw new Exception($"Command failed: {command} {args}\n{output}\n{error}");
            }
                
        }

        private void WriteTargetsToBuildDirectory()
        {
            var buildDir = Path.Combine(tempDir, "build");
            Directory.CreateDirectory(buildDir);

            // Create .targets file
            var targetsPath = Path.Combine(buildDir, "CustomBuildTarget.targets");
            File.WriteAllText(targetsPath, @$"
<Project>
  <Target Name=""CustomMessageTarget"" AfterTargets=""Build"">
    <Message Text=""Hello from CustomMessageTarget!"" Importance=""High"" />
  </Target>
</Project>
");
        }

        private string WriteNuspec()
        {
            // Create .nuspec file
            var nuspecPath = Path.Combine(tempDir, "CustomBuildTarget.nuspec");
            File.WriteAllText(nuspecPath, 
@"<?xml version=""1.0""?>
<package>
  <metadata>
    <id>CustomBuildTarget</id>
    <version>1.0.0</version>
    <authors>Test</authors>
    <description>Test package with build target</description>
  </metadata>
  <files>
    <file src=""build\CustomBuildTarget.targets"" target=""build\CustomBuildTarget.targets"" />
  </files>
</package>
");
            return nuspecPath;
        }

        private string Pack(string nuspecPath)
        {
            var nupkgPath = Path.Combine(tempDir, "CustomBuildTarget.1.0.0.nupkg");
            var packArgs = $"pack \"{nuspecPath}\" -OutputDirectory \"{tempDir}\"";
            RunCommand("nuget", packArgs, tempDir);
            if (!File.Exists(nupkgPath))
            {
                throw new Exception("pack failed");
            }

            return nupkgPath;
        }

        [Test]
        public void Setup_ShouldInjectCustomBuildTargetAndOutputMessage()
        {
            WriteTargetsToBuildDirectory();
            string nuspecPath = WriteNuspec();
            string nupkgPath = Pack(nuspecPath);

            string dependentProjectContents = 
@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
";

            IProjectBuilder projectBuilder = _dependentProjectBuilder
                .NewProject()
                .AddProject(dependentProjectContents, projectFileRelativePath)
                .AddNuPkg(nupkgPath);
            IBuildResult buildResult = buildWithMSBuild ? projectBuilder.BuildWithMSBuild() : projectBuilder.BuildWithDotNet();

            StringAssert.Contains("Hello from CustomMessageTarget!", buildResult.Output);
        }


        [TearDown]
        public void TearDown()
        {
            _dependentProjectBuilder.TearDown();
            Directory.Delete(tempDir, true);
        }
    }

}
using System.Diagnostics;
using NugetBuildTargetsIntegrationTesting;

namespace IntegrationTest
{
    public class NugetBuildTargetsIntegrationTests
    {
        private readonly NugetBuildTargetsTestSetupBuilder _testSetupBuilder = new();
        private string tempDir;

        [SetUp]
        public void SetUp()
        {
            tempDir = Directory.CreateTempSubdirectory().FullName;
        }

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
            //var nugetExe = "nuget";
            var packArgs = $"pack \"{nuspecPath}\" -OutputDirectory \"{tempDir}\"";
            //var packProcess = System.Diagnostics.Process.Start(nugetExe, packArgs);
            //packProcess.WaitForExit();
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
            var nuspecPath = WriteNuspec();
            var nupkgPath = Pack(nuspecPath);

            // Create dependent project contents
            var dependentProjectContents = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
";

            // Act: Call Setup
            var buildResult = _testSetupBuilder
                .CreateProject()
                .AddProject(dependentProjectContents)
                .AddNuPkg(nupkgPath)
                .BuildWithDotNet();

            // Assert: Output contains custom message
            StringAssert.Contains("Hello from CustomMessageTarget!", buildResult.Output);
        }


        [TearDown]
        public void TearDown()
        {
            _testSetupBuilder.TearDown();
            Directory.Delete(tempDir, true);
        }
    }

}
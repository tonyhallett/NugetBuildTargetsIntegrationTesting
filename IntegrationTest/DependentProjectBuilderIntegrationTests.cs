using System.Diagnostics;
using NugetBuildTargetsIntegrationTesting;
using NugetBuildTargetsIntegrationTesting.Builder;

namespace IntegrationTest
{
    internal sealed class DependentProjectBuilderIntegrationTests
    {
        private readonly DependentProjectBuilder _dependentProjectBuilder = new();
        private string _tempDir = string.Empty;

        [SetUp]
        public void SetUp() => _tempDir = Directory.CreateTempSubdirectory().FullName;

        private static void RunCommand(string command, string args, string workingDir)
        {
            var proc = Process.Start(new ProcessStartInfo(command, args)
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            });
            proc!.WaitForExit();
            if (proc.ExitCode == 0)
            {
                return;
            }

            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            throw new Exception($"Command failed: {command} {args}\n{output}\n{error}");
        }

        private void WriteTargetsToBuildDirectory()
        {
            string buildDir = Path.Combine(_tempDir, "build");
            _ = Directory.CreateDirectory(buildDir);

            const string targets = """
<Project>
  <Target Name="CustomMessageTarget" AfterTargets="Build">
    <Message Text="Hello from CustomMessageTarget!" Importance="High" />
  </Target>
</Project>
""";
            string targetsPath = Path.Combine(buildDir, "CustomBuildTarget.targets");
            File.WriteAllText(
                targetsPath,
                targets);
        }

        private string WriteNuspec()
        {
            string nuspecPath = Path.Combine(_tempDir, "CustomBuildTarget.nuspec");
            const string nuspec = """
<?xml version="1.0"?>
<package>
  <metadata>
    <id>CustomBuildTarget</id>
    <version>1.0.0</version>
    <authors>Test</authors>
    <description>Test package with build target</description>
  </metadata>
  <files>
    <file src="build\CustomBuildTarget.targets" target="build\CustomBuildTarget.targets" />
  </files>
</package>
""";
            File.WriteAllText(
                nuspecPath,
                nuspec);
            return nuspecPath;
        }

        private string Pack(string nuspecPath)
        {
            string nupkgPath = Path.Combine(_tempDir, "CustomBuildTarget.1.0.0.nupkg");
            string packArgs = $"pack \"{nuspecPath}\" -OutputDirectory \"{_tempDir}\"";
            RunCommand("nuget", packArgs, _tempDir);
            return !File.Exists(nupkgPath) ? throw new Exception("pack failed") : nupkgPath;
        }

        private class IntegrationTestCase(string projectFileRelativePath, bool buildWithMS, string buildArgs = "")
            : TestCaseData(projectFileRelativePath, buildWithMS, buildArgs)
        {
        }

        private static readonly TestCaseData[] s_testCases =
        [
            new IntegrationTestCase("DependentProject.csproj", true).SetName("msbuild"),
            new IntegrationTestCase("DependentProject.csproj", true, "-restore -property:Configuration=Debug").SetName("msbuildcustomargs"),
            new IntegrationTestCase("DependentProject.csproj", false).SetName("NS"),
            new IntegrationTestCase("./DependentProject.csproj", false).SetName("FS"),
            new IntegrationTestCase(".\\DependentProject.csproj", false).SetName("BS"),
            new IntegrationTestCase("sub dir/DependentProject.csproj", false).SetName("subdirwithspace"),
            new IntegrationTestCase("DependentProject.csproj", false, "-c Debug").SetName("dotentcustomargs"),
        ];

        [TestCaseSource(nameof(s_testCases))]
        public void Setup_ShouldInjectCustomBuildTargetAndOutputMessage(
            string projectFileRelativePath,
            bool buildWithMSBuild,
            string buildArgs)
        {
            WriteTargetsToBuildDirectory();
            string nuspecPath = WriteNuspec();
            string nupkgPath = Pack(nuspecPath);

            const string dependentProjectContents =
"""
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
""";

            IProjectBuilder projectBuilder = _dependentProjectBuilder
                .NewProject()
                .AddProject(dependentProjectContents, projectFileRelativePath)
                .AddNuPkg(nupkgPath);

            IBuildResult buildResult = buildArgs.Length == 0
                ? buildWithMSBuild ? projectBuilder.BuildWithMSBuild() : projectBuilder.BuildWithDotNet()
                : buildWithMSBuild ? projectBuilder.BuildWithMSBuild(buildArgs) : projectBuilder.BuildWithDotNet(buildArgs);
            StringAssert.Contains("Hello from CustomMessageTarget!", buildResult.StandardOutput);
        }

        [TearDown]
        public void TearDown()
        {
            _dependentProjectBuilder.TearDown();
            Directory.Delete(_tempDir, true);
        }
    }
}

using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.Building
{
    internal sealed class DotnetMsBuildProjectBuilder() : IDotnetMsBuildProjectBuilder
    {
        private const string DefaultDotNetBuildArguments = "-c Release";
        private readonly string _defaultMSBuildArguments = $"-restore {CreateCommandLineProperty("Configuration", "Release")}";
        private string _dotnetFileName = "dotnet";
        private string _msBuildFileName = "msbuild";

        private static string CreateCommandLineProperty(string name, string value) => $"-property:{name}={value}";

        public ProcessResult Build(string projectFilePath, bool isDotnet, string arguments, string workingDirectory)
        {
            projectFilePath = QuotePath(projectFilePath);
            return isDotnet
                ? DotNetBuild(projectFilePath, arguments, workingDirectory)
                : MSBuildBuild(projectFilePath, arguments, workingDirectory);
        }

        private ProcessResult MSBuildBuild(string quotedProjectFilePath, string arguments, string workingDirectory)
        {
            arguments = arguments.Length == 0 ? _defaultMSBuildArguments : arguments;
            return ProcessHelper.StartAndWait(
                _msBuildFileName,
                $"{QuotePath(quotedProjectFilePath)} {arguments}",
                workingDirectory);
        }

        private static string QuotePath(string path) => $"\"{path}\"";

        private ProcessResult DotNetBuild(string quotedProjectFilePath, string arguments, string workingDirectory)
        {
            arguments = arguments.Length == 0 ? DefaultDotNetBuildArguments : arguments;
            arguments = $"build {quotedProjectFilePath} {arguments}";
            return ProcessHelper.StartAndWait(_dotnetFileName, arguments, workingDirectory);
        }

        public void SetCommandPaths(string? dotNet, string? msBuild)
        {
            if (dotNet != null)
            {
                _dotnetFileName = dotNet;
            }

            if (msBuild == null)
            {
                return;
            }

            _msBuildFileName = msBuild;
        }
    }
}

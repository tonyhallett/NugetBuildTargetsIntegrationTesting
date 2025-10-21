using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.Building
{
    internal class DotnetMsBuildProjectBuilder() : IDotnetMsBuildProjectBuilder
    {
        private const string defaultDotNetBuildArguments = "-c Release";
        private string dotnetFileName = "dotnet";
        private string msBuildFileName = "msbuild";
        private string defaultMSBuildArguments = $"-restore -t:rebuild {CreateCommandLineProperty("Configuration", "Release")}";

        private static string CreateCommandLineProperty(string name, string value) => $"-property:{name}={value}";

        public ProcessResult Build(string projectFilePath, bool isDotnet, string arguments, string workingDirectory)
        {
            projectFilePath = QuotePath(projectFilePath);
            if (isDotnet)
            {
                return DotNetBuild(projectFilePath, arguments, workingDirectory);
            }

            return MSBuildBuild(projectFilePath, arguments, workingDirectory);
        }

        private ProcessResult MSBuildBuild(string quotedProjectFilePath, string arguments, string workingDirectory)
        {
            arguments = arguments == string.Empty ? defaultMSBuildArguments : arguments;
            return ProcessHelper.StartAndWait(
                msBuildFileName, 
                $"{QuotePath(quotedProjectFilePath)} {arguments}", 
                workingDirectory);
        }

        private static string QuotePath(string path) => $"\"{path}\"";

        private ProcessResult DotNetBuild(string quotedProjectFilePath, string arguments, string workingDirectory)
        {
            arguments = arguments == string.Empty ? defaultDotNetBuildArguments : arguments;
            arguments = $"build {quotedProjectFilePath} {arguments}";
            return ProcessHelper.StartAndWait(dotnetFileName, arguments, workingDirectory);
        }

        public void SetCommandPaths(string? dotNet, string? msBuild)
        {
            if (dotNet != null)
            {
                dotnetFileName = dotNet;
            }

            if (msBuild != null)
            {
                msBuildFileName = msBuild;
            }
        }
    }
}

using NugetBuildTargetsIntegrationTesting.DotNet;
using NugetBuildTargetsIntegrationTesting.MSBuildHelpers;
using NugetBuildTargetsIntegrationTesting.Processing;

namespace NugetBuildTargetsIntegrationTesting.Building
{
    internal class DotnetMsBuildProjectBuilder(IMsBuildProjectHelper msBuildProjectHelper, IDotNetSdk dotNetSdk) : IDotnetMsBuildProjectBuilder
    {
        private const string defaultDotNetBuildArguments = "-c Release";
        private readonly IMsBuildProjectHelper msBuildProjectHelper = msBuildProjectHelper;
        private readonly IDotNetSdk dotNetSdk = dotNetSdk;
        private string dotnetFileName = "dotnet";
        private string msBuildFileName = "msbuild";

        private static string CreateCommandLineProperty(string name, string value) => $"-property:{name}={value}";

        private static string GetDefaultMSBuildArguments()
        {
            return $"-restore -t:rebuild {CreateCommandLineProperty("Configuration", "Release")}";
        }

        public ProcessResult Build(string projectFilePath, bool isDotnet, string arguments, string workingDirectory)
        {
            if (isDotnet)
            {
                arguments = arguments == string.Empty ? defaultDotNetBuildArguments : arguments;
                return DotNetBuild(projectFilePath, arguments, workingDirectory);
            }

            return MSBuildBuild(projectFilePath, arguments, workingDirectory);
        }

        private ProcessResult MSBuildBuild(string projectFilePath, string arguments, string workingDirectory)
        {
            if (arguments == string.Empty)
            {
                arguments = GetDefaultMSBuildArguments();
                var isSDKStyle = msBuildProjectHelper.IsSDKStyleProject(projectFilePath);
                if (isSDKStyle)
                {
                    var msBuildSDKsPath = dotNetSdk.GetActiveSdkSdksPath();
                    if (msBuildSDKsPath == null)
                    {
                        return new ProcessResult("", "Cannot find dotnet sdk path", 1);
                    }
                    
                    arguments = $"{CreateCommandLineProperty("MSBuildSDKsPath", QuotePath(msBuildSDKsPath))} {arguments}";
                }
            }
            arguments = $"{QuotePath(projectFilePath)} {arguments}";
            return ProcessHelper.StartAndWait(msBuildFileName, arguments, workingDirectory);
        }

        private static string QuotePath(string path) => $"\"{path}\"";

        private ProcessResult DotNetBuild(string projectFilePath, string arguments, string workingDirectory)
            => DotNetCommand("build", projectFilePath, workingDirectory, arguments);

        private ProcessResult DotNetCommand(string command, string projectFilePath, string workingDirectory, string additionalArguments = "")
        {
            var arguments = $"{command} \"{projectFilePath}\" {additionalArguments}";
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

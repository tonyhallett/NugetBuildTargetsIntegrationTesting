using System.Xml.Linq;
using NugetBuildTargetsIntegrationTesting.IO;
using NugetBuildTargetsIntegrationTesting.MSBuildHelpers;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    /*
        see https://github.com/NuGet/Home/wiki/%5BSpec%5D-NuGet-settings-in-MSBuild for
        RestorePackagesPath and RestoreSources
    */
    internal sealed class NugetTempEnvironmentManager : INugetTempEnvironmentManager
    {
        private const string NuGetApiSource = "https://api.nuget.org/v3/index.json";
        private readonly IIOUtilities _ioUtilities;
        private readonly INugetAddCommand _nugetAddCommand;
        private readonly IMsBuildProjectHelper _msBuildProjectHelper;
        private readonly INuGetGlobalPackagesPathProvider _nuGetGlobalPackagesPathProvider;
        private readonly HashSet<string> _addedPackages = [];
        private string? _localFeedPath;

        internal NugetTempEnvironmentManager(
            IIOUtilities ioUtilities,
            INugetAddCommand nugetAddCommand,
            IMsBuildProjectHelper msBuildProjectHelper,
            INuGetGlobalPackagesPathProvider nuGetGlobalPackagesPathProvider)
        {
            _ioUtilities = ioUtilities;
            _nugetAddCommand = nugetAddCommand;
            _msBuildProjectHelper = msBuildProjectHelper;
            _nuGetGlobalPackagesPathProvider = nuGetGlobalPackagesPathProvider;
        }

        public async Task SetupAsync(string nupkgPath, XDocument project, string packageInstallPath, string? nugetCommandPath)
        {
            XElement propertyGroup = _msBuildProjectHelper.InsertPropertyGroup(project);

            await SetUpForTempSourceAsync(nupkgPath, propertyGroup, nugetCommandPath);
            SetupTempPackageInstallPath(propertyGroup, packageInstallPath);
        }

        private async Task SetUpForTempSourceAsync(string nupkgPath, XElement propertyGroup, string? nugetCommandPath)
        {
            await AddPackageToTempSourceAsync(nupkgPath, nugetCommandPath);
            /*
                https://www.nuget.org/packages/Microsoft.Net.Sdk.Compilers.Toolset
                This package is automatically downloaded when your MSBuild version does not match your SDK version.
                Then the package is used to build your project with the compiler version matching your SDK version
                instead of the one bundled with MSBuild.

                fallback to download
             */
            string globalPackagesPath = _nuGetGlobalPackagesPathProvider.Provide();
            _msBuildProjectHelper.AddProperty(propertyGroup, "RestoreSources", $"{_localFeedPath!};{globalPackagesPath};{NuGetApiSource}");
        }

        private async Task AddPackageToTempSourceAsync(string nupkgPath, string? nugetCommandPath)
        {
            _localFeedPath ??= _ioUtilities.CreateTempDirectory();
            if (!_addedPackages.Add(nupkgPath))
            {
                return;
            }

            Processing.ProcessResult processResult = await _nugetAddCommand.AddPackageToSourceAsync(nupkgPath, _localFeedPath, nugetCommandPath);
            if (processResult.ExitCode == 0)
            {
                return;
            }

            throw new NugetAddException(processResult.StandardError, processResult.StandardOutput, processResult.ExitCode);
        }

        private void SetupTempPackageInstallPath(XElement propertyGroup, string packageInstallPath)
            => _msBuildProjectHelper.AddProperty(
                propertyGroup,
                "RestorePackagesPath",
                packageInstallPath);

        public void CleanUp() => _ioUtilities.TryDeleteDirectoryRecursive(_localFeedPath);
    }
}

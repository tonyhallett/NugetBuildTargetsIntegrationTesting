using System.Xml.Linq;
using NugetBuildTargetsIntegrationTesting.IO;
using NugetBuildTargetsIntegrationTesting.MSBuildHelpers;

namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    internal class NugetTempEnvironmentManager : INugetTempEnvironmentManager
    {
        private readonly IIOUtilities _ioUtilities;
        private readonly INugetAddCommand _nugetAddCommand;
        private readonly IMsBuildProjectHelper _msBuildProjectHelper;
        private string? _localFeedPath;
        private readonly HashSet<string> _addedPackages = [];

        public NugetTempEnvironmentManager() :
            this(IOUtilities.Instance, new NugetAddCommand(), MsBuildProjectHelper.Instance)
        {
        }

        internal NugetTempEnvironmentManager(
            IIOUtilities ioUtilities,
            INugetAddCommand nugetAddCommand,
            IMsBuildProjectHelper msBuildProjectHelper)
        {
            _ioUtilities = ioUtilities;
            _nugetAddCommand = nugetAddCommand;
            _msBuildProjectHelper = msBuildProjectHelper;
        }

        public void Setup(string nupkgPath, XDocument project, string packageInstallPath, string? nugetCommandPath)
        {
            var propertyGroup = _msBuildProjectHelper.InsertPropertyGroup(project);

            SetUpForTempSource(nupkgPath, propertyGroup, nugetCommandPath);
            SetupTempPackageInstallPath(propertyGroup, packageInstallPath);
        }

        private void SetUpForTempSource(string nupkgPath, XElement propertyGroup, string? nugetCommandPath)
        {
            AddPackageToTempSource(nupkgPath, nugetCommandPath);
            _msBuildProjectHelper.AddProperty(propertyGroup, "RestoreSources", $"{_localFeedPath!};");
        }

        private void AddPackageToTempSource(string nupkgPath, string? nugetCommandPath)
        {
            _localFeedPath ??= _ioUtilities.CreateTempDirectory();
            if (_addedPackages.Add(nupkgPath))
            {
                Processing.ProcessResult processResult = _nugetAddCommand.AddPackageToSource(nupkgPath, _localFeedPath, nugetCommandPath);
                if (processResult.ExitCode != 0)
                {
                    throw new NugetAddException(processResult.Error, processResult.Output, processResult.ExitCode);
                }
            }
        }

        private void SetupTempPackageInstallPath(XElement propertyGroup, string packageInstallPath)
            => _msBuildProjectHelper.AddProperty(
                propertyGroup,
                "RestorePackagesPath",
                packageInstallPath);

        public void CleanUp() => _ioUtilities.TryDeleteDirectoryRecursive(_localFeedPath);
    }
}
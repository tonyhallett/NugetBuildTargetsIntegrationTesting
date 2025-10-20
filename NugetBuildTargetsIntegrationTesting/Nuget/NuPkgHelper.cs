namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    public static class NuPkgHelper
    {
        public static (string PackageId, string Version) GetPackageIdAndVersionFromNupkgPath(string nupkgPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(nupkgPath);
            var packageNameParts = new List<string>();
            var versionParts = new List<string>();
            var foundMajor = false;
            var parts = fileName.Split('.');
            foreach (var part in parts)
            {
                if (foundMajor)
                {
                    versionParts.Add(part);
                }
                else
                {
                    if (int.TryParse(part, out _))
                    {
                        foundMajor = true;
                        versionParts.Add(part);
                    }
                    else
                    {
                        packageNameParts.Add(part);
                    }
                }
            }

            var packageId = string.Join(".", [.. packageNameParts]);
            var version = string.Join(".", [.. versionParts]);
            return (packageId, version);
        }
    }
}
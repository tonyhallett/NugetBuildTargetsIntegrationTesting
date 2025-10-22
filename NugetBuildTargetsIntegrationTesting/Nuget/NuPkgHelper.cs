namespace NugetBuildTargetsIntegrationTesting.Nuget
{
    public static class NuPkgHelper
    {
        public static (string PackageId, string Version) GetPackageIdAndVersionFromNupkgPath(string nupkgPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(nupkgPath);
            var packageNameParts = new List<string>();
            var versionParts = new List<string>();
            bool foundMajor = false;
            string[] parts = fileName.Split('.');
            foreach (string part in parts)
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

            string packageId = string.Join(".", [.. packageNameParts]);
            string version = string.Join(".", [.. versionParts]);
            return (packageId, version);
        }
    }
}

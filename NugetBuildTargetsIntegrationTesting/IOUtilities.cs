using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting
{
    internal class IOUtilities : IIOUtilities
    {
        public static IOUtilities Instance { get; } = new IOUtilities();

        public string CreateTempDirectory()
        {
            return Directory.CreateTempSubdirectory().FullName;
        }

        public string CreateUniqueSubdirectory(string parentDirectory)
        {
            string uniqueDir = Path.Combine(parentDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(uniqueDir);
            return uniqueDir;
        }

        public void TryDeleteDirectoryRecursive(string? directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return;
                }

                Directory.Delete(directoryPath, true);
            }
            catch
            {
                // Ignore exceptions during cleanup
            }
        }

        public string SaveXDocumentToDirectory(XDocument doc, string directory, string fileName)
        {
            var path = Path.Combine(directory, fileName);
            doc.Save(path);
            return path;
        }

        public XDocument XDocParse(string text) 
            => XDocument.Parse(text);
    }
}

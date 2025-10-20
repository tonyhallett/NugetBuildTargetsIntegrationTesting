using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting.IO
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
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var path = Path.Combine(directory, fileName);
            doc.Save(path);
            return path;
        }

        public XDocument XDocParse(string text) => XDocument.Parse(text);

        public void AddRelativeFile(string directory, string relativePath, string contents)
        {
            var filePath = Path.Combine(directory, relativePath);
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath!);
            }

            File.WriteAllText(filePath, contents);
        }
    }
}

using System.Xml.Linq;

namespace NugetBuildTargetsIntegrationTesting
{
    internal interface IIOUtilities
    {
        string CreateTempDirectory();

        string CreateUniqueSubdirectory(string parentDirectory);
        
        void TryDeleteDirectoryRecursive(string? directoryPath);
        
        string SaveXDocumentToDirectory(XDocument doc, string directory, string fileName);
        
        XDocument XDocParse(string text);

        void AddRelativeFile(string directory, string relativePath, string contents);
    }
}

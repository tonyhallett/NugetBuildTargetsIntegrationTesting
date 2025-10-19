
namespace NugetBuildTargetsIntegrationTesting
{
    public class ProjectBuildContext { 
        public ProjectBuildContext(
            string projectContents, 
            string projectRelativePath, 
            string nuPkgPath, 
            List<(string Contents, string RelativePath)>? _files) 
        {
            ProjectContents = projectContents;
            ProjectRelativePath = projectRelativePath;
            NuPkgPath = nuPkgPath;
            Files = _files;
        }

        public string ProjectContents { get; }
        public string ProjectRelativePath { get; }
        public string NuPkgPath { get; }
        public List<(string Contents, string RelativePath)>? Files { get; }
    }
}

using System.Diagnostics;

namespace NugetBuildTargetsIntegrationTesting
{
    internal class NugetAddCommand : INugetAddCommand
    {
        private static void RunCommand(string command, string args, string workingDir)
        {
            var proc = Process.Start(new ProcessStartInfo(command, args)
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            proc!.WaitForExit();
            if (proc.ExitCode != 0)
                throw new Exception($"Command failed: {command} {args}\n{proc.StandardOutput.ReadToEnd()}\n{proc.StandardError.ReadToEnd()}");
        }

        public void AddPackageToSource(string nupkgPath, string source)
        {
            var args = $"add \"{nupkgPath}\" -Source \"{source}\"";
            RunCommand("nuget", args, Directory.GetCurrentDirectory());
        }
    }
}
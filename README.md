# DependentProjectBuilder

## Overview

`DependentProjectBuilder` streamlines integration testing for NuGet packages providing build targets.
It creates a temporary nuget environment with temporary dependent projects that are built for you, enabling custom targets to be run and their behaviour asserted.

## Command requirements

The NuGet CLI [add command](https://learn.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-add) is used.

If you `BuildWithDotNet` then the .Net CLI [build command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) is used.

If you `BuildWithMSBuild` then [msbuild.exe](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference?view=vs-2022) is used.

Each of these are expected to be on Path.

If this is not the case then the `DependentProjectBuilder` constructor can be passed a `CommandPaths` object.

## Usage

1.  In your tests create a field of DependentProjectBuilder.
2.  CreateProject()
3.  If required AddFiles - `AddFiles(IEnumerable<(string Contents, string RelativePath)> files);`
4.  Add the proj file as a string - `AddProject(string projectContents, string relativePath);`
5.  Add the nupkg to test - `AddNuPkg(string nuPkgPath);`
6.  Build - if required specify your own arguments that will be appended to the generated project file path

    `IBuildResult BuildWithDotNet(string arguments = "");`

    Default is - `-c Release`

    `IBuildResult BuildWithMSBuild(string arguments = "");`

    Default is `-restore -property:Configuration=Release`

    If your task is not working you may want to add the binary logger switch for viewing with the [MSBuild Structured Log Viewer](https://www.msbuildlog.com/)

    **BuildWithMSBuild sdk style** 
    The package [Microsoft.Net.Sdk.Compilers.Toolset](https://www.nuget.org/packages/Microsoft.Net.Sdk.Compilers.Toolset) may be rquired.

    > This package is automatically downloaded when your MSBuild version does not match your SDK version.
    
    If you already have the corresponding version in `{userprofile}\.nuget\packages` it will be used **otherwise internet connectivity is required.**

7.  If you need to rebuild the return value of `BuildWithDotNet` and `BuildWithMSBuild`, `IBuildResult`, has

    `IBuildResult AddFiles(IEnumerable<(string Contents, string RelativePath)> files);`

    `void Rebuild(string? args = null);`

    Rebuild will use the command that you chose before. If you do not specify args then it will use the same args from before.

## Failing build

The `IBuildResult` has the following properties

```
    string StandardError { get; }

    string ErrorAndOutput { get; }

    string StandardOutput { get; }

    bool Passed { get; }
```

## Temp directories

The `IBuildResult` has the properties

```
    DirectoryInfo ContainingDirectory { get; }

    DirectoryInfo ProjectDirectory { get; }
```

Use `DependentProjectBuilder` `TearDown()` in a one time tear down to remove all temp directories.

### NUnitTest

See the IntegrationTest project in the GitHub repository for a simple test that uses this setup class.

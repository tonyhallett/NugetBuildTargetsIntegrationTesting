## NugetBuildTargetsIntegrationTesting.NugetBuildTargetsTestSetup

### Overview

`NugetBuildTargetsTestSetup` streamlines integration testing for NuGet packages providing build targets. 
It creates a temporary nuget environment with temporary dependent projects that are built for you, enabling custom targets to be run and their behaviour asserted.

### Constructor Requirements

You can use the default constructor if both `nuget` and `dotnet` are available on your system path.  
If these requirements cannot be met, you may use the alternative constructors to inject custom implementations for project building or NuGet package management:

- `NugetBuildTargetsTestSetup(IProjectBuilder projectBuilder)`
- `NugetBuildTargetsTestSetup(INugetAddCommand nugetAddCommand)`
- `NugetBuildTargetsTestSetup(IProjectBuilder projectBuilder, INugetAddCommand nugetAddCommand)`



### Example NUnit Test

Below is an example NUnit test using `NugetBuildTargetsTestSetup` as a private readonly variable, with `TearDown` called in a `OneTimeTearDown`:


```csharp
using NUnit.Framework;
using NugetBuildTargetsIntegrationTesting;

namespace IntegrationTests
{
    [TestFixture]
    public class NugetBuildTargetsTests
    {
        private readonly NugetBuildTargetsTestSetup _testSetup = new NugetBuildTargetsTestSetup();

        [Test]
        public void CanBuildProjectWithNugetPackage()
        {
            string projectXml = "<Project Sdk=\"Microsoft.NET.Sdk\"/>";
            string nupkgPath = @"C:\packages\MyPackage.1.0.0.nupkg";

            string buildOutput = _testSetup.Setup(projectXml, nupkgPath);

            Assert.IsNotNull(buildOutput);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _testSetup.TearDown();
        }
    }
}

```

### Actual NUnitTest

See the IntegrationTest project in the GitHub repository for a simple test that uses this setup class.

### Setup

If additional files need to be added for the dependent project you can use the projectPathCallback parameter.
This provides the path to the temporary dependent project so you can add files as needed, prior to building.
```csharp
public string Setup(string dependentProjectContents, string nupkgPath, Action<string>? projectPathCallback = null)
```


### Additional Information

`NugetTempEnvironmentManager` is available if you would like to use it as part of your own setup class.

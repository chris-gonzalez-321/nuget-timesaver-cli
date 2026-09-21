using NugetTimesaver.Cli.Core;
using Xunit;

namespace NugetTimesaver.Cli.Tests;

public class ProjectFileReaderTests : IDisposable
{
    private readonly string _projectPath;

    public ProjectFileReaderTests()
    {
        _projectPath = Path.Combine(Directory.CreateTempSubdirectory("nuget-t-projfile-tests-").FullName, "Sample.csproj");
    }

    public void Dispose()
    {
        Directory.Delete(Path.GetDirectoryName(_projectPath)!, recursive: true);
    }

    [Fact]
    public void ReadPackageReferences_ReturnsIdAndVersionForEachReference()
    {
        File.WriteAllText(_projectPath, """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="IntegrationBroker.Core" Version="2.0.26126.1-test" />
                <PackageReference Include="IntegrationBroker.Logging" Version="2.0.26085.1-test" />
              </ItemGroup>
            </Project>
            """);

        var references = ProjectFileReader.ReadPackageReferences(_projectPath);

        Assert.Equal(2, references.Count);
        Assert.Contains(references, r => r.PackageId == "IntegrationBroker.Core" && r.Version == "2.0.26126.1-test");
        Assert.Contains(references, r => r.PackageId == "IntegrationBroker.Logging" && r.Version == "2.0.26085.1-test");
        Assert.All(references, r => Assert.Equal(_projectPath, r.ProjectPath));
    }

    [Fact]
    public void ReadPackageReferences_SkipsReferencesWithoutAVersionAttribute()
    {
        // e.g. Central Package Management style references, which this tool doesn't support.
        File.WriteAllText(_projectPath, """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" />
              </ItemGroup>
            </Project>
            """);

        var references = ProjectFileReader.ReadPackageReferences(_projectPath);

        Assert.Empty(references);
    }

    [Fact]
    public void ReadPackageReferences_NoPackageReferences_ReturnsEmpty()
    {
        File.WriteAllText(_projectPath, """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        var references = ProjectFileReader.ReadPackageReferences(_projectPath);

        Assert.Empty(references);
    }
}

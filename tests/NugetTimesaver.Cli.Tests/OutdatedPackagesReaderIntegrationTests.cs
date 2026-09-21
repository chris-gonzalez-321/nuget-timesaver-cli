using NugetTimesaver.Cli.Core;
using Xunit;

namespace NugetTimesaver.Cli.Tests;

/// <summary>
/// Exercises the real `dotnet` CLI against a project with an unresolvable package reference —
/// a regression guard for the bug where a project that fails to restore surfaced an empty error
/// message (dotnet writes its real errors to stdout, not stderr) and aborted the whole run
/// instead of reporting just that project as failed.
/// </summary>
public class OutdatedPackagesReaderIntegrationTests : IDisposable
{
    private readonly string _projectDir;
    private readonly string _projectPath;

    public OutdatedPackagesReaderIntegrationTests()
    {
        _projectDir = Directory.CreateTempSubdirectory("nuget-t-outdated-tests-").FullName;
        _projectPath = Path.Combine(_projectDir, "Broken.csproj");

        File.WriteAllText(_projectPath, """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" Version="999.999.999" />
              </ItemGroup>
            </Project>
            """);
    }

    public void Dispose()
    {
        Directory.Delete(_projectDir, recursive: true);
    }

    [Fact]
    public async Task GetOutdatedAsync_ProjectFailsToRestore_ReturnsFailureWithDiagnostics_InsteadOfThrowing()
    {
        var result = await OutdatedPackagesReader.GetOutdatedAsync(
            _projectPath, FeedResolver.DefaultFeedUrl, includePrerelease: false);

        Assert.False(result.Succeeded);
        Assert.Empty(result.Updates);
        Assert.NotEmpty(result.DiagnosticLines);

        // The real `dotnet` failure text ("Restore failed. Run `dotnet restore` for more
        // details...") should come through, parsed out of the structured "problems" JSON
        // report — not a bare "level": "error" line from naive text grepping.
        Assert.Contains(result.DiagnosticLines, line => line.Contains("Restore failed", StringComparison.OrdinalIgnoreCase));
    }
}

using NugetTimesaver.Cli.Core;
using Xunit;

namespace NugetTimesaver.Cli.Tests;

public class OutdatedPackagesReaderTests
{
    // Captured verbatim from `dotnet list package --outdated --format json --output-version 1`
    // against a real project referencing an outdated package.
    private const string SampleJsonWithOutdatedPackage = """
        {
          "version": 1,
          "parameters": "--outdated --include-prerelease",
          "sources": [
            "https://api.nuget.org/v3/index.json"
          ],
          "projects": [
            {
              "path": "/repo/src/Probe/Probe.csproj",
              "frameworks": [
                {
                  "framework": "net10.0",
                  "topLevelPackages": [
                    {
                      "id": "Newtonsoft.Json",
                      "requestedVersion": "12.0.1",
                      "resolvedVersion": "12.0.1",
                      "latestVersion": "13.0.5-beta1"
                    }
                  ]
                }
              ]
            }
          ]
        }
        """;

    // Captured when no packages are outdated: the "frameworks" key is entirely absent.
    private const string SampleJsonWithNoOutdatedPackages = """
        {
          "version": 1,
          "parameters": "--outdated",
          "sources": [
            "https://api.nuget.org/v3/index.json"
          ],
          "projects": [
            {
              "path": "/repo/src/NugetTimesaver.Cli/NugetTimesaver.Cli.csproj"
            }
          ]
        }
        """;

    [Fact]
    public void ParseOutdatedJson_WithOutdatedPackage_ReturnsIt()
    {
        var updates = OutdatedPackagesReader.ParseOutdatedJson(SampleJsonWithOutdatedPackage);

        var update = Assert.Single(updates);
        Assert.Equal("/repo/src/Probe/Probe.csproj", update.ProjectPath);
        Assert.Equal("Newtonsoft.Json", update.PackageId);
        Assert.Equal("12.0.1", update.CurrentVersion);
        Assert.Equal("13.0.5-beta1", update.LatestVersion);
    }

    [Fact]
    public void ParseOutdatedJson_WithNoOutdatedPackages_ReturnsEmpty()
    {
        var updates = OutdatedPackagesReader.ParseOutdatedJson(SampleJsonWithNoOutdatedPackages);

        Assert.Empty(updates);
    }

    [Fact]
    public void ParseOutdatedJson_DuplicatePackageAcrossFrameworks_IsDeduplicated()
    {
        const string json = """
            {
              "version": 1,
              "parameters": "--outdated",
              "sources": [],
              "projects": [
                {
                  "path": "/repo/src/Multi/Multi.csproj",
                  "frameworks": [
                    {
                      "framework": "net8.0",
                      "topLevelPackages": [
                        { "id": "Newtonsoft.Json", "requestedVersion": "12.0.1", "resolvedVersion": "12.0.1", "latestVersion": "13.0.3" }
                      ]
                    },
                    {
                      "framework": "net10.0",
                      "topLevelPackages": [
                        { "id": "Newtonsoft.Json", "requestedVersion": "12.0.1", "resolvedVersion": "12.0.1", "latestVersion": "13.0.3" }
                      ]
                    }
                  ]
                }
              ]
            }
            """;

        var updates = OutdatedPackagesReader.ParseOutdatedJson(json);

        Assert.Single(updates);
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using NugetTimesaver.Cli.Core.Models;

namespace NugetTimesaver.Cli.Core;

/// <summary>Runs and parses `dotnet list package --outdated --format json`.</summary>
public static class OutdatedPackagesReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<IReadOnlyList<PackageUpdate>> GetOutdatedAsync(
        string projectPath, string feedUrl, bool includePrerelease)
    {
        var args = new List<string>
        {
            "list", projectPath, "package", "--outdated",
            "--format", "json", "--output-version", "1",
            "--source", feedUrl,
        };

        if (includePrerelease)
        {
            args.Add("--include-prerelease");
        }

        var result = await DotnetCli.RunAsync(args.ToArray());
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"`dotnet list package --outdated` failed for {projectPath}:\n{result.StandardError}");
        }

        return ParseOutdatedJson(result.StandardOutput);
    }

    /// <summary>Flattens the `dotnet list package --outdated --format json` report into top-level package updates.</summary>
    public static IReadOnlyList<PackageUpdate> ParseOutdatedJson(string json)
    {
        var report = JsonSerializer.Deserialize<OutdatedReport>(json, JsonOptions)
            ?? throw new InvalidOperationException("Unexpected empty output from `dotnet list package --outdated`.");

        var updates = new List<PackageUpdate>();
        var seen = new HashSet<(string ProjectPath, string PackageId)>();

        foreach (var project in report.Projects ?? [])
        {
            foreach (var framework in project.Frameworks ?? [])
            {
                foreach (var package in framework.TopLevelPackages ?? [])
                {
                    if (!seen.Add((project.Path, package.Id)))
                    {
                        continue;
                    }

                    updates.Add(new PackageUpdate(project.Path, package.Id, package.ResolvedVersion, package.LatestVersion));
                }
            }
        }

        return updates;
    }

    private sealed record OutdatedReport(
        [property: JsonPropertyName("projects")] List<ProjectReport>? Projects);

    private sealed record ProjectReport(
        [property: JsonPropertyName("path")] string Path,
        [property: JsonPropertyName("frameworks")] List<FrameworkReport>? Frameworks);

    private sealed record FrameworkReport(
        [property: JsonPropertyName("topLevelPackages")] List<TopLevelPackage>? TopLevelPackages);

    private sealed record TopLevelPackage(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("resolvedVersion")] string ResolvedVersion,
        [property: JsonPropertyName("latestVersion")] string LatestVersion);
}

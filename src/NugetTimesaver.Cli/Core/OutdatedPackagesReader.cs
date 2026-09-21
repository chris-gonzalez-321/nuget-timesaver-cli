using System.Text.Json;
using System.Text.Json.Serialization;
using NugetTimesaver.Cli.Core.Models;

namespace NugetTimesaver.Cli.Core;

/// <summary>Runs and parses `dotnet list package --outdated --format json`.</summary>
public static class OutdatedPackagesReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Checks one project for outdated packages. Does not throw on a `dotnet` failure (e.g. a
    /// pre-existing restore error in that project) — callers checking many projects at once
    /// should report that project as failed and keep going rather than abort the whole run.
    /// </summary>
    public static async Task<OutdatedCheckResult> GetOutdatedAsync(
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
            var diagnostics = TryParseProblems(result.StandardOutput) ?? result.GetDiagnosticLines();
            return new OutdatedCheckResult(projectPath, false, [], diagnostics);
        }

        return new OutdatedCheckResult(projectPath, true, ParseOutdatedJson(result.StandardOutput), []);
    }

    /// <summary>
    /// On failure, `dotnet list package --outdated --format json` emits a structured
    /// {"problems": [{"text": "...", "level": "error"}]} report instead of the normal project
    /// report. Parsing it directly gives a much more useful message than grepping raw output
    /// lines for the word "error" (which can just as easily match a `"level": "error"` field
    /// as the actual explanation next to it).
    /// </summary>
    private static IReadOnlyList<string>? TryParseProblems(string json)
    {
        try
        {
            var report = JsonSerializer.Deserialize<ProblemsReport>(json, JsonOptions);
            if (report?.Problems is { Count: > 0 } problems)
            {
                return problems.Select(p => $"[{p.Level}] {p.Text}").ToList();
            }
        }
        catch (JsonException)
        {
            // Not the expected problems-report shape; fall back to raw diagnostics.
        }

        return null;
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

    private sealed record ProblemsReport(
        [property: JsonPropertyName("problems")] List<Problem>? Problems);

    private sealed record Problem(
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("level")] string Level);
}

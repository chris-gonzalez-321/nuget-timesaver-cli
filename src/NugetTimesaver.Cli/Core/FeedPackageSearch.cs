using System.Text.Json;
using System.Text.Json.Serialization;
using NuGet.Versioning;

namespace NugetTimesaver.Cli.Core;

/// <summary>
/// Looks up a package's available versions directly from a feed via `dotnet package search`.
/// Unlike `dotnet list package --outdated`, this needs no project restore at all — it's the
/// fallback used to find update targets for a project whose restore is currently broken.
/// </summary>
public static class FeedPackageSearch
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>All versions of <paramref name="packageId"/> found on the feed, newest first.
    /// Empty (never throws) if the search itself fails or the package isn't on that feed.</summary>
    public static async Task<IReadOnlyList<NuGetVersion>> GetVersionsAsync(
        string packageId, string feedUrl, bool includePrerelease)
    {
        var args = new List<string>
        {
            "package", "search", packageId,
            "--exact-match", "--source", feedUrl, "--format", "json",
        };

        if (includePrerelease)
        {
            args.Add("--prerelease");
        }

        var result = await DotnetCli.RunAsync(args.ToArray());
        if (!result.Succeeded)
        {
            return [];
        }

        SearchReport? report;
        try
        {
            report = JsonSerializer.Deserialize<SearchReport>(result.StandardOutput, JsonOptions);
        }
        catch (JsonException)
        {
            return [];
        }

        return (report?.SearchResult ?? [])
            .SelectMany(source => source.Packages ?? [])
            .Select(p => NuGetVersion.TryParse(p.Version, out var version) ? version : null)
            .Where(v => v is not null)
            .Select(v => v!)
            .Distinct()
            .OrderByDescending(v => v)
            .ToList();
    }

    public static async Task<NuGetVersion?> GetLatestVersionAsync(
        string packageId, string feedUrl, bool includePrerelease)
    {
        var versions = await GetVersionsAsync(packageId, feedUrl, includePrerelease);
        return versions.Count > 0 ? versions[0] : null;
    }

    private sealed record SearchReport(
        [property: JsonPropertyName("searchResult")] List<SearchResultSource>? SearchResult);

    private sealed record SearchResultSource(
        [property: JsonPropertyName("packages")] List<SearchResultPackage>? Packages);

    private sealed record SearchResultPackage(
        [property: JsonPropertyName("version")] string Version);
}

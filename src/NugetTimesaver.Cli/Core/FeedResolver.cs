using System.Text.RegularExpressions;
using NugetTimesaver.Cli.Core.Models;

namespace NugetTimesaver.Cli.Core;

/// <summary>Resolves --feed names to source URLs via `dotnet nuget list source`.</summary>
public static partial class FeedResolver
{
    public const string DefaultFeedUrl = "https://api.nuget.org/v3/index.json";
    public const string DefaultFeedName = "nuget.org";

    [GeneratedRegex(@"^\s*\d+\.\s+(?<name>.+?)\s+\[(?<status>Enabled|Disabled)\]\s*$")]
    private static partial Regex SourceHeaderRegex();

    /// <summary>Parses the text output of `dotnet nuget list source`.</summary>
    public static IReadOnlyList<NugetSource> ParseListSourcesOutput(string output)
    {
        var sources = new List<NugetSource>();
        var lines = output.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();

        for (var i = 0; i < lines.Length; i++)
        {
            var headerMatch = SourceHeaderRegex().Match(lines[i]);
            if (!headerMatch.Success)
            {
                continue;
            }

            var name = headerMatch.Groups["name"].Value.Trim();
            var isEnabled = headerMatch.Groups["status"].Value == "Enabled";
            var url = i + 1 < lines.Length ? lines[i + 1].Trim() : string.Empty;

            sources.Add(new NugetSource(name, url, isEnabled));
        }

        return sources;
    }

    public static async Task<IReadOnlyList<NugetSource>> ListSourcesAsync()
    {
        var result = await DotnetCli.RunAsync("nuget", "list", "source");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"`dotnet nuget list source` failed:\n{result.StandardError}");
        }

        return ParseListSourcesOutput(result.StandardOutput);
    }

    /// <summary>Resolves a --feed name to its URL, or the nuget.org default when none is given.</summary>
    public static async Task<string> ResolveFeedUrlAsync(string? feedName)
    {
        if (string.IsNullOrWhiteSpace(feedName))
        {
            return DefaultFeedUrl;
        }

        var sources = await ListSourcesAsync();
        var match = sources.FirstOrDefault(s => string.Equals(s.Name, feedName, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            throw new InvalidOperationException(
                $"No configured NuGet feed named '{feedName}'. Run `nuget-t feeds view` to see configured feeds.");
        }

        return match.Url;
    }
}

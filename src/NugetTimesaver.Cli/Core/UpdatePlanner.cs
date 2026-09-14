using NugetTimesaver.Cli.Core.Models;

namespace NugetTimesaver.Cli.Core;

/// <summary>Shared discovery -> outdated-check -> filter pipeline used by both view-updates and update.</summary>
public static class UpdatePlanner
{
    public static async Task<IReadOnlyList<PackageUpdate>> BuildPlanAsync(
        string folder, string? feedName, string? packageWildcard, bool includePrerelease)
    {
        var feedUrl = await FeedResolver.ResolveFeedUrlAsync(feedName);
        var projects = ProjectDiscovery.FindProjects(folder);

        var plan = new List<PackageUpdate>();
        foreach (var project in projects)
        {
            var outdated = await OutdatedPackagesReader.GetOutdatedAsync(project, feedUrl, includePrerelease);
            plan.AddRange(outdated.Where(u => PackageWildcardMatcher.IsMatch(u.PackageId, packageWildcard)));
        }

        return plan;
    }
}

using NugetTimesaver.Cli.Core.Models;

namespace NugetTimesaver.Cli.Core;

/// <summary>Shared discovery -> outdated-check -> filter pipeline used by both view-updates and update.</summary>
public static class UpdatePlanner
{
    public static async Task<UpdatePlanResult> BuildPlanAsync(
        string folder, string? feedName, string? packageWildcard, bool includePrerelease)
    {
        var feedUrl = await FeedResolver.ResolveFeedUrlAsync(feedName);
        var projects = ProjectDiscovery.FindProjects(folder);

        var updates = new List<PackageUpdate>();
        var failures = new List<ProjectCheckFailure>();

        foreach (var project in projects)
        {
            var check = await OutdatedPackagesReader.GetOutdatedAsync(project, feedUrl, includePrerelease);

            if (!check.Succeeded)
            {
                failures.Add(new ProjectCheckFailure(project, check.DiagnosticLines));
                continue;
            }

            updates.AddRange(check.Updates.Where(u => PackageWildcardMatcher.IsMatch(u.PackageId, packageWildcard)));
        }

        return new UpdatePlanResult(updates, failures);
    }
}

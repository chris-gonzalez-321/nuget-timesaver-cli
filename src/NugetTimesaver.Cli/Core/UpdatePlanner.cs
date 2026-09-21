using NuGet.Versioning;
using NugetTimesaver.Cli.Core.Models;

namespace NugetTimesaver.Cli.Core;

/// <summary>Shared discovery -> outdated-check -> filter pipeline used by both view-updates and update.</summary>
public static class UpdatePlanner
{
    public static async Task<UpdatePlanResult> BuildPlanAsync(
        string folder, string? feedName, string? packageWildcard, bool includePrerelease, bool force = false)
    {
        var feedUrl = await FeedResolver.ResolveFeedUrlAsync(feedName);
        var projects = ProjectDiscovery.FindProjects(folder);

        var updates = new List<PackageUpdate>();
        var failures = new List<ProjectCheckFailure>();

        foreach (var project in projects)
        {
            var check = await OutdatedPackagesReader.GetOutdatedAsync(project, feedUrl, includePrerelease);

            if (check.Succeeded)
            {
                updates.AddRange(check.Updates.Where(u => PackageWildcardMatcher.IsMatch(u.PackageId, packageWildcard)));
                continue;
            }

            if (!force)
            {
                failures.Add(new ProjectCheckFailure(project, check.DiagnosticLines));
                continue;
            }

            try
            {
                updates.AddRange(await BuildForcedUpdatesAsync(project, feedUrl, packageWildcard, includePrerelease));
            }
            catch (Exception ex)
            {
                failures.Add(new ProjectCheckFailure(project, [$"--force fallback also failed: {ex.Message}"]));
            }
        }

        return new UpdatePlanResult(updates, failures);
    }

    /// <summary>
    /// Used when a project's restore-based check fails (e.g. a version conflict that itself
    /// needs a package update to resolve). Reads current versions straight from the .csproj
    /// XML and looks up latest versions directly against the feed — neither step needs restore
    /// to succeed.
    /// </summary>
    private static async Task<IReadOnlyList<PackageUpdate>> BuildForcedUpdatesAsync(
        string projectPath, string feedUrl, string? packageWildcard, bool includePrerelease)
    {
        var references = ProjectFileReader.ReadPackageReferences(projectPath)
            .Where(r => PackageWildcardMatcher.IsMatch(r.PackageId, packageWildcard));

        var updates = new List<PackageUpdate>();

        foreach (var reference in references)
        {
            if (!NuGetVersion.TryParse(reference.Version, out var currentVersion))
            {
                continue;
            }

            var latestVersion = await FeedPackageSearch.GetLatestVersionAsync(reference.PackageId, feedUrl, includePrerelease);
            if (latestVersion is null || latestVersion <= currentVersion)
            {
                continue;
            }

            updates.Add(new PackageUpdate(
                projectPath, reference.PackageId, reference.Version, latestVersion.ToString(), IsForced: true));
        }

        return updates;
    }
}

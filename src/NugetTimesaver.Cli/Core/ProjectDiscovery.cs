namespace NugetTimesaver.Cli.Core;

public static class ProjectDiscovery
{
    /// <summary>Recursively finds all .csproj files under the given folder, sorted for stable output.</summary>
    public static IReadOnlyList<string> FindProjects(string folder)
    {
        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException($"Folder not found: {folder}");
        }

        return Directory.EnumerateFiles(folder, "*.csproj", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

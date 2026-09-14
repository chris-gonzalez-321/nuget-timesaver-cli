using NugetTimesaver.Cli.Core;
using Xunit;

namespace NugetTimesaver.Cli.Tests;

public class ProjectDiscoveryTests : IDisposable
{
    private readonly string _root;

    public ProjectDiscoveryTests()
    {
        _root = Directory.CreateTempSubdirectory("nuget-t-tests-").FullName;
    }

    public void Dispose()
    {
        Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void FindProjects_FindsCsprojFilesRecursively()
    {
        var nested = Directory.CreateDirectory(Path.Combine(_root, "nested"));
        File.WriteAllText(Path.Combine(_root, "Top.csproj"), "<Project />");
        File.WriteAllText(Path.Combine(nested.FullName, "Nested.csproj"), "<Project />");
        File.WriteAllText(Path.Combine(_root, "not-a-project.txt"), "ignore me");

        var found = ProjectDiscovery.FindProjects(_root);

        Assert.Equal(2, found.Count);
        Assert.Contains(found, p => p.EndsWith("Top.csproj", StringComparison.Ordinal));
        Assert.Contains(found, p => p.EndsWith("Nested.csproj", StringComparison.Ordinal));
    }

    [Fact]
    public void FindProjects_MissingFolder_Throws()
    {
        var missing = Path.Combine(_root, "does-not-exist");

        Assert.Throws<DirectoryNotFoundException>(() => ProjectDiscovery.FindProjects(missing));
    }
}

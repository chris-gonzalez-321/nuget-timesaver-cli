using NugetTimesaver.Cli.Core;
using Xunit;

namespace NugetTimesaver.Cli.Tests;

/// <summary>
/// Regression coverage for the --force fallback: a project whose restore is broken (here, by an
/// unrelated unresolvable package reference) should still surface an update for a *different*,
/// genuinely outdated package in the same project — mirroring the real scenario where a project
/// pinned to stale prerelease versions fails restore entirely, hiding every package in it from
/// the normal `dotnet list package --outdated` path.
/// </summary>
public class UpdatePlannerForceIntegrationTests : IDisposable
{
    private readonly string _projectDir;

    public UpdatePlannerForceIntegrationTests()
    {
        _projectDir = Directory.CreateTempSubdirectory("nuget-t-force-tests-").FullName;

        File.WriteAllText(Path.Combine(_projectDir, "Broken.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" Version="12.0.1" />
                <PackageReference Include="This.Package.Does.Not.Exist.Xyz123" Version="1.0.0" />
              </ItemGroup>
            </Project>
            """);
    }

    public void Dispose()
    {
        Directory.Delete(_projectDir, recursive: true);
    }

    [Fact]
    public async Task BuildPlanAsync_WithoutForce_ReportsProjectAsFailure()
    {
        var plan = await UpdatePlanner.BuildPlanAsync(_projectDir, feedName: null, packageWildcard: null, includePrerelease: false, force: false);

        Assert.Empty(plan.Updates);
        Assert.Single(plan.Failures);
    }

    [Fact]
    public async Task BuildPlanAsync_WithForce_FindsTheOutdatedPackageAnyway()
    {
        var plan = await UpdatePlanner.BuildPlanAsync(_projectDir, feedName: null, packageWildcard: null, includePrerelease: false, force: true);

        Assert.Empty(plan.Failures);
        var update = Assert.Single(plan.Updates);
        Assert.Equal("Newtonsoft.Json", update.PackageId);
        Assert.Equal("12.0.1", update.CurrentVersion);
        Assert.True(update.IsForced);
    }
}

namespace NugetTimesaver.Cli.Core.Models;

public sealed record PackageUpdate(
    string ProjectPath,
    string PackageId,
    string CurrentVersion,
    string LatestVersion);

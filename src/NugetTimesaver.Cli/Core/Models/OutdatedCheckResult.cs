namespace NugetTimesaver.Cli.Core.Models;

/// <summary>Result of checking one project for outdated packages — a failure here (e.g. a
/// pre-existing restore error in that project) shouldn't stop the rest of the projects from
/// being checked.</summary>
public sealed record OutdatedCheckResult(
    string ProjectPath,
    bool Succeeded,
    IReadOnlyList<PackageUpdate> Updates,
    IReadOnlyList<string> DiagnosticLines);

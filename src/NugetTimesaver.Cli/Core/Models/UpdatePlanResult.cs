namespace NugetTimesaver.Cli.Core.Models;

public sealed record ProjectCheckFailure(string ProjectPath, IReadOnlyList<string> DiagnosticLines);

public sealed record UpdatePlanResult(
    IReadOnlyList<PackageUpdate> Updates,
    IReadOnlyList<ProjectCheckFailure> Failures);

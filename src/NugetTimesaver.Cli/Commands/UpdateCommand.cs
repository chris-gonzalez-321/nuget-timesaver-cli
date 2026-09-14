using NugetTimesaver.Cli.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NugetTimesaver.Cli.Commands;

public sealed class UpdateCommand : AsyncCommand<UpdateSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, UpdateSettings settings, CancellationToken cancellationToken)
    {
        var folder = settings.Folder ?? Directory.GetCurrentDirectory();

        try
        {
            var plan = await UpdatePlanner.BuildPlanAsync(folder, settings.Feed, settings.PackageWildcard, settings.AllowPrerelease);
            PlanTableRenderer.Render(plan);

            if (plan.Count == 0)
            {
                return 0;
            }

            if (!settings.Apply)
            {
                AnsiConsole.MarkupLine("\n[yellow]Dry run — pass --apply to write these changes.[/]");
                return 0;
            }

            var feedUrl = await FeedResolver.ResolveFeedUrlAsync(settings.Feed);
            var succeeded = 0;
            var failed = 0;

            foreach (var update in plan)
            {
                // --version already pins the exact resolved version, prerelease or not;
                // `dotnet add package` rejects --prerelease alongside --version.
                var result = await DotnetCli.RunAsync(
                    "add", update.ProjectPath, "package", update.PackageId,
                    "--version", update.LatestVersion,
                    "--source", feedUrl);
                var projectName = Path.GetFileNameWithoutExtension(update.ProjectPath);

                if (result.Succeeded)
                {
                    succeeded++;
                    AnsiConsole.MarkupLineInterpolated(
                        $"[green]Updated[/] {update.PackageId} in {projectName} -> {update.LatestVersion}");
                }
                else
                {
                    failed++;
                    AnsiConsole.MarkupLineInterpolated(
                        $"[red]Failed[/] {update.PackageId} in {projectName} (exit code {result.ExitCode}):");

                    foreach (var line in result.GetDiagnosticLines())
                    {
                        AnsiConsole.MarkupLineInterpolated($"    {line}");
                    }
                }
            }

            AnsiConsole.MarkupLineInterpolated($"\n{succeeded} updated, {failed} failed.");
            return failed == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}

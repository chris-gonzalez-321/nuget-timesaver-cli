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
            var plan = await UpdatePlanner.BuildPlanAsync(folder, settings.Feed, settings.PackageWildcard, settings.AllowPrerelease, settings.Force);
            PlanTableRenderer.Render(plan);

            if (plan.Updates.Count == 0)
            {
                return plan.Failures.Count == 0 ? 0 : 1;
            }

            if (!settings.Apply)
            {
                AnsiConsole.MarkupLine("\n[yellow]Dry run — pass --apply to write these changes.[/]");
                return plan.Failures.Count == 0 ? 0 : 1;
            }

            var feedUrl = await FeedResolver.ResolveFeedUrlAsync(settings.Feed);
            var succeeded = 0;
            var forcedSucceeded = 0;
            var failed = plan.Failures.Count;

            foreach (var update in plan.Updates)
            {
                // --version already pins the exact resolved version, prerelease or not;
                // `dotnet add package` rejects --prerelease alongside --version.
                var args = new List<string>
                {
                    "add", update.ProjectPath, "package", update.PackageId,
                    "--version", update.LatestVersion,
                    "--source", feedUrl,
                };

                // Forced updates come from projects whose restore is currently broken, so the
                // normal restore-preview/compatibility check `dotnet add package` would do is
                // guaranteed to fail too — skip it and write the version unconditionally.
                if (update.IsForced)
                {
                    args.Add("--no-restore");
                }

                var result = await DotnetCli.RunAsync(args.ToArray());
                var projectName = Path.GetFileNameWithoutExtension(update.ProjectPath);

                if (result.Succeeded)
                {
                    succeeded++;
                    if (update.IsForced)
                    {
                        forcedSucceeded++;
                    }

                    var marker = update.IsForced ? " [yellow](forced)[/]" : "";
                    AnsiConsole.MarkupLineInterpolated(
                        $"[green]Updated[/] {update.PackageId} in {projectName} -> {update.LatestVersion}{marker}");
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

            if (forcedSucceeded > 0)
            {
                AnsiConsole.MarkupLine(
                    "[yellow]Forced updates skip the normal compatibility check — run `dotnet restore` "
                    + "on the affected project(s) to confirm everything actually resolves.[/]");
            }

            return failed == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}

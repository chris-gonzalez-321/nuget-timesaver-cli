using NugetTimesaver.Cli.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NugetTimesaver.Cli.Commands;

public sealed class ViewUpdatesCommand : AsyncCommand<UpdatesSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, UpdatesSettings settings, CancellationToken cancellationToken)
    {
        var folder = settings.Folder ?? Directory.GetCurrentDirectory();

        try
        {
            var plan = await UpdatePlanner.BuildPlanAsync(folder, settings.Feed, settings.PackageWildcard, settings.AllowPrerelease, settings.Force);
            PlanTableRenderer.Render(plan);
            return plan.Failures.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}

using NugetTimesaver.Cli.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NugetTimesaver.Cli.Commands.Feeds;

public sealed class FeedsAddSettings : CommandSettings
{
    [CommandArgument(0, "<NAME>")]
    public required string Name { get; init; }

    [CommandArgument(1, "<URL>")]
    public required string Url { get; init; }

    [CommandOption("--username <USERNAME>")]
    public string? Username { get; init; }

    [CommandOption("--password <PASSWORD>")]
    public string? Password { get; init; }
}

public sealed class FeedsAddCommand : AsyncCommand<FeedsAddSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, FeedsAddSettings settings, CancellationToken cancellationToken)
    {
        var args = new List<string> { "nuget", "add", "source", settings.Url, "--name", settings.Name };

        if (!string.IsNullOrWhiteSpace(settings.Username))
        {
            args.AddRange(["--username", settings.Username]);
        }

        if (!string.IsNullOrWhiteSpace(settings.Password))
        {
            args.AddRange(["--password", settings.Password, "--store-password-in-clear-text"]);
        }

        var result = await DotnetCli.RunAsync(args.ToArray());

        if (!result.Succeeded)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Failed to add feed:[/] {result.StandardError.Trim()}");
            return 1;
        }

        AnsiConsole.MarkupLineInterpolated($"[green]Added feed[/] '{settings.Name}' -> {settings.Url}");
        return 0;
    }
}

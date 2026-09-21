# nuget-t — NuGet Timesaver CLI

A small CLI for managing NuGet packages across many C# projects at once. The
stock `dotnet`/`nuget` CLI only operates on one project at a time and has no
wildcard filtering — `nuget-t` wraps it to add:

- **Feed management** — view, add, and remove configured NuGet sources.
- **Bulk update checks** — see what's outdated across every project in a
  folder (recursively), optionally scoped to one feed and/or a package
  name wildcard.
- **Bulk updates** — apply those updates across every matching project at
  once, with a safe dry-run by default.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later (`dotnet --version`).

## Install

`nuget-t` isn't published to a feed yet, so install it by building from source:

```bash
git clone https://github.com/chris-gonzalez-321/nuget-timesaver-cli.git
cd nuget-timesaver-cli
dotnet pack src/NugetTimesaver.Cli -c Release -o ./nupkg
dotnet tool install --global --add-source ./nupkg NugetTimesaver.Cli
```

This puts `nuget-t` on your PATH as a .NET global tool. Confirm it worked:

```bash
nuget-t --help
```

### Updating to a newer version

NuGet package versions are immutable, so pulling new code alone isn't enough —
the version in [`NugetTimesaver.Cli.csproj`](src/NugetTimesaver.Cli/NugetTimesaver.Cli.csproj)
must be bumped before repacking, then:

```bash
git pull
# bump <Version> in src/NugetTimesaver.Cli/NugetTimesaver.Cli.csproj
dotnet pack src/NugetTimesaver.Cli -c Release -o ./nupkg
dotnet tool update --global --add-source ./nupkg NugetTimesaver.Cli
```

### Uninstalling

```bash
dotnet tool uninstall --global NugetTimesaver.Cli
```

## Usage

### Feeds

```bash
nuget-t feeds view                                  # list configured NuGet sources
nuget-t feeds add <name> <url> [--username --password]
nuget-t feeds remove <name>
```

### View available updates

```bash
nuget-t view-updates [--feed <name>] [--fldr <path>] [--pkg-wc <wildcard>] [--pre-r]
```

- `--feed` — name of a configured source to check for newer versions (as
  shown by `nuget-t feeds view`). Defaults to `nuget.org` if omitted.
- `--fldr` — folder to search recursively for `.csproj` files. Defaults to
  the current directory.
- `--pkg-wc` — filter by package id. `"IntegrationBroker.Domain"` matches
  only that exact package; `"IntegrationBroker*"` matches every package
  starting with that prefix. Omit to check every package.
- `--pre-r` — include prerelease versions when checking for updates.
- `--force` — see [Handling a project whose restore is already broken](#handling-a-project-whose-restore-is-already-broken) below.

If a project's own restore fails (independent of `nuget-t`), it's listed
separately as unable to be checked instead of aborting the whole run — every
other project still gets reported normally.

### Update packages

Same options as `view-updates`, plus `--apply`:

```bash
nuget-t update [--feed <name>] [--fldr <path>] [--pkg-wc <wildcard>] [--pre-r] [--force] [--apply]
```

Without `--apply`, `update` only prints the plan (identical to
`view-updates`) and changes nothing. Pass `--apply` to actually write the
version bumps to each project.

**Example** — bump every `IntegrationBroker*` package to the latest
prerelease build from a private feed, across every project under the current
folder:

```bash
nuget-t update --feed OTP_PreRelease --pkg-wc "IntegrationBroker*" --pre-r --apply
```

### Handling a project whose restore is already broken

`view-updates`/`update` normally rely on `dotnet list package --outdated`,
which needs a project's restore to succeed. If a project's restore is
*already* failing — for example a direct `PackageReference` pinned to a
version too old for what the rest of the dependency graph now needs
(`NU1605`, a package downgrade) — there's nothing for the normal check to
report, even though bumping that exact package is the fix.

`--force` works around this for any project whose restore fails: it reads
current versions straight from the `.csproj` XML (no restore needed) and
looks up the latest version of each directly against the feed via
`dotnet package search` (also no restore needed), then applies with
`--no-restore`. Forced updates are marked `forced` in the output.

This skips the compatibility check `dotnet add package` would normally do,
so **run `dotnet restore` on the affected project afterward** to confirm it
actually resolves — the tool prints a reminder when this happens. Projects
that already restore fine are never affected by `--force`; it only kicks in
as a fallback for ones that were already broken.

```bash
nuget-t update --feed OTP_Release --fldr ProductBroker --pkg-wc "IntegrationBroker.*" --pre-r --force --apply
```

## Development

```bash
dotnet build
dotnet test
dotnet run --project src/NugetTimesaver.Cli -- <command> [options]   # run without installing
```

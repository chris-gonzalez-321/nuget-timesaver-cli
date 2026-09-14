using System.Text.RegularExpressions;

namespace NugetTimesaver.Cli.Core;

/// <summary>
/// Matches package IDs against a "pkg-wc" filter:
///   - null/empty  -> matches everything
///   - no '*'      -> exact, case-insensitive match ("IntegrationBroker.Domain")
///   - contains '*' -> glob match, case-insensitive ("IntegrationBroker*")
/// </summary>
public static class PackageWildcardMatcher
{
    public static bool IsMatch(string packageId, string? wildcard)
    {
        if (string.IsNullOrWhiteSpace(wildcard))
        {
            return true;
        }

        if (!wildcard.Contains('*'))
        {
            return string.Equals(packageId, wildcard, StringComparison.OrdinalIgnoreCase);
        }

        var regexPattern = "^" + string.Join(".*", wildcard.Split('*').Select(Regex.Escape)) + "$";

        return Regex.IsMatch(packageId, regexPattern, RegexOptions.IgnoreCase);
    }
}

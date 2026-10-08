using System.Text.RegularExpressions;

namespace CadBridge.AutoCAD.Reading;

internal static class TagTextParser
{
    private static readonly Regex TagValuePair = new(
        @"tag:\s*(?<number>\S+)\s+value:\s*(?<name>\S+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static (string Number, string Name) Parse(IReadOnlyList<(string Tag, string Value)> attributes)
    {
        foreach (var (_, value) in attributes)
        {
            var match = TagValuePair.Match(value);
            if (match.Success)
                return (match.Groups["number"].Value, match.Groups["name"].Value);
        }

        var number = Find(attributes, "TAG", "USAGE_TYPE", "NUMBER");
        var name = Find(attributes, "VALUE", "NAME");
        if (number is not null || name is not null)
            return (number ?? string.Empty, name ?? string.Empty);

        return attributes.Count switch
        {
            0 => (string.Empty, string.Empty),
            1 => (attributes[0].Value.Trim(), string.Empty),
            _ => (attributes[0].Value.Trim(), attributes[1].Value.Trim()),
        };
    }

    private static string? Find(IReadOnlyList<(string Tag, string Value)> attributes, params string[] names)
    {
        foreach (var (tag, value) in attributes)
        {
            foreach (var name in names)
            {
                if (string.Equals(tag, name, StringComparison.OrdinalIgnoreCase))
                    return value.Trim();
            }
        }

        return null;
    }
}


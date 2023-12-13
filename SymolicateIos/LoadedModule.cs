using System.Diagnostics;
using System.Text.RegularExpressions;

[DebuggerDisplay("{Module} {MemoryAddressStart} {ModuleId}")]
public partial struct LoadedModule
{
    public Guid ModuleId { get; private init; }
    public string Module { get; private init; }
    public long MemoryAddressStart { get; private init; }
    public long MemoryAddressEnd { get; private init; }
    public string Architecture { get; private init; }
    public string Path { get; private init; }

    public static LoadedModule? TryParse(string line)
    {
        // Parses lines from a crashlog.crash file
        //
        // For example:
        // 0x102864000 - 0x108b17fff Naturoppdrag arm64 <d2ae8a66d5643a7e9bb182b11962110d> /private/var/containers/Bundle/Application/A174A17B-78EC-4CD7-BD83-5E91C9C146AD/Naturoppdrag.app/Naturoppdrag
        // 0x114100000 - 0x114647fff libSkiaSharp arm64 <e731a267097031bda384c26188c796ca> /private/var/containers/Bundle/Application/A174A17B-78EC-4CD7-BD83-5E91C9C146AD/Naturoppdrag.app/Frameworks/libSkiaSharp.framework/libSkiaSharp
        //
        // The token consists of one byte for the type (only methods, 0x06, is
        // supported for now) and three bytes for the token value.

        var match = LineRegex().Match(line);

        if (match is { Success: true, Groups.Count: 7 })
        {
            var offsetStart = Convert.ToInt64(match.Groups[1].Value, 16);
            var offsetEnd = Convert.ToInt64(match.Groups[2].Value, 16);
            var module = match.Groups[3].Value;
            var architecture = match.Groups[4].Value;
            var moduleId = Guid.Parse(match.Groups[5].Value);
            var path = match.Groups[6].Value;

            return new LoadedModule
            {
                ModuleId = moduleId,
                Module = module,
                MemoryAddressStart = offsetStart,
                MemoryAddressEnd = offsetEnd,
                Architecture = architecture,
                Path = path
            };
        }

        return null;
    }

    [GeneratedRegex(@"\s*(0x[0-9A-Fa-f]+)\s+-\s+(0x[0-9A-Fa-f]+)\s+(\S+)\s+(\S+)\s+<(\S+)>\s+(\S+)")]
    private static partial Regex LineRegex();
}
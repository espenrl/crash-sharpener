using System.Diagnostics;
using System.Text.RegularExpressions;

[DebuggerDisplay("{Module} {Offset} {Symbol}")]
public partial struct StackFrame
{
    public string Module { get; private init; }
    public string Symbol { get; private init; }
    public long Offset { get; private init; }

    public static StackFrame? FromString(string line)
    {
        // Parses lines from a stack trace that has been decorated with IL offsets and
        // method tokens.
        //
        // For example:
        // 0   Naturoppdrag   0x208b96724         <unknown> + 204
        // 1   Naturoppdrag   0x208b5e1f4[inlined] < unknown > +356
        //
        // The token consists of one byte for the type (only methods, 0x06, is
        // supported for now) and three bytes for the token value.

        var match = LineRegex().Match(line);

        if (match is { Success: true, Groups.Count: 6 })
        {
            var module = match.Groups[2].Value;
            var offset = Convert.ToInt64(match.Groups[3].Value, 16);
            var symbol = match.Groups[4].Value;
            var offset2 = Convert.ToInt64(match.Groups[5].Value, 10);

            return new StackFrame
            {
                Module = module,
                Offset = offset - offset2,
                Symbol = symbol
            };
        }

        return null;
    }

    [GeneratedRegex(@"(\d+)\s+(\S+)\s+(0x[0-9A-Fa-f]+)\s+(\S+)\s+\+\s+(\d+)")]
    private static partial Regex LineRegex();
}
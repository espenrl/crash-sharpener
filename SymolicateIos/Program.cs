using System.Text;
using ELFSharp.MachO;

var path = @"C:\Users\erl0\Downloads\naturoppdrag-ci-build-ios-2.0.36\Naturoppdrag";
var path2 = @"C:\Users\erl0\Downloads\stacktrace.txt";
var outputPath = @"C:\Users\erl0\Downloads\stacktrace.fixed.txt";

var loadAddress = 0x000102be0000;

var machOReader = MachOReader.Load(path);

var dylib = machOReader
    .GetCommandsOfType<Dylib>()
    .ToList();

var uuid = machOReader
    .GetCommandsOfType<UUID>()
    .ToList();

var entryPoint = machOReader
    .GetCommandsOfType<EntryPoint>()
    .ToList();

var segment = machOReader
    .GetCommandsOfType<Segment>()
    .ToList();

var symbols2 = machOReader
    .GetCommandsOfType<SymbolTable>()
    .Single()
    .Symbols;

var map = symbols2
    .GroupBy(x => x.Value)
    .ToDictionary(x => x.Key);

var a = 0x208b96724 - loadAddress;
if (map.TryGetValue(a, out var symbols22))
{
}

// debug_meta -> images -> debug_id, code_file, image_addr

var ass = symbols2.Where(x => string.Equals("_sqlite3VdbeParameterIndex", x.Name, StringComparison.Ordinal)).ToList();


var stringBuilder = new StringBuilder();

foreach (var line in File.ReadLines(path2))
{
    if (StackFrame.FromString(line) is { } stackFrame && string.Equals("Naturoppdrag", stackFrame.Module, StringComparison.Ordinal))
    {
        // TODO: handle multiple symbols with the same offset
        string outputLine;
        if (map.TryGetValue(stackFrame.Offset - loadAddress, out var symbols))
            outputLine = line.Replace(stackFrame.Symbol, symbols.First().Name);
        else
            outputLine = line;

        stringBuilder.AppendLine(outputLine);
    }
    else
    {
        stringBuilder.AppendLine(line);
    }
}

File.WriteAllText(outputPath, stringBuilder.ToString());

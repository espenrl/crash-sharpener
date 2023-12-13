using System.Collections.Frozen;
using System.Text;
using ELFSharp.MachO;

var path = @"C:\Users\erl0\Downloads\naturoppdrag-ci-build-ios-2.0.48\Naturoppdrag";
var path2 = @"C:\Users\erl0\Downloads\stacktrace.txt";
var outputPath = @"C:\Users\erl0\Downloads\stacktrace.fixed.txt";

// from Sentry json -> debug_meta -> images -> debug_id, code_file, image_addr
var moduleLoadAddress = 0x000100b00000;

var modules = File.ReadAllLines(path2)
    .Select(LoadedModule.TryParse)
    .Where(x => x is not null)
    .ToFrozenDictionary(
        static x => x!.Value.ModuleId,
        static x => x!.Value);

var machOReader = MachOReader.Load(path);

var uuidList = machOReader
    .GetCommandsOfType<UUID>()
    .ToList();

// get modules address from Apple crash report
if (uuidList.FirstOrDefault() is { } uuid && modules.TryGetValue(uuid.ID, out var module))
{
    moduleLoadAddress = module.MemoryAddressStart;
}

var dylib = machOReader
    .GetCommandsOfType<Dylib>()
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

var symbolsMap = symbols2
    .GroupBy(x => x.Value)
    .ToDictionary(x => x.Key);

var stringBuilder = new StringBuilder();

foreach (var line in File.ReadLines(path2))
{
    if (StackFrame.TryParse(line) is { } stackFrame && string.Equals("Naturoppdrag", stackFrame.Module, StringComparison.Ordinal))
    {
        // TODO: handle multiple symbols with the same offset
        string outputLine;
        var moduleOffset = stackFrame.FunctionAddress - moduleLoadAddress;
        if (symbolsMap.TryGetValue(moduleOffset, out var symbols))
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

using DatafileGenerator.Data;
using DatafileGenerator.Data.Models;
using DatafileGenerator.Game;
using DatafileGenerator.Source.Data.Models;
using ServiceStack;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CsvSerializer = ServiceStack.Text.CsvSerializer;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DatafileGenerator;

public static class Program
{
    private const int MightOfTheVaal = 76;
    private const int LegacyOfTheVaal = 77;
    private const int MaxBytesInFile = 5242880; //5MB
    private const int ExpectedArgumentCount = 5;
    private const int MaxJewelType = 11;
    private static int NumAdditions;
    private static bool IsInteractiveMode = true;
    private static IReadOnlyDictionary<uint, AlternateTreeVersion> AlternateTreeVersionsByIndex;
    private static IReadOnlyDictionary<int, IReadOnlyDictionary<uint, byte>> GlobalToLocalIdMappingsByJewelType;
    private static IReadOnlyDictionary<int, IReadOnlyDictionary<byte, uint>> LocalToGlobalIdMappingsByJewelType;
    private const string LuaMappingFileName = "NodeIndexMapping.lua";
    private const string CsvFileName = "node_indices.csv";

    public static void Main(string[] args)
    {
        Console.Title = $"{GeneratorSettings.ApplicationName} (v{GeneratorSettings.ApplicationVersion})";
        AnsiConsole.MarkupLine("Spinning up!");
        IsInteractiveMode = args == null || args.Length == 0;

        if (IsAbyssRangeExportRequested(args))
        {
            RunAbyssRangeExport(args);
            return;
        }
        if (IsAbyssAscendancyInspectionRequested(args))
        {
            RunAbyssAscendancyInspection(args);
            return;
        }
        if (IsAbyssInspectionRequested(args))
        {
            RunAbyssInspection(args);
            return;
        }
        if (IsHelpRequested(args))
        {
            PrintUsage();
            return;
        }

        const string OutputCompressedFiles = "compressed";
        const string OutputUncompressedFiles = "uncompressed";
        const string OutputBothFileFormats = "both";
        const string OutputCsvFiles = "csv";
        const string OutputCsvFilesCompressed = "csv (compressed)";
        var exportOptions = new List<string>()
        {
            OutputCompressedFiles,
            OutputUncompressedFiles,
            OutputBothFileFormats,
            OutputCsvFiles,
            OutputCsvFilesCompressed
        };

        string outputDir;
        string compression;
        if (args != null && args.Length > 0)
        {
            IsInteractiveMode = false;
            if (!TryParseCommandLineArguments(args, out outputDir, out compression, out string argumentError))
            {
                PrintUsage();
                ExitWithError(argumentError);
                return;
            }
            AnsiConsole.MarkupLine("[grey]Using command-line arguments.[/]");
        }
        else
        {
            IsInteractiveMode = true;

            GeneratorSettings.AlternatePassiveAdditionsFilePath = Path.GetFullPath(@"source-data\alternatepassiveadditions.json");
            if (!File.Exists(GeneratorSettings.AlternatePassiveAdditionsFilePath))
            {
                PromptUserForFile("Path to [yellow]alternate passive ADDITIONS[/] file:", out GeneratorSettings.AlternatePassiveAdditionsFilePath);
            }

            GeneratorSettings.AlternatePassiveSkillsFilePath = Path.GetFullPath(@"source-data\alternatepassiveskills.json");
            if (!File.Exists(GeneratorSettings.AlternatePassiveSkillsFilePath))
            {
                PromptUserForFile("Path to [yellow]alternate passive SKILLS[/] file:", out GeneratorSettings.AlternatePassiveSkillsFilePath);
            }

            GeneratorSettings.PassiveSkillsFilePath = Path.GetFullPath(@"source-data\data.json");
            if (!File.Exists(GeneratorSettings.PassiveSkillsFilePath))
            {
                PromptUserForFile("Path to [yellow]skill tree[/] file:", out GeneratorSettings.PassiveSkillsFilePath);
            }

            outputDir = Path.GetFullPath(@"output-data");
            if (!Directory.Exists(outputDir))
            {
                PromptUserForFile("Path to [yellow]output[/] directory:", out outputDir, true);
            }

            PromptUserForChoice("Select output type:", exportOptions, out int compressionChoice);
            compression = exportOptions.ElementAt(compressionChoice);
        }

        var isCsvExport = compression == OutputCsvFiles || compression == OutputCsvFilesCompressed;
        var compressCSVOutput = compression == OutputCsvFilesCompressed;

        AnsiConsole.MarkupLine("[green]Loading[/]...");

        if (!DataManager.Initialize())
            ExitWithError("Failed to initialize the [yellow]data manager[/].");
        AlternateTreeVersionsByIndex = DataManager.AlternateTreeVersions.ToDictionary(q => q.Index);
        NumAdditions = DataManager.AlternatePassiveAdditions.Count;
        InitializeLocalIdMappings();
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        Dictionary<string, int> notableJewelSocketMappings = null;
        if (isCsvExport)
        {
            AnsiConsole.MarkupLine("[green]Calculating Notable skills affected by jewel radii[/]...");
            var calculator = new AffectedNotablesCalculator();
            notableJewelSocketMappings = calculator.GetNotableToSocketMapping();
            AnsiConsole.MarkupLine($"{notableJewelSocketMappings.Count} Affected Notable skills found");
        }

        var justNotables = GetModifiableNodes(true);
        var justSmallNodes = GetModifiableNodes(false);
        justNotables.Sort();
        justSmallNodes.Sort();
        //generate our indices
        var notablesThenSmalls = justNotables.ToList();
        notablesThenSmalls.AddRange(justSmallNodes);

        AnsiConsole.MarkupLine("[green]Processing[/]...");
        //build the csv
        if (File.Exists(CsvFileName))
            File.Delete(CsvFileName);

        var sb = new StringBuilder("PassiveSkillGraphId,Name,Datafile Parsing Index\n");
        for (int k = 0; k < notablesThenSmalls.Count; k++)
        {
            var node = notablesThenSmalls[k];
            sb.AppendLine(node.GraphIdentifier + "," + (node.Name.Contains(',') ? ("\"" + node.Name + "\"") : node.Name) + "," + k);
        }
        File.WriteAllText(Path.Combine(outputDir, CsvFileName), sb.ToString());
        sb.Clear();

        //begin iterating over the 11 jewel types
        //reverse order since glorious vanity sucks
        string outputPath = null;
        for (int i = 11; i > 0; i--)
        {
            var sw = Stopwatch.StartNew();
            GetJewelTypeInfo(i, out _, out _, out _, out string outputFile);
            byte[] dataBuffer;
            List<CsvExportRow> csvExport = new List<CsvExportRow>();

            //glorious vanity logic
            if (i == 1)
            {
                GenerateGloriousVanity(notablesThenSmalls, isCsvExport, notableJewelSocketMappings, out var luaSizes, out dataBuffer, out csvExport);
                sb.Clear();
                sb.AppendLine("nodeIDList = { }");
                sb.AppendLine($"nodeIDList[\"size\"] = {notablesThenSmalls.Count}");
                sb.AppendLine($"nodeIDList[\"sizeNotable\"] = {justNotables.Count}");
                for (int k = 0; k < notablesThenSmalls.Count; k++)
                {
                    var node = notablesThenSmalls[k];
                    sb.AppendLine($"nodeIDList[{node.GraphIdentifier}] = {{ index = {k}, size = {luaSizes[k]} }}");
                }
                AppendLocalIdMappings(sb);
                sb.Append("return nodeIDList");
                File.WriteAllText(Path.Combine(outputDir, LuaMappingFileName), sb.ToString());
                sb.Clear();
            }
            // Abyss Jewel logic (that depends on sockets)
            else if (i >= 7 && i <= 10)
            {
                GetJewelTypeInfo(i, out int jewelMin, out int jewelMax, out int jewelIncrement, out string jewelName);
                AnsiConsole.MarkupLine($"[green]Calculating {jewelName} seeds[/]...");

                if (isCsvExport && compression != OutputBothFileFormats)
                {
                    AnsiConsole.MarkupLine("[yellow]Warning: CSV export is not supported for Abyss Jewels. CSV export will be skipped.[/]");
                    continue;
                }

                IReadOnlyDictionary<uint, byte> localIdMapping = GetGlobalToLocalIdMapping(i);
                dataBuffer = AbyssDataWriter.GenerateSocketDependent(
                    i,
                    jewelMin,
                    jewelMax,
                    jewelIncrement,
                    NumAdditions,
                    globalId => MapGlobalIdToLocalId(localIdMapping, i, globalId));
            }
            // Abyss Jewel logic (that depends on path)
            else if (i == 11)
            {
                GetJewelTypeInfo(i, out int jewelMin, out int jewelMax, out int jewelIncrement, out string jewelName);
                AnsiConsole.MarkupLine($"[green]Calculating {jewelName} seeds[/]...");

                if (isCsvExport && compression != OutputBothFileFormats)
                {
                    AnsiConsole.MarkupLine("[yellow]Warning: CSV export is not supported for Abyss Jewels. CSV export will be skipped.[/]");
                    continue;
                }

                IReadOnlyDictionary<uint, byte> localIdMapping = GetGlobalToLocalIdMapping(i);
                dataBuffer = AbyssDataWriter.GeneratePathDependentChanges(
                    i,
                    jewelMin,
                    jewelMax,
                    jewelIncrement,
                    NumAdditions,
                    globalId => MapGlobalIdToLocalId(localIdMapping, i, globalId));
            }
            //non-glorious vanity logic
            else
            {
                GenerateRegular(justNotables, isCsvExport, i, notableJewelSocketMappings, out dataBuffer, out csvExport);
            }

            //output uncompressed
            if (compression == OutputUncompressedFiles || compression == OutputBothFileFormats)
            {
                AnsiConsole.MarkupLine("[green]Generating uncompressed output file[/]...");
                outputPath = Path.Combine(outputDir, outputFile);
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
                using (Stream file = File.OpenWrite(outputPath))
                {
                    file.Write(dataBuffer, 0, dataBuffer.Length);
                }
            }

            //output compressed
            if (compression == OutputCompressedFiles || compression == OutputBothFileFormats)
            {
                AnsiConsole.MarkupLine("[green]Generating compressed output file[/]...");
                byte[] compressedData = Compress(dataBuffer);
                //need to split into multiple files because PoB is dumb
                if (compressedData.Length > MaxBytesInFile)
                {
                    int splitIndex = 0;
                    byte[] split = compressedData.Take(MaxBytesInFile).ToArray();
                    while (split.Any())
                    {
                        //write the data
                        outputPath = Path.Combine(outputDir, Path.ChangeExtension(outputFile, $"zip.part{splitIndex}"));
                        if (File.Exists(outputPath))
                        {
                            File.Delete(outputPath);
                        }
                        using (Stream file = File.OpenWrite(outputPath))
                        {
                            file.Write(split, 0, split.Length);
                        }
                        split = compressedData.Skip(MaxBytesInFile * ++splitIndex).Take(MaxBytesInFile).ToArray();
                    }
                }
                //file is small enough as is, just write the file
                else
                {
                    outputPath = Path.Combine(outputDir, Path.ChangeExtension(outputFile, "zip"));
                    if (File.Exists(outputPath))
                    {
                        File.Delete(outputPath);
                    }
                    using (Stream file = File.OpenWrite(outputPath))
                    {
                        file.Write(compressedData, 0, compressedData.Length);
                    }
                }
            }

            if (isCsvExport && IsTimelessJewel(i))
            {
                AnsiConsole.MarkupLine("[green]Generating CSV export[/]...");

                outputPath = CreateCSVFile(csvExport, outputDir, outputFile, compressCSVOutput);
            }
            //log completion
            sw.Stop();
            AnsiConsole.MarkupLine($"{outputFile} took {string.Format("{0:0.##}", sw.Elapsed.TotalSeconds)} seconds");
            if (outputPath != null)
            {
                AnsiConsole.MarkupLine($"[blue]File available at:[/] {outputPath}");
            }
        }

        AnsiConsole.MarkupLine("[green]Done[/]!");
    }

    private static bool IsTimelessJewel(int jewelType)
    {
        return jewelType < 7;
    }

    private static List<PassiveSkill> GetModifiableNodes(bool notables)
    {
        return DataManager.PassiveSkills.Where(x => x.IsModifiable && (!notables ^ x.IsNotable)).ToList();
    }

    private static void InitializeLocalIdMappings()
    {
        Dictionary<int, IReadOnlyDictionary<uint, byte>> globalToLocalIdMappingsByJewelType = new Dictionary<int, IReadOnlyDictionary<uint, byte>>(MaxJewelType);
        Dictionary<int, IReadOnlyDictionary<byte, uint>> localToGlobalIdMappingsByJewelType = new Dictionary<int, IReadOnlyDictionary<byte, uint>>(MaxJewelType);

        for (int jewelType = 1; jewelType <= MaxJewelType; jewelType++)
        {
            HashSet<uint> globalIds = new HashSet<uint>();
            uint alternateTreeVersionIndex = (uint)jewelType;

            foreach (AlternatePassiveAddition alternatePassiveAddition in DataManager.AlternatePassiveAdditions)
            {
                if (alternatePassiveAddition.AlternateTreeVersionIndex == alternateTreeVersionIndex)
                    globalIds.Add(alternatePassiveAddition.Index);
            }

            foreach (AlternatePassiveSkill alternatePassiveSkill in DataManager.AlternatePassiveSkills)
            {
                if (alternatePassiveSkill.AlternateTreeVersionIndex == alternateTreeVersionIndex)
                    globalIds.Add((alternatePassiveSkill.Index + (uint)NumAdditions));
            }

            if (globalIds.Count > (byte.MaxValue + 1))
            {
                GetJewelTypeInfo(jewelType, out _, out _, out _, out string jewelName);
                ExitWithError($"Jewel type [yellow]{jewelName}[/] requires [yellow]{globalIds.Count}[/] local ids, which exceeds 1-byte capacity.");
            }

            Dictionary<uint, byte> globalToLocal = new Dictionary<uint, byte>(globalIds.Count);
            Dictionary<byte, uint> localToGlobal = new Dictionary<byte, uint>(globalIds.Count);
            List<uint> overflowGlobalIds = new List<uint>();

            foreach (uint globalId in globalIds.OrderBy(q => q))
            {
                if (globalId <= byte.MaxValue)
                {
                    byte localId = (byte)globalId;
                    if (!localToGlobal.ContainsKey(localId))
                    {
                        globalToLocal.Add(globalId, localId);
                        localToGlobal.Add(localId, globalId);
                        continue;
                    }
                }
                overflowGlobalIds.Add(globalId);
            }

            byte nextAvailableLocalId = 0;
            for (int i = 0; i < overflowGlobalIds.Count; i++)
            {
                while (localToGlobal.ContainsKey(nextAvailableLocalId))
                {
                    if (nextAvailableLocalId == byte.MaxValue)
                    {
                        GetJewelTypeInfo(jewelType, out _, out _, out _, out string jewelName);
                        ExitWithError($"No free 1-byte local ids remain for [yellow]{jewelName}[/].");
                    }
                    nextAvailableLocalId++;
                }

                uint overflowGlobalId = overflowGlobalIds[i];
                globalToLocal.Add(overflowGlobalId, nextAvailableLocalId);
                localToGlobal.Add(nextAvailableLocalId, overflowGlobalId);

                if (nextAvailableLocalId < byte.MaxValue)
                    nextAvailableLocalId++;
            }

            globalToLocalIdMappingsByJewelType.Add(jewelType, globalToLocal);
            localToGlobalIdMappingsByJewelType.Add(jewelType, localToGlobal);
        }

        GlobalToLocalIdMappingsByJewelType = globalToLocalIdMappingsByJewelType;
        LocalToGlobalIdMappingsByJewelType = localToGlobalIdMappingsByJewelType;
    }

    private static IReadOnlyDictionary<uint, byte> GetGlobalToLocalIdMapping(int jewelType)
    {
        if (GlobalToLocalIdMappingsByJewelType == null ||
            !GlobalToLocalIdMappingsByJewelType.TryGetValue(jewelType, out IReadOnlyDictionary<uint, byte> mapping))
        {
            ExitWithError($"No local id mapping exists for jewel type [yellow]{jewelType}[/].");
            throw new InvalidOperationException($"No local id mapping exists for jewel type {jewelType}.");
        }
        return mapping;
    }

    private static byte MapGlobalIdToLocalId(IReadOnlyDictionary<uint, byte> localIdMapping, int jewelType, uint globalId)
    {
        if (!localIdMapping.TryGetValue(globalId, out byte localId))
        {
            GetJewelTypeInfo(jewelType, out _, out _, out _, out string jewelName);
            ExitWithError($"No local id mapping for [yellow]{jewelName}[/] global id [yellow]{globalId}[/].");
        }
        return localId;
    }

    private static void AppendLocalIdMappings(StringBuilder sb)
    {
        sb.AppendLine("nodeIDList[\"localIdToGlobalId\"] = { }");

        for (int jewelType = 1; jewelType <= MaxJewelType; jewelType++)
        {
            if (LocalToGlobalIdMappingsByJewelType == null ||
                !LocalToGlobalIdMappingsByJewelType.TryGetValue(jewelType, out IReadOnlyDictionary<byte, uint> localToGlobalIdMapping))
            {
                ExitWithError($"No reverse local id mapping exists for jewel type [yellow]{jewelType}[/].");
                throw new InvalidOperationException($"No reverse local id mapping exists for jewel type {jewelType}.");
            }

            sb.AppendLine($"nodeIDList[\"localIdToGlobalId\"][{jewelType}] = {{ }}");
            sb.AppendLine($"nodeIDList[\"localIdToGlobalId\"][{jewelType}][\"size\"] = {localToGlobalIdMapping.Count}");

            foreach (KeyValuePair<byte, uint> keyValuePair in localToGlobalIdMapping.OrderBy(q => q.Key))
                sb.AppendLine($"nodeIDList[\"localIdToGlobalId\"][{jewelType}][{keyValuePair.Key}] = {keyValuePair.Value}");
        }
    }

    private static void GenerateGloriousVanity(List<PassiveSkill> nodes, bool isCsvExport, Dictionary<string, int> notableJewelSocketMappings, out int[] luaDefinitions, out byte[] data, out List<CsvExportRow> csvData)
    {
        IReadOnlyDictionary<uint, byte> localIdMapping = GetGlobalToLocalIdMapping(1);
        GetJewelTypeInfo(1, out int jewelMin, out int jewelMax, out int jewelIncrement, out string jewelName);
        int seedMinimumIndex = jewelMin / jewelIncrement;
        int seedMaximumIndex = jewelMax / jewelIncrement;
        int maxSeed = (jewelMax - jewelMin) / jewelIncrement + 1;
        //the datafile header. cant use out params in anonymous methods
        byte[] header = new byte[maxSeed * nodes.Count];
        //the actual information for the jewels. will convert to 1d later
        byte[][] data2d = new byte[maxSeed * nodes.Count][];
        var export = new ConcurrentBag<CsvExportRow>();

        //nested parallel tasks, in case your cpu wasn't on fire yet
        Parallel.For(0, nodes.Count, nodeIndex =>
        {
            var node = nodes[nodeIndex];
            for (int seedIndex = seedMinimumIndex; seedIndex <= seedMaximumIndex; seedIndex++)
            {
                //which jewel seed is this
                int jewelSeed = (seedIndex * jewelIncrement);
                int jewelIndex = (seedIndex - seedMinimumIndex);
                int jewelType = 1;
                //modify the tree using that jewel
                TimelessJewel timelessJewelFromInput = GetTimelessJewel((uint)jewelSeed, (uint)jewelType);
                if (timelessJewelFromInput == null)
                    Program.ExitWithError("Failed to get the [yellow]timeless jewel[/] from input.");
                //determine how the particular node was modified
                var alternateTreeManager = new AlternateTreeManager(node, timelessJewelFromInput);
                var skillInfo = alternateTreeManager.ReplacePassiveSkill();
                byte[] dataEntry;
                //handle might/legacy of the vaal
                if (skillInfo.AlternatePassiveSkill.Index == LegacyOfTheVaal || skillInfo.AlternatePassiveSkill.Index == MightOfTheVaal)
                {
                    //just shit out the stats, the fact that its legacy/might is implied
                    int additionCount = skillInfo.AlternatePassiveAdditionInformations.Count;
                    dataEntry = new byte[additionCount * 2];
                    for (int k = 0; k < additionCount; k++)
                    {
                        //add the additions
                        AlternatePassiveAdditionInformation additionInformation = skillInfo.AlternatePassiveAdditionInformations[k];
                        dataEntry[k] = MapGlobalIdToLocalId(localIdMapping, 1, additionInformation.AlternatePassiveAddition.Index);
                        dataEntry[additionCount + k] = (byte)additionInformation.StatRolls[0];
                    }
                }
                //handle all others
                else
                {
                    int rollCount = skillInfo.StatRolls.Count;
                    dataEntry = new byte[rollCount + 1];
                    uint replacementGlobalId = (skillInfo.AlternatePassiveSkill.Index + (uint)NumAdditions);
                    dataEntry[0] = MapGlobalIdToLocalId(localIdMapping, 1, replacementGlobalId);
                    for (int k = 0; k < rollCount; k++)
                        dataEntry[k + 1] = (byte)skillInfo.StatRolls[k];
                }
                //save the data
                int dataIndex = ((nodeIndex * maxSeed) + jewelIndex);
                header[dataIndex] = (byte)dataEntry.Length;
                data2d[dataIndex] = dataEntry;

                if (isCsvExport)
                {
                    var exportRow = new CsvExportRow(
                        jewelSeed,
                        jewelName,
                        jewelType,
                        node,
                        (uint)nodeIndex,
                        skillInfo.AlternatePassiveSkill.Name,
                        notableJewelSocketMappings
                    );
                    // A JewelSocketId of 0 indicates that the notable does not appear
                    // in the radius of a jewel socket...so don't export it to the CSV
                    if (exportRow.JewelSocketId > 0)
                    {
                        export.Add(exportRow);
                    }
                }
            }
        });

        var outputData = new List<byte>(header);
        foreach (var entry in data2d.Where(d => d is not null))
        {
            outputData.AddRange(entry);
        }
        luaDefinitions = new int[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            luaDefinitions[i] = header.Skip(i * maxSeed).Take(maxSeed).Sum(x => x);
        }
        data = outputData.ToArray();
        csvData = export.ToList();
    }

    private static void GenerateRegular(List<PassiveSkill> nodes, bool isCsvExport, int jewelType, Dictionary<string, int> notableJewelSocketMappings, out byte[] data, out List<CsvExportRow> csvData)
    {
        GetJewelTypeInfo(jewelType, out int jewelMin, out int jewelMax, out int jewelIncrement, out string jewelName);

        AnsiConsole.MarkupLine($"[green]Calculating {jewelName} seeds[/]...");

        int maxSeed = (jewelMax - jewelMin) / jewelIncrement + 1;
        byte[] dataInternal = new byte[maxSeed * nodes.Count];
        IReadOnlyDictionary<uint, byte> localIdMapping = GetGlobalToLocalIdMapping(jewelType);

        //re-index our additions and replacements to consider only this jewel type
        uint numAdditions = (uint)DataManager.AlternatePassiveAdditions.Count;
        var jewelEffectOptions = DataManager.AlternatePassiveAdditions.Where(x => x.AlternateTreeVersionIndex == jewelType).Select(x => x.Index)
            .Concat(DataManager.AlternatePassiveSkills.Where(x => x.AlternateTreeVersionIndex == jewelType).Select(x => x.Index + numAdditions))
            .Select((x, i) => new { rid = x, index = (byte)i }).ToDictionary(x => x.rid, x => x.index);

        Dictionary<uint, string> notableJewelReplacementNames;
        var timelessJewelsWithAdditions = new[] { "LethalPride", "BrutalRestraint" };
        if (timelessJewelsWithAdditions.Contains(jewelName))
        {
            notableJewelReplacementNames = DataManager.AlternatePassiveAdditions
                .Where(x => x.AlternateTreeVersionIndex == jewelType)
                .ToDictionary(x => x.Index, x => x.Id);
        }
        else
        {
            notableJewelReplacementNames = DataManager.AlternatePassiveSkills
                .Where(x => x.AlternateTreeVersionIndex == jewelType)
                .ToDictionary(x => x.Index, x => x.Name);
        }

        if (jewelEffectOptions.Count > 256)
        {
            throw new Exception("Cannot safely construct file: possible indices greater than byte size");
        }

        var export = new ConcurrentBag<CsvExportRow>();
        //for non glorious vanity, we only care about notables
        Parallel.For(0, nodes.Count, notableIndex =>
        {
            var notable = nodes[notableIndex];
            Parallel.For(jewelMin / jewelIncrement, jewelMax / jewelIncrement + 1, i =>
            {
                //which jewel seed is this
                i *= jewelIncrement;
                int jewelSeed = i;
                int jewelIndex = (jewelSeed - jewelMin) / jewelIncrement;
                //modify the tree using that jewel
                TimelessJewel timelessJewelFromInput = GetTimelessJewel((uint)jewelSeed, (uint)jewelType);
                if (timelessJewelFromInput == null)
                    Program.ExitWithError("Failed to get the [yellow]timeless jewel[/] from input.");
                //figure out how it affects this specific notable
                var alternateTreeManager = new AlternateTreeManager(notable, timelessJewelFromInput);
                bool flag = alternateTreeManager.IsPassiveSkillReplaced();
                byte passiveSkillIndex = 0;
                uint notableAlternatePassiveIndex = 0;
                string notableReplacementName = string.Empty;
                if (flag)
                {
                    notableAlternatePassiveIndex = alternateTreeManager.ReplacePassiveSkill().AlternatePassiveSkill.Index;
                    notableReplacementName = notableJewelReplacementNames[notableAlternatePassiveIndex];
                    passiveSkillIndex = jewelEffectOptions[notableAlternatePassiveIndex + numAdditions];
                }
                else
                {
                    notableAlternatePassiveIndex = alternateTreeManager.AugmentPassiveSkill().FirstOrDefault()?.AlternatePassiveAddition.Index ?? 0;
                    notableReplacementName = notableJewelReplacementNames[notableAlternatePassiveIndex];
                    passiveSkillIndex = jewelEffectOptions[notableAlternatePassiveIndex];
                }

                if (isCsvExport)
                {
                    var exportRow = new CsvExportRow(
                        jewelSeed,
                        jewelName,
                        jewelType,
                        notable,
                        notableAlternatePassiveIndex,
                        notableReplacementName,
                        notableJewelSocketMappings
                    );
                    // A JewelSocketId of 0 indicates that the notable does not appear in the radius of a jewel socket
                    if (exportRow.JewelSocketId > 0)
                    {
                        export.Add(exportRow);
                    }
                }

                dataInternal[notableIndex * maxSeed + jewelIndex] = passiveSkillIndex;
            });
        });

        data = dataInternal;
        csvData = export.ToList();
    }

    private static void GetJewelTypeInfo(int jewelType, out int jewelMin, out int jewelMax, out int jewelIncrement, out string jewelName)
    {
        switch (jewelType)
        {
            case 1:
                jewelMin = 100;
                jewelMax = 8000;
                jewelIncrement = 1;
                jewelName = "GloriousVanity";
                break;
            case 2:
                jewelMin = 10000;
                jewelMax = 18000;
                jewelIncrement = 1;
                jewelName = "LethalPride";
                break;
            case 3:
                jewelMin = 500;
                jewelMax = 8000;
                jewelIncrement = 1;
                jewelName = "BrutalRestraint";
                break;
            case 4:
                jewelMin = 2000;
                jewelMax = 10000;
                jewelIncrement = 1;
                jewelName = "MilitantFaith";
                break;
            case 5:
                jewelMin = 2000;
                jewelMax = 160000;
                jewelIncrement = 20;
                jewelName = "ElegantHubris";
                break;
            case 6:
                jewelMin = 100;
                jewelMax = 8000;
                jewelIncrement = 1;
                jewelName = "HeroicTragedy";
                break;
            case 7:
                jewelMin = 100;
                jewelMax = 8000;
                jewelIncrement = 1;
                jewelName = "AbyssTecrod";
                break;
            case 8:
                jewelMin = 100;
                jewelMax = 8000;
                jewelIncrement = 1;
                jewelName = "AbyssUlaman";
                break;
            case 9:
                jewelMin = 100;
                jewelMax = 8000;
                jewelIncrement = 1;
                jewelName = "AbyssKurgal";
                break;
            case 10:
                jewelMin = 100;
                jewelMax = 8000;
                jewelIncrement = 1;
                jewelName = "AbyssAmanamu";
                break;
            case 11:
                jewelMin = 100;
                jewelMax = 8000;
                jewelIncrement = 1;
                jewelName = "AbyssZorath";
                break;
            default:
                ExitWithError($"Unrecognized jewel type code: [yellow]{jewelType}[/].");
                jewelMin = 0;
                jewelMax = 0;
                jewelIncrement = 1;
                jewelName = "UnknownJewel";
                break;
        }
    }

    private static byte[] Compress(byte[] data)
    {
        var internalMemoryStream = new MemoryStream();
        //deflate it to the smallest size. we have time.
        using (var deflateStream = new ZLibStream(internalMemoryStream, CompressionLevel.SmallestSize))
        {
            deflateStream.Write(data, 0, data.Length);
        }
        return internalMemoryStream.ToArray();
    }

    private static TimelessJewel GetTimelessJewel(uint seed, uint jewelType)
    {
        if (!AlternateTreeVersionsByIndex.TryGetValue(jewelType, out AlternateTreeVersion alternateTreeVersion))
            throw new InvalidOperationException($"No alternate tree version exists for jewel type {jewelType}.");
        return new TimelessJewel(alternateTreeVersion, seed);
    }

    private static bool IsAbyssInspectionRequested(string[] args) =>
        args != null && args.Length > 0 && string.Equals(args[0], "--inspect-abyss", StringComparison.OrdinalIgnoreCase);

    private static bool IsAbyssRangeExportRequested(string[] args) =>
        args != null && args.Length > 0 && string.Equals(args[0], "--export-abyss-range", StringComparison.OrdinalIgnoreCase);

    private static void RunAbyssRangeExport(string[] args)
    {
        if (args.Length != 8 && args.Length != 9)
        {
            ExitWithError("Abyss range export requires: --export-abyss-range <additions.json> <skills.json> <tree.json> <outputFile> <jewelType> <seedMin> <seedMax> [compressed]");
            return;
        }
        bool compressed = args.Length == 9;
        if (compressed && !string.Equals(args[8], "compressed", StringComparison.OrdinalIgnoreCase))
        {
            ExitWithError("The optional Abyss range export mode must be 'compressed'.");
            return;
        }
        GeneratorSettings.AlternatePassiveAdditionsFilePath = args[1];
        GeneratorSettings.AlternatePassiveSkillsFilePath = args[2];
        GeneratorSettings.PassiveSkillsFilePath = args[3];
        if (!int.TryParse(args[5], out int jewelType) || jewelType < 7 || jewelType > 11)
        {
            ExitWithError("Abyss range export jewelType must be from 7 through 11.");
            return;
        }
        if (!int.TryParse(args[6], out int seedMinimum) || !int.TryParse(args[7], out int seedMaximum) ||
            seedMinimum < 100 || seedMaximum > 8000 || seedMinimum > seedMaximum)
        {
            ExitWithError("Abyss range export seeds must satisfy 100 <= seedMin <= seedMax <= 8000.");
            return;
        }
        if (!DataManager.Initialize())
        {
            ExitWithError("Failed to initialize the data manager for Abyss range export.");
            return;
        }
        AlternateTreeVersionsByIndex = DataManager.AlternateTreeVersions.ToDictionary(q => q.Index);
        NumAdditions = DataManager.AlternatePassiveAdditions.Count;
        InitializeLocalIdMappings();
        IReadOnlyDictionary<uint, byte> localIdMapping = GetGlobalToLocalIdMapping(jewelType);
        byte[] output = jewelType == 11
            ? AbyssDataWriter.GeneratePathDependentChanges(
                jewelType, seedMinimum, seedMaximum, 1, NumAdditions,
                globalId => MapGlobalIdToLocalId(localIdMapping, jewelType, globalId))
            : AbyssDataWriter.GenerateSocketDependent(
                jewelType, seedMinimum, seedMaximum, 1, NumAdditions,
                globalId => MapGlobalIdToLocalId(localIdMapping, jewelType, globalId));
        if (compressed)
            output = Compress(output);
        string outputPath = Path.GetFullPath(args[4]);
        string outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
            Directory.CreateDirectory(outputDirectory);
        File.WriteAllBytes(outputPath, output);
        Console.WriteLine($"Wrote {output.Length} bytes{(compressed ? " (zlib compressed)" : string.Empty)} to {outputPath}");
    }

    private static bool IsAbyssAscendancyInspectionRequested(string[] args) =>
        args != null && args.Length > 0 && string.Equals(args[0], "--inspect-abyss-ascendancy", StringComparison.OrdinalIgnoreCase);

    private static void RunAbyssAscendancyInspection(string[] args)
    {
        if (args.Length != 6)
        {
            ExitWithError("Ascendancy inspection requires: --inspect-abyss-ascendancy <additions.json> <skills.json> <tree.json> <seed> <ascendancyName>");
            return;
        }
        GeneratorSettings.AlternatePassiveAdditionsFilePath = args[1];
        GeneratorSettings.AlternatePassiveSkillsFilePath = args[2];
        GeneratorSettings.PassiveSkillsFilePath = args[3];
        if (!uint.TryParse(args[4], out uint seed) || seed < 100 || seed > 8000)
        {
            ExitWithError("Ascendancy inspection seed must be from 100 through 8000.");
            return;
        }
        if (!DataManager.Initialize())
        {
            ExitWithError("Failed to initialize the data manager for ascendancy inspection.");
            return;
        }
        AlternateTreeVersionsByIndex = DataManager.AlternateTreeVersions.ToDictionary(q => q.Index);
        TimelessJewel jewel = GetTimelessJewel(seed, 11);
        AbyssAscendancyManager ascendancyManager = new AbyssAscendancyManager();
        IReadOnlyList<PassiveSkill> selected = ascendancyManager.SelectAffectedNotables(args[5], seed);
        object output = new
        {
            Seed = seed,
            Ascendancy = args[5],
            Selected = selected.Select(q =>
            {
                AlternatePassiveSkillInformation replacement = new AlternateTreeManager(q, jewel).ReplacePassiveSkill();
                return new
                {
                    q.GraphIdentifier,
                    q.Name,
                    AlternatePassiveId = replacement.AlternatePassiveSkill.Id,
                    AlternatePassiveName = replacement.AlternatePassiveSkill.Name,
                    Stats = BuildStatRolls(replacement.AlternatePassiveSkill.StatIndices, replacement.StatRolls)
                };
            }).ToArray()
        };
        Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions() { WriteIndented = true }));
    }

    private static void RunAbyssInspection(string[] args)
    {
        const int expectedInspectionArgumentCount = 7;
        if (args.Length != expectedInspectionArgumentCount)
        {
            ExitWithError("Abyss inspection requires: --inspect-abyss <additions.json> <skills.json> <tree.json> <jewelType> <seed> <socketGraphId>");
            return;
        }

        GeneratorSettings.AlternatePassiveAdditionsFilePath = args[1];
        GeneratorSettings.AlternatePassiveSkillsFilePath = args[2];
        GeneratorSettings.PassiveSkillsFilePath = args[3];
        if (!uint.TryParse(args[4], out uint jewelType) || jewelType < 7 || jewelType > 11)
        {
            ExitWithError("Abyss inspection jewelType must be from 7 through 11.");
            return;
        }
        if (!uint.TryParse(args[5], out uint seed) || seed < 100 || seed > 8000)
        {
            ExitWithError("Abyss inspection seed must be from 100 through 8000.");
            return;
        }
        if (!uint.TryParse(args[6], out uint socketGraphIdentifier))
        {
            ExitWithError("Abyss inspection socketGraphId must be an unsigned integer.");
            return;
        }

        if (!DataManager.Initialize())
        {
            ExitWithError("Failed to initialize the data manager for Abyss inspection.");
            return;
        }
        AlternateTreeVersionsByIndex = DataManager.AlternateTreeVersions.ToDictionary(q => q.Index);

        TimelessJewel timelessJewel = GetTimelessJewel(seed, jewelType);
        IReadOnlyList<PassiveSkill> affectedPassives;
        if (jewelType == 11)
        {
            PassiveSkill passiveSkill = DataManager.GetPassiveSkill(socketGraphIdentifier);
            if (passiveSkill == null)
            {
                ExitWithError($"No passive exists with graph identifier {socketGraphIdentifier}.");
                return;
            }
            affectedPassives = new PassiveSkill[] { passiveSkill };
        }
        else
        {
            AbyssTreeManager abyssTreeManager = new AbyssTreeManager();
            affectedPassives = abyssTreeManager.SelectAffectedPassives(socketGraphIdentifier, seed);
        }
        List<object> changes = new List<object>(affectedPassives.Count);

        foreach (PassiveSkill passiveSkill in affectedPassives)
        {
            AlternateTreeManager alternateTreeManager = new AlternateTreeManager(passiveSkill, timelessJewel);
            if (alternateTreeManager.IsPassiveSkillReplaced())
            {
                AlternatePassiveSkillInformation replacement = alternateTreeManager.ReplacePassiveSkill();
                changes.Add(new
                {
                    passiveSkill.GraphIdentifier,
                    passiveSkill.Name,
                    PassiveType = DataManager.GetPassiveSkillType(passiveSkill).ToString(),
                    ChangeType = "Replacement",
                    AlternatePassiveId = replacement.AlternatePassiveSkill.Id,
                    AlternatePassiveName = replacement.AlternatePassiveSkill.Name,
                    Stats = BuildStatRolls(replacement.AlternatePassiveSkill.StatIndices, replacement.StatRolls)
                });
            }
            else
            {
                IReadOnlyList<AlternatePassiveAdditionInformation> additions = alternateTreeManager.AugmentPassiveSkill();
                changes.Add(new
                {
                    passiveSkill.GraphIdentifier,
                    passiveSkill.Name,
                    PassiveType = DataManager.GetPassiveSkillType(passiveSkill).ToString(),
                    ChangeType = "Addition",
                    Additions = additions.Select(q => new
                    {
                        AlternatePassiveId = q.AlternatePassiveAddition.Id,
                        Stats = BuildStatRolls(q.AlternatePassiveAddition.StatIndices, q.StatRolls)
                    }).ToArray()
                });
            }
        }

        object output = new
        {
            JewelType = jewelType,
            Seed = seed,
            InputGraphIdentifier = socketGraphIdentifier,
            AffectedCount = affectedPassives.Count,
            AffectedGraphIdentifiers = affectedPassives.Select(q => q.GraphIdentifier).ToArray(),
            Changes = changes
        };
        Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions() { WriteIndented = true }));
    }

    private static object[] BuildStatRolls(IReadOnlyCollection<uint> statIndices, IReadOnlyList<int> statRolls)
    {
        uint[] indices = statIndices?.ToArray() ?? Array.Empty<uint>();
        int count = Math.Min(indices.Length, statRolls.Count);
        object[] result = new object[count];
        for (int i = 0; i < count; i++)
            result[i] = new { StatIndex = indices[i], Roll = statRolls[i] };
        return result;
    }

    private static bool TryParseCommandLineArguments(string[] args, out string outputDir, out string compression, out string error)
    {
        outputDir = null;
        compression = null;
        error = null;

        if (args.Length != ExpectedArgumentCount)
        {
            error = $"Expected {ExpectedArgumentCount} arguments, received {args.Length}.";
            return false;
        }

        string additionsFilePath = args[0];
        string skillsFilePath = args[1];
        string treeFilePath = args[2];
        outputDir = args[3];
        string compressionMode = args[4];

        if (!File.Exists(additionsFilePath))
        {
            error = $"Unable to find alternate passive additions file: '{additionsFilePath}'.";
            return false;
        }

        if (!File.Exists(skillsFilePath))
        {
            error = $"Unable to find alternate passive skills file: '{skillsFilePath}'.";
            return false;
        }

        if (!File.Exists(treeFilePath))
        {
            error = $"Unable to find skill tree file: '{treeFilePath}'.";
            return false;
        }

        if (File.Exists(outputDir))
        {
            error = $"Output path points to a file, expected a directory: '{outputDir}'.";
            return false;
        }

        if (TryParseCompressionMode(compressionMode, out compression) == null)
        {
            error = $"Unrecognized output type '{compressionMode}'. Use compressed, uncompressed, both, or 1/2/3.";
            return false;
        }

        GeneratorSettings.AlternatePassiveAdditionsFilePath = additionsFilePath;
        GeneratorSettings.AlternatePassiveSkillsFilePath = skillsFilePath;
        GeneratorSettings.PassiveSkillsFilePath = treeFilePath;

        return true;
    }

    private static string TryParseCompressionMode(string compressionMode, out string compression)
    {
        compression = null;

        if (string.IsNullOrWhiteSpace(compressionMode))
            return "uncompressed";

        if (int.TryParse(compressionMode, out int numericCompression) &&
            numericCompression >= 1 && numericCompression <= 3)
        {
            compression = numericCompression.ToString();
            return "compressed";
        }

        switch (compressionMode.Trim().ToLowerInvariant())
        {
            case "1":
                compression = "compressed";
                break;
            case "2":
                compression = "uncompressed";
                break;
            case "3":
                compression = "both";
                break;
            default:
                compression = "uncompressed";
                break;
        }

        return compression;
    }

    private static bool IsHelpRequested(string[] args)
    {
        return args != null &&
            args.Length == 1 &&
            (args[0] == "-h" || args[0] == "--help" || args[0] == "/?");
    }

    private static void PrintUsage()
    {
        AnsiConsole.WriteLine("Usage:");
        AnsiConsole.WriteLine("  DataFileGenerator <alternate_additions_json> <alternate_skills_json> <skill_tree_json> <output_dir> <output_type>");
        AnsiConsole.WriteLine("  output_type: compressed | uncompressed | both | csv | csv (compressed) | 1 | 2 | 3");
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("Abyss diagnostics:");
        AnsiConsole.WriteLine("  DataFileGenerator --inspect-abyss <additions_json> <skills_json> <tree_json> <jewel_type> <seed> <socket_or_node_id>");
        AnsiConsole.WriteLine("  DataFileGenerator --inspect-abyss-ascendancy <additions_json> <skills_json> <tree_json> <seed> <ascendancy_name>");
        AnsiConsole.WriteLine("  DataFileGenerator --export-abyss-range <additions_json> <skills_json> <tree_json> <output_file> <jewel_type> <seed_min> <seed_max> [compressed]");
    }

    private static string CreateCSVFile(List<CsvExportRow> csvExport, string outputDir, string outputFile, bool compress = false)
    {
        var outputPath = Path.Combine(outputDir, Path.ChangeExtension(outputFile, compress ? "csv.gz" : "csv"));
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        if (compress)
        {
            using var fileStream = File.Create(outputPath);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
            using var writer = new StreamWriter(gzipStream);
            writer.Write(SerializeCSV(csvExport));
        }
        else
        {
            File.WriteAllText(outputPath, SerializeCSV(csvExport));
        }

        return outputPath;
    }

    private static string SerializeCSV(List<CsvExportRow> csvExport)
    {
        return CsvSerializer.SerializeToCsv(
            csvExport
                .OrderBy(s => s.JewelSeed)
                .ThenBy(s => s.JewelSocketId)
        );
    }

    private static void WaitForExit(int exitCode = 0)
    {
        if (IsInteractiveMode)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("Press [yellow]any key[/] to exit.");

            try
            {
                Console.ReadKey();
            }
            catch { }
        }

        Environment.Exit(exitCode);
    }

    private static void PrintError(string error)
    {
        AnsiConsole.MarkupLine($"[red]Error[/]: {error}");
    }

    private static void ExitWithError(string error)
    {
        PrintError(error);
        WaitForExit(1);
    }

    private static void PromptUserForFile(string query, out string response, bool isDir = false)
    {
        TextPrompt<string> fileTextPrompt = new TextPrompt<string>(query)
            .Validate((string input) =>
            {
                if (isDir)
                {
                    if (!Directory.Exists(input))
                    {
                        return ValidationResult.Error($"[red]Error[/]: Unable to find directory: '{input}'");
                    }
                    return ValidationResult.Success();
                }

                if (!File.Exists(input))
                {
                    return ValidationResult.Error($"[red]Error[/]: Unable to find file: '{input}'");
                }
                return ValidationResult.Success();
            });
        response = AnsiConsole.Prompt(fileTextPrompt);
    }

    private static void PromptUserForChoice(string query, List<string> choices, out int response)
    {
        SelectionPrompt<string> fileTextPrompt = new SelectionPrompt<string>().Title(query);
        foreach (string choice in choices)
        {
            fileTextPrompt.AddChoice(choice);
        }
        response = choices.IndexOf(AnsiConsole.Prompt(fileTextPrompt));
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using DatafileGenerator.Data;
using DatafileGenerator.Data.Models;
using DatafileGenerator.Game;

namespace DatafileGenerator;

public static class Program
{
    private static List<PassiveSkill> GetModifiableNonNotables()
    {
        return DataManager.PassiveSkills.Where(x => x.IsModifiable && !x.IsNotable).ToList();
    }
    private static List<PassiveSkill> GetModifiableNotables()
    {
        return DataManager.PassiveSkills.Where(x => x.IsModifiable && x.IsNotable).ToList();
    }

    public static void Main(string[] arguments)
    {
        //most of this datafile generation is hacked together on top of a timeless jewel emulator
        //as such, forgive the scuff

        //this is the path to the data.json file that GGG releases over at https://github.com/grindinggear/skilltree-export
        Settings.PassiveSkillsFilePath = @"C:\RealShit\poe tools\data.json";
        if (!DataManager.Initialize())
            Program.ExitWithError("Failed to initialize the [yellow]data manager[/].");

        //where everything will output
        string outputDir = "DatafileOutput";
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        var justNotables = GetModifiableNotables();
        var justSmallNodes = GetModifiableNonNotables();
        justNotables.Sort();
        justSmallNodes.Sort();
        //generate our indices
        var notablesThenSmalls = justNotables.ToList();
        notablesThenSmalls.AddRange(justSmallNodes);
        string csvFileName = "node_indices.csv";
        //build the csv
        if (File.Exists(csvFileName))
            File.Delete(csvFileName);
        StringBuilder sb = new StringBuilder("PassiveSkillGraphId,Name,Datafile Parsing Index\n");
        for (int k = 0; k < notablesThenSmalls.Count; k++)
        {
            var node = notablesThenSmalls[k];
            sb.AppendLine(node.GraphIdentifier + "," + (node.Name.Contains(',') ? ("\"" + node.Name + "\"") : node.Name) + "," + k);
        }
        File.WriteAllText(Path.Combine(outputDir, csvFileName), sb.ToString());
        Console.WriteLine("Data loaded, starting processing");
        //begin iterating over the 5 jewel types
        for (int i = 5; i > 0; i--)
        {
            string output_file = string.Empty;
            int jewel_type_in = i;
            int jewel_min = 0;
            int jewel_max = 0;
            int jewel_increment = 1;
            switch (jewel_type_in)
            {
                case 1:
                    jewel_min = 100;
                    jewel_max = 8000;
                    output_file = Path.Combine(outputDir, @"Glorious Vanity");
                    break;
                case 2:
                    jewel_min = 10000;
                    jewel_max = 18000;
                    output_file = Path.Combine(outputDir, @"Lethal Pride");
                    break;
                case 3:
                    jewel_min = 500;
                    jewel_max = 8000;
                    output_file = Path.Combine(outputDir, @"Brutal Restraint");
                    break;
                case 4:
                    jewel_min = 2000;
                    jewel_max = 10000;
                    output_file = Path.Combine(outputDir, @"Militant Faith");
                    break;
                case 5:
                    jewel_min = 2000;
                    jewel_max = 160000;
                    jewel_increment = 20;
                    output_file = Path.Combine(outputDir, @"Elegant Hubris");
                    break;
                default:
                    return;
            }
            var sw = Stopwatch.StartNew();
            //glorious vanity logic
            if (i == 1)
            {
                int maxSeed = (jewel_max - jewel_min) / jewel_increment + 1;
                //the datafile header
                byte[] header = new byte[maxSeed * notablesThenSmalls.Count];
                //the actual information for the jewels
                byte[][] data = new byte[maxSeed * notablesThenSmalls.Count][];
                //nested parallell tasks, in case your cpu wasnt on fire yet
                Parallel.For(0, notablesThenSmalls.Count, nodeIndex =>
                {
                    var node = notablesThenSmalls[nodeIndex];
                    Parallel.For(jewel_min / jewel_increment, jewel_max / jewel_increment + 1, i =>
                    {
                        //which jewel seed is this
                        i *= jewel_increment;
                        int jewel_seed = i;
                        int jewel_index = (jewel_seed - jewel_min) / jewel_increment;
                        int jewel_type = jewel_type_in;
                        //modify the tree using that jewel
                        TimelessJewel timelessJewelFromInput = GetTimelessJewel((uint)jewel_seed, (uint)jewel_type);
                        if (timelessJewelFromInput == null)
                            Program.ExitWithError("Failed to get the [yellow]timeless jewel[/] from input.");
                        //determine how the particular node was modified
                        var alternateTreeManager = new AlternateTreeManager(node, timelessJewelFromInput);
                        //GV will always replace nodes
                        List<byte> indices = new List<byte>();
                        List<byte> rolls = new List<byte>();
                        var skillInfo = alternateTreeManager.ReplacePassiveSkill();
                        //handle might/legacy of the vaal
                        if (skillInfo.AlternatePassiveSkill.Index == 76 || skillInfo.AlternatePassiveSkill.Index == 77)
                        {
                            //do we want to add the indicator for this being Legacy of the Vaal/Might of the Vaal or just shit out the stats?
                            //indices.Add((byte)(skillInfo.AlternatePassiveSkill.Index + 93 + 1));
                            for (int k = 0; k < skillInfo.AlternatePassiveAdditionInformations.Count; k++)
                            {
                                //add the additions
                                indices.Add((byte)skillInfo.AlternatePassiveAdditionInformations.ElementAt(k).AlternatePassiveAddition.Index);
                                rolls.Add((byte)skillInfo.AlternatePassiveAdditionInformations.ElementAt(k).StatRolls[0U]);
                            }
                        }
                        //handle all others
                        else
                        {
                            indices.Add((byte)(skillInfo.AlternatePassiveSkill.Index + 93 + 1));
                            for (int k = 0; k < skillInfo.StatRolls.Count; k++)
                            {
                                rolls.Add((byte)skillInfo.StatRolls[(uint)k]);
                            }
                        }
                        //save the data
                        var dataEntry = new List<byte>(indices);
                        dataEntry.AddRange(rolls);
                        header[nodeIndex * maxSeed + jewel_index] = (byte)dataEntry.Count;
                        data[nodeIndex * maxSeed + jewel_index] = dataEntry.ToArray();
                    });
                });
                //wipe the file if it exists
                if (File.Exists(output_file))
                    File.Delete(output_file);
                //write the data
                using (Stream file = File.OpenWrite(output_file))
                {
                    file.Write(header, 0, header.Length);
                    for (int k = 0; k < data.Length; k++)
                    {
                        file.Write(data[k], 0, data[k].Length);
                    }
                }
            }
            //non-glorious vanity logic
            else
            {
                int maxSeed = (jewel_max - jewel_min) / jewel_increment + 1;
                byte[] data = new byte[maxSeed * justNotables.Count];
                //for non glorious vanity, we only care about notables
                Parallel.For(0, justNotables.Count, notableIndex =>
                {
                    var notable = justNotables[notableIndex];
                    Parallel.For(jewel_min / jewel_increment, jewel_max / jewel_increment + 1, i =>
                    {
                        //which jewel seed is this
                        i *= jewel_increment;
                        int jewel_seed = i;
                        int jewel_index = (jewel_seed - jewel_min) / jewel_increment;
                        int jewel_type = jewel_type_in;
                        //modify the tree using that jewel
                        TimelessJewel timelessJewelFromInput = GetTimelessJewel((uint)jewel_seed, (uint)jewel_type);
                        if (timelessJewelFromInput == null)
                            Program.ExitWithError("Failed to get the [yellow]timeless jewel[/] from input.");
                        //figure out how it affects this specific notable
                        var alternateTreeManager = new AlternateTreeManager(notable, timelessJewelFromInput);
                        bool flag = alternateTreeManager.IsPassiveSkillReplaced();
                        byte passiveSkillIndex = 0;
                        if (flag)
                        {
                            //replacements get stat rid + 94
                            passiveSkillIndex = (byte)(alternateTreeManager.ReplacePassiveSkill().AlternatePassiveSkill.Index + 93 + 1);
                        }
                        else
                        {
                            //additions get stat rid
                            passiveSkillIndex = (byte)alternateTreeManager.AugmentPassiveSkill().First().AlternatePassiveAddition.Index;
                            //previously templar jewels were weirdly hardcoded to say "no stat" here which is where the stat index 249 case came from
                            //readd a check if that 249 thing is desirable
                        }
                        data[notableIndex * maxSeed + jewel_index] = passiveSkillIndex;
                    });
                });
                //wipe the file if it exists
                if (File.Exists(output_file))
                    File.Delete(output_file);
                //write to the file
                using (Stream file = File.OpenWrite(output_file))
                {
                    file.Write(data, 0, data.Length);
                }
            }
            sw.Stop();
            Console.WriteLine($"{output_file} took {sw.Elapsed.TotalSeconds} seconds");
        }
    }

    private static TimelessJewel GetTimelessJewel(uint seed, uint jewelType)
    {
        Dictionary<uint, Dictionary<string, TimelessJewelConqueror>> timelessJewelConquerors = new Dictionary<uint, Dictionary<string, TimelessJewelConqueror>>()
        {
            {
                1, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Xibaqua", new TimelessJewelConqueror(1, 0) },
                    { "[springgreen3]Zerphi (Legacy)[/]", new TimelessJewelConqueror(2, 0) },
                    { "Ahuana", new TimelessJewelConqueror(2, 1) },
                    { "Doryani", new TimelessJewelConqueror(3, 0) }
                }
            },
            {
                2, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Kaom", new TimelessJewelConqueror(1, 0) },
                    { "Rakiata", new TimelessJewelConqueror(2, 0) },
                    { "[springgreen3]Kiloava (Legacy)[/]", new TimelessJewelConqueror(3, 0) },
                    { "Akoya", new TimelessJewelConqueror(3, 1) }
                }
            },
            {
                3, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "[springgreen3]Deshret (Legacy)[/]", new TimelessJewelConqueror(1, 0) },
                    { "Balbala", new TimelessJewelConqueror(1, 1) },
                    { "Asenath", new TimelessJewelConqueror(2, 0) },
                    { "Nasima", new TimelessJewelConqueror(3, 0) }
                }
            },
            {
                4, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "[springgreen3]Venarius (Legacy)[/]", new TimelessJewelConqueror(1, 0) },
                    { "Maxarius", new TimelessJewelConqueror(1, 1) },
                    { "Dominus", new TimelessJewelConqueror(2, 0) },
                    { "Avarius", new TimelessJewelConqueror(3, 0) }
                }
            },
            {
                5, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Cadiro", new TimelessJewelConqueror(1, 0) },
                    { "Victario", new TimelessJewelConqueror(2, 0) },
                    { "[springgreen3]Chitus (Legacy)[/]", new TimelessJewelConqueror(3, 0) },
                    { "Caspiro", new TimelessJewelConqueror(3, 1) }
                }
            }
        };
        TimelessJewelConqueror timelessJewelConqueror = timelessJewelConquerors[jewelType]
            .First()
            .Value;
        AlternateTreeVersion alternateTreeVersion = DataManager.AlternateTreeVersions
            .First(q => (q.Index == jewelType));
        return new TimelessJewel(alternateTreeVersion, timelessJewelConqueror, (uint)seed);
    }

    private static void WaitForExit()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("Press [yellow]any key[/] to exit.");

        try
        {
            Console.ReadKey();
        }
        catch { }

        Environment.Exit(0);
    }

    private static void PrintError(string error)
    {
        AnsiConsole.MarkupLine($"[red]Error[/]: {error}");
    }

    private static void ExitWithError(string error)
    {
        PrintError(error);
        WaitForExit();
    }

}

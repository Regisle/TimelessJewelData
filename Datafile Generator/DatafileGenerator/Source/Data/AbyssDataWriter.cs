using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatafileGenerator.Data.Models;
using DatafileGenerator.Game;

namespace DatafileGenerator.Data;

public static class AbyssDataWriter
{
    private const byte FormatVersion = 1;
    private const byte ReplacementComponent = 1;
    private const byte AdditionComponent = 2;

    public static byte[] GenerateSocketDependent(
        int jewelType,
        int seedMinimum,
        int seedMaximum,
        int seedIncrement,
        int alternatePassiveAdditionCount,
        Func<uint, byte> mapLocalId)
    {
        ArgumentNullException.ThrowIfNull(mapLocalId, nameof(mapLocalId));
        PassiveSkill[] sockets = DataManager.BaseJewelSockets.OrderBy(q => q.GraphIdentifier).ToArray();
        byte[][] socketBlocks = new byte[sockets.Length][];

        Parallel.For(0, sockets.Length, socketIndex =>
        {
            PassiveSkill socket = sockets[socketIndex];
            AbyssTreeManager abyssTreeManager = new AbyssTreeManager();
            using MemoryStream blockStream = new MemoryStream();
            using BinaryWriter blockWriter = new BinaryWriter(blockStream, Encoding.UTF8, true);

            for (int seed = seedMinimum; seed <= seedMaximum; seed += seedIncrement)
            {
                TimelessJewel jewel = CreateJewel(jewelType, (uint)seed);
                IReadOnlyList<PassiveSkill> selected = abyssTreeManager.SelectAffectedPassives(socket.GraphIdentifier, (uint)seed);
                PassiveSkill[] affected = selected.Where(q => CanModify(q, jewel)).ToArray();
                if (affected.Length > byte.MaxValue)
                    throw new InvalidOperationException($"Seed {seed}, socket {socket.GraphIdentifier} selected too many passives.");

                blockWriter.Write((byte)affected.Length);
                for (int i = 0; i < affected.Length; i++)
                {
                    blockWriter.Write(checked((ushort)affected[i].GraphIdentifier));
                    WriteModification(blockWriter, affected[i], jewel, alternatePassiveAdditionCount, mapLocalId);
                }
            }
            socketBlocks[socketIndex] = blockStream.ToArray();
        });

        using MemoryStream outputStream = new MemoryStream();
        using BinaryWriter outputWriter = new BinaryWriter(outputStream, Encoding.UTF8, true);
        WriteHeader(outputWriter, "ABYS", jewelType, seedMinimum, seedMaximum, seedIncrement);
        outputWriter.Write(checked((byte)sockets.Length));
        outputWriter.Write(checked((byte)AbyssTreeManager.DefaultAbyssSize));
        for (int i = 0; i < sockets.Length; i++)
            outputWriter.Write(checked((ushort)sockets[i].GraphIdentifier));
        for (int i = 0; i < socketBlocks.Length; i++)
            outputWriter.Write(socketBlocks[i]);
        return outputStream.ToArray();
    }

    public static byte[] GeneratePathDependentChanges(
        int jewelType,
        int seedMinimum,
        int seedMaximum,
        int seedIncrement,
        int alternatePassiveAdditionCount,
        Func<uint, byte> mapLocalId)
    {
        ArgumentNullException.ThrowIfNull(mapLocalId, nameof(mapLocalId));
        TimelessJewel sampleJewel = CreateJewel(jewelType, (uint)seedMinimum);
        PassiveSkill[] nodes = DataManager.PassiveSkills
            .Where(q => IsSpecialCandidate(q) && CanModify(q, sampleJewel))
            .OrderBy(q => q.GraphIdentifier)
            .ToArray();
        byte[][] nodeBlocks = new byte[nodes.Length][];

        Parallel.For(0, nodes.Length, nodeIndex =>
        {
            PassiveSkill node = nodes[nodeIndex];
            using MemoryStream blockStream = new MemoryStream();
            using BinaryWriter blockWriter = new BinaryWriter(blockStream, Encoding.UTF8, true);
            for (int seed = seedMinimum; seed <= seedMaximum; seed += seedIncrement)
            {
                TimelessJewel jewel = CreateJewel(jewelType, (uint)seed);
                WriteModification(blockWriter, node, jewel, alternatePassiveAdditionCount, mapLocalId);
            }
            nodeBlocks[nodeIndex] = blockStream.ToArray();
        });

        using MemoryStream outputStream = new MemoryStream();
        using BinaryWriter outputWriter = new BinaryWriter(outputStream, Encoding.UTF8, true);
        WriteHeader(outputWriter, "ABYN", jewelType, seedMinimum, seedMaximum, seedIncrement);
        outputWriter.Write(checked((ushort)nodes.Length));
        for (int i = 0; i < nodes.Length; i++)
            outputWriter.Write(checked((ushort)nodes[i].GraphIdentifier));
        for (int i = 0; i < nodeBlocks.Length; i++)
            outputWriter.Write(nodeBlocks[i]);
        WriteAscendancySelections(outputWriter, seedMinimum, seedMaximum, seedIncrement);
        return outputStream.ToArray();
    }

    private static void WriteAscendancySelections(BinaryWriter writer, int seedMinimum, int seedMaximum, int seedIncrement)
    {
        AbyssAscendancyManager ascendancyManager = new AbyssAscendancyManager();
        IReadOnlyList<string> ascendancies = ascendancyManager.GetSupportedAscendancies();
        writer.Write(Encoding.ASCII.GetBytes("ASCS"));
        writer.Write(checked((ushort)ascendancies.Count));
        for (int ascendancyIndex = 0; ascendancyIndex < ascendancies.Count; ascendancyIndex++)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(ascendancies[ascendancyIndex]);
            writer.Write(checked((byte)nameBytes.Length));
            writer.Write(nameBytes);
            for (int seed = seedMinimum; seed <= seedMaximum; seed += seedIncrement)
            {
                IReadOnlyList<PassiveSkill> selected = ascendancyManager.SelectAffectedNotables(ascendancies[ascendancyIndex], (uint)seed);
                writer.Write(checked((byte)selected.Count));
                for (int selectedIndex = 0; selectedIndex < selected.Count; selectedIndex++)
                    writer.Write(checked((ushort)selected[selectedIndex].GraphIdentifier));
            }
        }
    }

    private static bool IsSpecialCandidate(PassiveSkill passiveSkill) =>
        passiveSkill.IsAbyssTransformable ||
        (passiveSkill.IsAscendancy && passiveSkill.IsNotable && !passiveSkill.IsProxy && !passiveSkill.IsMastery && !passiveSkill.IsJustIcon);

    private static bool CanModify(PassiveSkill passiveSkill, TimelessJewel jewel) =>
        DataManager.GetApplicableAlternatePassiveSkills(passiveSkill, jewel).Count > 0 ||
        DataManager.GetApplicableAlternatePassiveAdditions(passiveSkill, jewel).Count > 0;

    private static TimelessJewel CreateJewel(int jewelType, uint seed)
    {
        AlternateTreeVersion alternateTreeVersion = DataManager.AlternateTreeVersions.First(q => q.Index == (uint)jewelType);
        return new TimelessJewel(alternateTreeVersion, seed);
    }

    private static void WriteHeader(BinaryWriter writer, string magic, int jewelType, int seedMinimum, int seedMaximum, int seedIncrement)
    {
        writer.Write(Encoding.ASCII.GetBytes(magic));
        writer.Write(FormatVersion);
        writer.Write(checked((byte)jewelType));
        writer.Write(checked((ushort)seedMinimum));
        writer.Write(checked((ushort)seedMaximum));
        writer.Write(checked((ushort)seedIncrement));
    }

    private static void WriteModification(
        BinaryWriter writer,
        PassiveSkill passiveSkill,
        TimelessJewel jewel,
        int alternatePassiveAdditionCount,
        Func<uint, byte> mapLocalId)
    {
        AlternateTreeManager alternateTreeManager = new AlternateTreeManager(passiveSkill, jewel);
        if (alternateTreeManager.IsPassiveSkillReplaced())
        {
            AlternatePassiveSkillInformation replacement = alternateTreeManager.ReplacePassiveSkill();
            int componentCount = 1 + replacement.AlternatePassiveAdditionInformations.Count;
            writer.Write(checked((byte)componentCount));
            uint replacementGlobalId = replacement.AlternatePassiveSkill.Index + checked((uint)alternatePassiveAdditionCount);
            WriteComponent(writer, ReplacementComponent, mapLocalId(replacementGlobalId), replacement.StatRolls);
            for (int i = 0; i < replacement.AlternatePassiveAdditionInformations.Count; i++)
            {
                AlternatePassiveAdditionInformation addition = replacement.AlternatePassiveAdditionInformations[i];
                WriteComponent(writer, AdditionComponent, mapLocalId(addition.AlternatePassiveAddition.Index), addition.StatRolls);
            }
        }
        else
        {
            IReadOnlyList<AlternatePassiveAdditionInformation> additions = alternateTreeManager.AugmentPassiveSkill();
            writer.Write(checked((byte)additions.Count));
            for (int i = 0; i < additions.Count; i++)
                WriteComponent(writer, AdditionComponent, mapLocalId(additions[i].AlternatePassiveAddition.Index), additions[i].StatRolls);
        }
    }

    private static void WriteComponent(BinaryWriter writer, byte componentType, byte localId, IReadOnlyList<int> statRolls)
    {
        writer.Write(componentType);
        writer.Write(localId);
        writer.Write(checked((byte)statRolls.Count));
        for (int i = 0; i < statRolls.Count; i++)
            writer.Write(checked((short)statRolls[i]));
    }
}

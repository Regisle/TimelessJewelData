using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DatafileGenerator.Data.Models;
using DatafileGenerator.Game;

namespace DatafileGenerator.Data;

public static class DataManager
{
    private static readonly IReadOnlyList<AlternatePassiveAddition> EmptyAlternatePassiveAdditions = Array.Empty<AlternatePassiveAddition>();
    private static readonly IReadOnlyList<AlternatePassiveSkill> EmptyAlternatePassiveSkills = Array.Empty<AlternatePassiveSkill>();
    public static IReadOnlyCollection<AlternatePassiveAddition> AlternatePassiveAdditions { get; private set; }

    public static IReadOnlyCollection<AlternatePassiveSkill> AlternatePassiveSkills { get; private set; }

    public static IReadOnlyCollection<AlternateTreeVersion> AlternateTreeVersions { get; private set; }

    public static IReadOnlyCollection<PassiveSkill> PassiveSkills { get; private set; }
    public static IReadOnlyDictionary<uint, PassiveSkill> PassiveSkillsByGraphIdentifier { get; private set; }
    public static IReadOnlyList<PassiveSkill> BaseJewelSockets { get; private set; }
    private static IReadOnlyDictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), IReadOnlyList<AlternatePassiveAddition>> AlternatePassiveAdditionsLookup { get; set; }
    private static IReadOnlyDictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), uint> AlternatePassiveAdditionSpawnWeightLookup { get; set; }
    private static IReadOnlyDictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), IReadOnlyList<AlternatePassiveSkill>> AlternatePassiveSkillsLookup { get; set; }
    private static IReadOnlyDictionary<uint, AlternatePassiveSkill> FirstAlternatePassiveSkillByTreeVersion { get; set; }
    private static IReadOnlyDictionary<uint, PassiveSkillType> PassiveSkillTypesByGraphIdentifier { get; set; }
    public static TreeDataFile FullTreeData { get; private set; }

    public static bool Initialize()
    {
        AlternatePassiveAdditions = LoadFromFile<AlternatePassiveAddition>(GeneratorSettings.AlternatePassiveAdditionsFilePath);
        AlternatePassiveSkills = LoadFromFile<AlternatePassiveSkill>(GeneratorSettings.AlternatePassiveSkillsFilePath);
        if (AlternatePassiveAdditions == null || AlternatePassiveSkills == null)
            return false;
        AlternateTreeVersions = GetAlternateTrees();
        AlternatePassiveAdditionsLookup = BuildAlternatePassiveAdditionLookup();
        AlternatePassiveAdditionSpawnWeightLookup = BuildAlternatePassiveAdditionSpawnWeightLookup();
        AlternatePassiveSkillsLookup = BuildAlternatePassiveSkillLookup();
        FirstAlternatePassiveSkillByTreeVersion = BuildFirstAlternatePassiveSkillByTreeVersionLookup();
        TreeDataFile treeDataFile = LoadSingleFromFile<TreeDataFile>(GeneratorSettings.PassiveSkillsFilePath);
        if (treeDataFile == null || treeDataFile.PassiveSkills == null)
            return false;
        FullTreeData = treeDataFile;
        var treeData = treeDataFile.PassiveSkills;
        PassiveSkills = treeData.Values.Where(q => q.GraphIdentifier != 0).ToList();
        PassiveSkillsByGraphIdentifier = PassiveSkills
            .GroupBy(q => q.GraphIdentifier)
            .ToDictionary(q => q.Key, q => q.First());
        BaseJewelSockets = (treeDataFile.JewelSlots ?? Array.Empty<uint>())
            .Select(GetPassiveSkill)
            .Where(q => q != null && q.IsJewelSocket && !q.IsClusterExpansionSocket && !q.IsAscendancy)
            .ToList();
        PassiveSkillTypesByGraphIdentifier = BuildPassiveSkillTypeLookup();

        return !((AlternatePassiveAdditions == null) || (AlternatePassiveSkills == null) || (AlternateTreeVersions == null) || (PassiveSkills == null) || (PassiveSkillsByGraphIdentifier == null) || (BaseJewelSockets == null));
    }

    private static IReadOnlyCollection<AlternateTreeVersion> GetAlternateTrees() =>
        new List<AlternateTreeVersion>()
        {
            new AlternateTreeVersion(1),
            new AlternateTreeVersion(2),
            new AlternateTreeVersion(3),
            new AlternateTreeVersion(4),
            new AlternateTreeVersion(5),
            new AlternateTreeVersion(6),
            new AlternateTreeVersion(7),
            new AlternateTreeVersion(8),
            new AlternateTreeVersion(9),
            new AlternateTreeVersion(10),
            new AlternateTreeVersion(11)
        };

    public static IReadOnlyList<AlternatePassiveAddition> GetApplicableAlternatePassiveAdditions(PassiveSkill passiveSkill, TimelessJewel timelessJewel)
    {
        ArgumentNullException.ThrowIfNull(passiveSkill, nameof(passiveSkill));
        ArgumentNullException.ThrowIfNull(timelessJewel, nameof(timelessJewel));
        PassiveSkillType passiveSkillType = GetPassiveSkillType(passiveSkill);
        (uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType) lookupKey = (timelessJewel.AlternateTreeVersion.Index, passiveSkillType);
        if (AlternatePassiveAdditionsLookup.TryGetValue(lookupKey, out IReadOnlyList<AlternatePassiveAddition> applicableAlternatePassiveAdditions))
            return applicableAlternatePassiveAdditions;
        return EmptyAlternatePassiveAdditions;
    }

    public static uint GetApplicableAlternatePassiveAdditionsSpawnWeight(PassiveSkill passiveSkill, TimelessJewel timelessJewel)
    {
        ArgumentNullException.ThrowIfNull(passiveSkill, nameof(passiveSkill));
        ArgumentNullException.ThrowIfNull(timelessJewel, nameof(timelessJewel));
        PassiveSkillType passiveSkillType = GetPassiveSkillType(passiveSkill);
        (uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType) lookupKey = (timelessJewel.AlternateTreeVersion.Index, passiveSkillType);
        if (AlternatePassiveAdditionSpawnWeightLookup.TryGetValue(lookupKey, out uint totalSpawnWeight))
            return totalSpawnWeight;
        return 0;
    }

    public static AlternatePassiveSkill GetAlternatePassiveSkillKeyStone(TimelessJewel timelessJewel)
    {
        ArgumentNullException.ThrowIfNull(timelessJewel, nameof(timelessJewel));
        if (!FirstAlternatePassiveSkillByTreeVersion.TryGetValue(timelessJewel.AlternateTreeVersion.Index, out AlternatePassiveSkill alternatePassiveSkillKeyStone))
            return null;
        if (!ContainsPassiveSkillType(alternatePassiveSkillKeyStone.ApplicablePassiveTypes, PassiveSkillType.KeyStone))
            return null;
        return alternatePassiveSkillKeyStone;
    }

    public static IReadOnlyList<AlternatePassiveSkill> GetApplicableAlternatePassiveSkills(PassiveSkill passiveSkill, TimelessJewel timelessJewel)
    {
        ArgumentNullException.ThrowIfNull(passiveSkill, nameof(passiveSkill));
        ArgumentNullException.ThrowIfNull(timelessJewel, nameof(timelessJewel));
        PassiveSkillType passiveSkillType = GetPassiveSkillType(passiveSkill);
        (uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType) lookupKey = (timelessJewel.AlternateTreeVersion.Index, passiveSkillType);
        if (AlternatePassiveSkillsLookup.TryGetValue(lookupKey, out IReadOnlyList<AlternatePassiveSkill> applicableAlternatePassiveSkills))
            return applicableAlternatePassiveSkills;
        return EmptyAlternatePassiveSkills;
    }

    public static PassiveSkillType GetPassiveSkillType(PassiveSkill passiveSkill)
    {
        ArgumentNullException.ThrowIfNull(passiveSkill, nameof(passiveSkill));
        if (PassiveSkillTypesByGraphIdentifier != null &&
            PassiveSkillTypesByGraphIdentifier.TryGetValue(passiveSkill.GraphIdentifier, out PassiveSkillType passiveSkillType))
        {
            return passiveSkillType;
        }
        return GetPassiveSkillTypeSlow(passiveSkill);
    }

    private static PassiveSkillType GetPassiveSkillTypeSlow(PassiveSkill passiveSkill)
    {
        if (passiveSkill.IsAscendancy && passiveSkill.IsNotable)
            return PassiveSkillType.AscendancyNotable;

        if (passiveSkill.IsJewelSocket)
            return PassiveSkillType.None;

        if (passiveSkill.IsKeyStone)
            return PassiveSkillType.KeyStone;

        if (passiveSkill.IsNotable)
            return PassiveSkillType.Notable;

        if (passiveSkill.IsAttribute)
            return PassiveSkillType.SmallAttribute;

        return PassiveSkillType.SmallNormal;
    }

    public static PassiveSkill GetPassiveSkill(uint graphIdentifier)
    {
        if (PassiveSkillsByGraphIdentifier != null &&
            PassiveSkillsByGraphIdentifier.TryGetValue(graphIdentifier, out PassiveSkill passiveSkill))
        {
            return passiveSkill;
        }
        return null;
    }

    private static Dictionary<uint, PassiveSkillType> BuildPassiveSkillTypeLookup()
    {
        Dictionary<uint, PassiveSkillType> lookup = new Dictionary<uint, PassiveSkillType>(PassiveSkills.Count);
        foreach (PassiveSkill passiveSkill in PassiveSkills)
        {
            if (!lookup.ContainsKey(passiveSkill.GraphIdentifier))
                lookup.Add(passiveSkill.GraphIdentifier, GetPassiveSkillTypeSlow(passiveSkill));
        }
        return lookup;
    }

    private static Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), IReadOnlyList<AlternatePassiveAddition>> BuildAlternatePassiveAdditionLookup()
    {
        Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), List<AlternatePassiveAddition>> lookup = new Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), List<AlternatePassiveAddition>>();
        foreach (AlternatePassiveAddition alternatePassiveAddition in AlternatePassiveAdditions)
        {
            foreach (uint passiveType in alternatePassiveAddition.ApplicablePassiveTypes)
            {
                (uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType) lookupKey = (alternatePassiveAddition.AlternateTreeVersionIndex, (PassiveSkillType)passiveType);
                if (!lookup.TryGetValue(lookupKey, out List<AlternatePassiveAddition> additions))
                {
                    additions = new List<AlternatePassiveAddition>();
                    lookup.Add(lookupKey, additions);
                }
                additions.Add(alternatePassiveAddition);
            }
        }
        Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), IReadOnlyList<AlternatePassiveAddition>> readOnlyLookup = new Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), IReadOnlyList<AlternatePassiveAddition>>(lookup.Count);
        foreach (KeyValuePair<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), List<AlternatePassiveAddition>> keyValuePair in lookup)
            readOnlyLookup.Add(keyValuePair.Key, keyValuePair.Value);
        return readOnlyLookup;
    }

    private static Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), uint> BuildAlternatePassiveAdditionSpawnWeightLookup()
    {
        Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), uint> lookup = new Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), uint>(AlternatePassiveAdditionsLookup.Count);
        foreach (KeyValuePair<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), IReadOnlyList<AlternatePassiveAddition>> keyValuePair in AlternatePassiveAdditionsLookup)
        {
            uint spawnWeight = 0;
            for (int i = 0; i < keyValuePair.Value.Count; i++)
                spawnWeight += keyValuePair.Value[i].SpawnWeight;
            lookup.Add(keyValuePair.Key, spawnWeight);
        }
        return lookup;
    }

    private static Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), IReadOnlyList<AlternatePassiveSkill>> BuildAlternatePassiveSkillLookup()
    {
        Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), List<AlternatePassiveSkill>> lookup = new Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), List<AlternatePassiveSkill>>();
        foreach (AlternatePassiveSkill alternatePassiveSkill in AlternatePassiveSkills)
        {
            foreach (uint passiveType in alternatePassiveSkill.ApplicablePassiveTypes)
            {
                (uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType) lookupKey = (alternatePassiveSkill.AlternateTreeVersionIndex, (PassiveSkillType)passiveType);
                if (!lookup.TryGetValue(lookupKey, out List<AlternatePassiveSkill> skills))
                {
                    skills = new List<AlternatePassiveSkill>();
                    lookup.Add(lookupKey, skills);
                }
                skills.Add(alternatePassiveSkill);
            }
        }
        Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), IReadOnlyList<AlternatePassiveSkill>> readOnlyLookup = new Dictionary<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), IReadOnlyList<AlternatePassiveSkill>>(lookup.Count);
        foreach (KeyValuePair<(uint alternateTreeVersionIndex, PassiveSkillType passiveSkillType), List<AlternatePassiveSkill>> keyValuePair in lookup)
            readOnlyLookup.Add(keyValuePair.Key, keyValuePair.Value);
        return readOnlyLookup;
    }

    private static Dictionary<uint, AlternatePassiveSkill> BuildFirstAlternatePassiveSkillByTreeVersionLookup()
    {
        Dictionary<uint, AlternatePassiveSkill> lookup = new Dictionary<uint, AlternatePassiveSkill>();
        foreach (AlternatePassiveSkill alternatePassiveSkill in AlternatePassiveSkills)
        {
            if (!lookup.ContainsKey(alternatePassiveSkill.AlternateTreeVersionIndex))
                lookup.Add(alternatePassiveSkill.AlternateTreeVersionIndex, alternatePassiveSkill);
        }
        return lookup;
    }

    private static bool ContainsPassiveSkillType(IReadOnlyCollection<uint> passiveSkillTypes, PassiveSkillType passiveSkillType)
    {
        uint passiveSkillTypeIndex = (uint)passiveSkillType;
        foreach (uint passiveType in passiveSkillTypes)
        {
            if (passiveType == passiveSkillTypeIndex)
                return true;
        }
        return false;
    }

    private static IReadOnlyCollection<T> LoadFromFile<T>(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));

        if (!File.Exists(filePath))
            return null;

        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            return JsonSerializer.Deserialize<IReadOnlyCollection<T>>(fileStream);
    }
    private static T LoadSingleFromFile<T>(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));

        if (!File.Exists(filePath))
            return default;

        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            return JsonSerializer.Deserialize<T>(fileStream);
    }
}

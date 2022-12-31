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
    public static IReadOnlyCollection<AlternatePassiveAddition> AlternatePassiveAdditions { get; private set; }

    public static IReadOnlyCollection<AlternatePassiveSkill> AlternatePassiveSkills { get; private set; }

    public static IReadOnlyCollection<AlternateTreeVersion> AlternateTreeVersions { get; private set; }

    public static IReadOnlyCollection<PassiveSkill> PassiveSkills { get; private set; }

    public static bool Initialize()
    {
        AlternatePassiveAdditions = LoadFromFile<AlternatePassiveAddition>(GeneratorSettings.AlternatePassiveAdditionsFilePath);
        AlternatePassiveSkills = LoadFromFile<AlternatePassiveSkill>(GeneratorSettings.AlternatePassiveSkillsFilePath);
        AlternateTreeVersions = GetAlternateTrees();
        var treeData = LoadSingleFromFile<TreeDataFile>(GeneratorSettings.PassiveSkillsFilePath).PassiveSkills;
        treeData.Remove("root");
        PassiveSkills = treeData.Values.ToList();

        return !((AlternatePassiveAdditions == null) || (AlternatePassiveSkills == null) || (AlternateTreeVersions == null) || (PassiveSkills == null));
    }

    private static IReadOnlyCollection<AlternateTreeVersion> GetAlternateTrees() =>
        new List<AlternateTreeVersion>()
        {
            new AlternateTreeVersion(1),
            new AlternateTreeVersion(2),
            new AlternateTreeVersion(3),
            new AlternateTreeVersion(4),
            new AlternateTreeVersion(5)
        };

    public static List<AlternatePassiveAddition> GetApplicableAlternatePassiveAdditions(PassiveSkill passiveSkill, TimelessJewel timelessJewel)
    {
        ArgumentNullException.ThrowIfNull(passiveSkill, nameof(passiveSkill));
        ArgumentNullException.ThrowIfNull(timelessJewel, nameof(timelessJewel));

        List<AlternatePassiveAddition> applicableAlternatePassiveAdditions = new List<AlternatePassiveAddition>();

        foreach (AlternatePassiveAddition alternatePassiveAddition in AlternatePassiveAdditions)
        {
            PassiveSkillType passiveSkillType = GetPassiveSkillType(passiveSkill);

            if ((alternatePassiveAddition.AlternateTreeVersionIndex != timelessJewel.AlternateTreeVersion.Index) ||
                !alternatePassiveAddition.ApplicablePassiveTypes.Any(q => (q == ((uint)passiveSkillType))))
            {
                continue;
            }

            applicableAlternatePassiveAdditions.Add(alternatePassiveAddition);
        }

        return applicableAlternatePassiveAdditions;
    }

    public static AlternatePassiveSkill GetAlternatePassiveSkillKeyStone(TimelessJewel timelessJewel)
    {
        ArgumentNullException.ThrowIfNull(timelessJewel, nameof(timelessJewel));

        AlternatePassiveSkill alternatePassiveSkillKeyStone = AlternatePassiveSkills.FirstOrDefault(q =>
            q.AlternateTreeVersionIndex == timelessJewel.AlternateTreeVersion.Index);

        if (!alternatePassiveSkillKeyStone.ApplicablePassiveTypes.Any(q => (q == ((uint)PassiveSkillType.KeyStone))))
            return null;

        return alternatePassiveSkillKeyStone;
    }

    public static List<AlternatePassiveSkill> GetApplicableAlternatePassiveSkills(PassiveSkill passiveSkill, TimelessJewel timelessJewel)
    {
        ArgumentNullException.ThrowIfNull(passiveSkill, nameof(passiveSkill));
        ArgumentNullException.ThrowIfNull(timelessJewel, nameof(timelessJewel));

        List<AlternatePassiveSkill> applicableAlternatePassiveSkills = new List<AlternatePassiveSkill>();

        foreach (AlternatePassiveSkill alternatePassiveSkill in AlternatePassiveSkills)
        {
            PassiveSkillType passiveSkillType = GetPassiveSkillType(passiveSkill);

            if ((alternatePassiveSkill.AlternateTreeVersionIndex != timelessJewel.AlternateTreeVersion.Index) ||
                !alternatePassiveSkill.ApplicablePassiveTypes.Any(q => (q == ((uint)passiveSkillType))))
            {
                continue;
            }

            applicableAlternatePassiveSkills.Add(alternatePassiveSkill);
        }

        return applicableAlternatePassiveSkills;
    }

    public static PassiveSkillType GetPassiveSkillType(PassiveSkill passiveSkill)
    {
        ArgumentNullException.ThrowIfNull(passiveSkill, nameof(passiveSkill));

        if (passiveSkill.IsJewelSocket)
            return PassiveSkillType.JewelSocket;

        if (passiveSkill.IsKeyStone)
            return PassiveSkillType.KeyStone;

        if (passiveSkill.IsNotable)
            return PassiveSkillType.Notable;

        if (passiveSkill.IsAttribute)
            return PassiveSkillType.SmallAttribute;

        return PassiveSkillType.SmallNormal;
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

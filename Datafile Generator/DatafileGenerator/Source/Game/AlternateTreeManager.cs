using System;
using System.Collections.Generic;
using DatafileGenerator.Data;
using DatafileGenerator.Data.Models;
using DatafileGenerator.Random;

namespace DatafileGenerator.Game;

public class AlternateTreeManager
{
    public PassiveSkill PassiveSkill { get; private set; }

    public TimelessJewel TimelessJewel { get; private set; }
    private PassiveSkillType PassiveSkillType { get; }
    private IReadOnlyList<AlternatePassiveSkill> ApplicableAlternatePassiveSkills { get; }
    private IReadOnlyList<AlternatePassiveAddition> ApplicableAlternatePassiveAdditions { get; }
    private uint ApplicableAlternatePassiveAdditionSpawnWeight { get; }

    public AlternateTreeManager(PassiveSkill passiveSkill, TimelessJewel timelessJewel)
    {
        ArgumentNullException.ThrowIfNull(passiveSkill, nameof(passiveSkill));
        ArgumentNullException.ThrowIfNull(timelessJewel, nameof(timelessJewel));

        PassiveSkill = passiveSkill;
        TimelessJewel = timelessJewel;
        PassiveSkillType = DataManager.GetPassiveSkillType(passiveSkill);
        ApplicableAlternatePassiveSkills = DataManager.GetApplicableAlternatePassiveSkills(passiveSkill, timelessJewel);
        ApplicableAlternatePassiveAdditions = DataManager.GetApplicableAlternatePassiveAdditions(passiveSkill, timelessJewel);
        ApplicableAlternatePassiveAdditionSpawnWeight = DataManager.GetApplicableAlternatePassiveAdditionsSpawnWeight(passiveSkill, timelessJewel);
    }

    public bool IsPassiveSkillReplaced()
    {
        if (PassiveSkillType == PassiveSkillType.AscendancyNotable)
            return true;

        if (PassiveSkill.IsKeyStone)
            return true;

        if (PassiveSkill.IsNotable)
        {
            if (TimelessJewel.AlternateTreeVersion.NotableReplacementSpawnWeight >= 100)
                return true;

            RandomNumberGenerator randomNumberGenerator = new RandomNumberGenerator(PassiveSkill, TimelessJewel);

            return (randomNumberGenerator.Generate(0U, 100U) < TimelessJewel.AlternateTreeVersion.NotableReplacementSpawnWeight);
        }

        if (PassiveSkillType == PassiveSkillType.SmallAttribute)
        {
            return TimelessJewel.AlternateTreeVersion.AreSmallAttributePassiveSkillsReplaced;
        }

        return TimelessJewel.AlternateTreeVersion.AreSmallNormalPassiveSkillsReplaced;
    }

    public uint GetRegularPassiveSkillIndex(uint alternatePassiveSkillOffset)
    {
        if (PassiveSkillType == PassiveSkillType.AscendancyNotable)
        {
            RandomNumberGenerator ascendancyRandomNumberGenerator = new RandomNumberGenerator(PassiveSkill, TimelessJewel);
            AlternatePassiveSkill ascendancyReplacement = RollAlternatePassiveSkill(ascendancyRandomNumberGenerator);
            return (ascendancyReplacement.Index + alternatePassiveSkillOffset);
        }

        if (PassiveSkill.IsKeyStone)
        {
            AlternatePassiveSkill alternatePassiveSkillKeyStone = DataManager.GetAlternatePassiveSkillKeyStone(TimelessJewel);
            return (alternatePassiveSkillKeyStone.Index + alternatePassiveSkillOffset);
        }

        bool isNotable = (PassiveSkillType == PassiveSkillType.Notable);
        bool hasConsumedNotableRoll = false;
        bool isReplaced;
        RandomNumberGenerator randomNumberGenerator = null;

        if (isNotable)
        {
            if (TimelessJewel.AlternateTreeVersion.NotableReplacementSpawnWeight >= 100)
            {
                isReplaced = true;
            }
            else
            {
                randomNumberGenerator = new RandomNumberGenerator(PassiveSkill, TimelessJewel);
                uint notableRoll = randomNumberGenerator.Generate(0U, 100U);
                hasConsumedNotableRoll = true;
                isReplaced = (notableRoll < TimelessJewel.AlternateTreeVersion.NotableReplacementSpawnWeight);
            }
        }
        else if (PassiveSkillType == PassiveSkillType.SmallAttribute)
        {
            isReplaced = TimelessJewel.AlternateTreeVersion.AreSmallAttributePassiveSkillsReplaced;
        }
        else
        {
            isReplaced = TimelessJewel.AlternateTreeVersion.AreSmallNormalPassiveSkillsReplaced;
        }

        if (randomNumberGenerator == null)
            randomNumberGenerator = new RandomNumberGenerator(PassiveSkill, TimelessJewel);

        if (isNotable && !hasConsumedNotableRoll)
            randomNumberGenerator.Generate(0U, 100U);

        if (isReplaced)
        {
            AlternatePassiveSkill rolledAlternatePassiveSkill = RollAlternatePassiveSkill(randomNumberGenerator);
            return (rolledAlternatePassiveSkill.Index + alternatePassiveSkillOffset);
        }

        uint minimumAdditions = TimelessJewel.AlternateTreeVersion.MinimumAdditions;
        uint maximumAdditions = TimelessJewel.AlternateTreeVersion.MaximumAdditions;
        uint additionCountRoll = minimumAdditions;

        if (maximumAdditions > minimumAdditions)
            additionCountRoll = randomNumberGenerator.Generate(minimumAdditions, maximumAdditions);

        if (additionCountRoll == 0)
            throw new InvalidOperationException("Expected at least one alternate passive addition for non-replaced passive skill.");

        AlternatePassiveAddition rolledAlternatePassiveAddition = null;

        while (rolledAlternatePassiveAddition == null)
            rolledAlternatePassiveAddition = RollAlternatePassiveAddition(randomNumberGenerator);

        return rolledAlternatePassiveAddition.Index;
    }

    public AlternatePassiveSkillInformation ReplacePassiveSkill()
    {
        if (PassiveSkill.IsKeyStone)
        {
            AlternatePassiveSkill alternatePassiveSkillKeyStone = DataManager.GetAlternatePassiveSkillKeyStone(TimelessJewel);
            int[] alternatePassiveSkillKeyStoneStatRolls = new int[]
            {
                alternatePassiveSkillKeyStone.StatAMinimumValue
            };
            return new AlternatePassiveSkillInformation(alternatePassiveSkillKeyStone, alternatePassiveSkillKeyStoneStatRolls, Array.Empty<AlternatePassiveAdditionInformation>());
        }

        RandomNumberGenerator randomNumberGenerator = new RandomNumberGenerator(PassiveSkill, TimelessJewel);

        if (PassiveSkillType == PassiveSkillType.Notable)
            randomNumberGenerator.Generate(0U, 100U);

        AlternatePassiveSkill rolledAlternatePassiveSkill = RollAlternatePassiveSkill(randomNumberGenerator);

        int rolledAlternatePassiveSkillStatCount = Math.Min(rolledAlternatePassiveSkill.StatIndices.Count, 4);
        int[] alternatePassiveSkillStatRolls = new int[rolledAlternatePassiveSkillStatCount];
        if (rolledAlternatePassiveSkillStatCount >= 1)
            alternatePassiveSkillStatRolls[0] = RollStat(randomNumberGenerator, rolledAlternatePassiveSkill.StatAMinimumValue, rolledAlternatePassiveSkill.StatAMaximumValue);
        if (rolledAlternatePassiveSkillStatCount >= 2)
            alternatePassiveSkillStatRolls[1] = RollStat(randomNumberGenerator, rolledAlternatePassiveSkill.StatBMinimumValue, rolledAlternatePassiveSkill.StatBMaximumValue);
        if (rolledAlternatePassiveSkillStatCount >= 3)
            alternatePassiveSkillStatRolls[2] = RollStat(randomNumberGenerator, rolledAlternatePassiveSkill.StatCMinimumValue, rolledAlternatePassiveSkill.StatCMaximumValue);
        if (rolledAlternatePassiveSkillStatCount >= 4)
            alternatePassiveSkillStatRolls[3] = RollStat(randomNumberGenerator, rolledAlternatePassiveSkill.StatDMinimumValue, rolledAlternatePassiveSkill.StatDMaximumValue);

        if ((rolledAlternatePassiveSkill.MinimumAdditions == 0) && (rolledAlternatePassiveSkill.MaximumAdditions == 0))
            return new AlternatePassiveSkillInformation(rolledAlternatePassiveSkill, alternatePassiveSkillStatRolls, Array.Empty<AlternatePassiveAdditionInformation>());

        uint minimumAdditions = (TimelessJewel.AlternateTreeVersion.MinimumAdditions + rolledAlternatePassiveSkill.MinimumAdditions);
        uint maximumAdditions = (TimelessJewel.AlternateTreeVersion.MaximumAdditions + rolledAlternatePassiveSkill.MaximumAdditions);

        uint additionCountRoll = minimumAdditions;

        if (maximumAdditions > minimumAdditions)
            additionCountRoll = randomNumberGenerator.Generate(minimumAdditions, maximumAdditions);

        List<AlternatePassiveAdditionInformation> alternatePassiveAdditionInformations = new List<AlternatePassiveAdditionInformation>((int)additionCountRoll);

        for (uint i = 0; i < additionCountRoll; i++)
        {
            AlternatePassiveAddition rolledAlternatePassiveAddition = null;

            while (rolledAlternatePassiveAddition == null)
                rolledAlternatePassiveAddition = RollAlternatePassiveAddition(randomNumberGenerator);

            int rolledAlternatePassiveAdditionStatCount = Math.Min(rolledAlternatePassiveAddition.StatIndices.Count, 2);
            int[] alternatePassiveAdditionStatRolls = new int[rolledAlternatePassiveAdditionStatCount];
            if (rolledAlternatePassiveAdditionStatCount >= 1)
                alternatePassiveAdditionStatRolls[0] = RollStat(randomNumberGenerator, rolledAlternatePassiveAddition.StatAMinimumValue, rolledAlternatePassiveAddition.StatAMaximumValue);
            if (rolledAlternatePassiveAdditionStatCount >= 2)
                alternatePassiveAdditionStatRolls[1] = RollStat(randomNumberGenerator, rolledAlternatePassiveAddition.StatBMinimumValue, rolledAlternatePassiveAddition.StatBMaximumValue);
            alternatePassiveAdditionInformations.Add(new AlternatePassiveAdditionInformation(rolledAlternatePassiveAddition, alternatePassiveAdditionStatRolls));
        }

        return new AlternatePassiveSkillInformation(rolledAlternatePassiveSkill, alternatePassiveSkillStatRolls, alternatePassiveAdditionInformations);
    }

    public IReadOnlyList<AlternatePassiveAdditionInformation> AugmentPassiveSkill()
    {
        RandomNumberGenerator randomNumberGenerator = new RandomNumberGenerator(PassiveSkill, TimelessJewel);

        if (PassiveSkillType == PassiveSkillType.Notable)
            randomNumberGenerator.Generate(0U, 100U);

        uint minimumAdditions = TimelessJewel.AlternateTreeVersion.MinimumAdditions;
        uint maximumAdditions = TimelessJewel.AlternateTreeVersion.MaximumAdditions;

        uint additionCountRoll = minimumAdditions;

        if (maximumAdditions > minimumAdditions)
            additionCountRoll = randomNumberGenerator.Generate(minimumAdditions, maximumAdditions);

        List<AlternatePassiveAdditionInformation> alternatePassiveAdditionInformations = new List<AlternatePassiveAdditionInformation>((int)additionCountRoll);

        for (uint i = 0; i < additionCountRoll; i++)
        {
            AlternatePassiveAddition rolledAlternatePassiveAddition = null;

            while (rolledAlternatePassiveAddition == null)
                rolledAlternatePassiveAddition = RollAlternatePassiveAddition(randomNumberGenerator);

            int rolledAlternatePassiveAdditionStatCount = Math.Min(rolledAlternatePassiveAddition.StatIndices.Count, 2);
            int[] alternatePassiveAdditionStatRolls = new int[rolledAlternatePassiveAdditionStatCount];
            if (rolledAlternatePassiveAdditionStatCount >= 1)
                alternatePassiveAdditionStatRolls[0] = RollStat(randomNumberGenerator, rolledAlternatePassiveAddition.StatAMinimumValue, rolledAlternatePassiveAddition.StatAMaximumValue);
            if (rolledAlternatePassiveAdditionStatCount >= 2)
                alternatePassiveAdditionStatRolls[1] = RollStat(randomNumberGenerator, rolledAlternatePassiveAddition.StatBMinimumValue, rolledAlternatePassiveAddition.StatBMaximumValue);
            alternatePassiveAdditionInformations.Add(new AlternatePassiveAdditionInformation(rolledAlternatePassiveAddition, alternatePassiveAdditionStatRolls));
        }

        return alternatePassiveAdditionInformations;
    }

    private AlternatePassiveAddition RollAlternatePassiveAddition(RandomNumberGenerator randomNumberGenerator)
    {
        ArgumentNullException.ThrowIfNull(randomNumberGenerator, nameof(randomNumberGenerator));
        uint additionRoll = randomNumberGenerator.Generate(ApplicableAlternatePassiveAdditionSpawnWeight);
        for (int i = 0; i < ApplicableAlternatePassiveAdditions.Count; i++)
        {
            AlternatePassiveAddition applicableAlternatePassiveAddition = ApplicableAlternatePassiveAdditions[i];
            if (applicableAlternatePassiveAddition.SpawnWeight > additionRoll)
                return applicableAlternatePassiveAddition;
            additionRoll -= applicableAlternatePassiveAddition.SpawnWeight;
        }
        return null;
    }

    private AlternatePassiveSkill RollAlternatePassiveSkill(RandomNumberGenerator randomNumberGenerator)
    {
        ArgumentNullException.ThrowIfNull(randomNumberGenerator, nameof(randomNumberGenerator));

        AlternatePassiveSkill rolledAlternatePassiveSkill = null;
        uint currentSpawnWeight = 0;

        for (int i = 0; i < ApplicableAlternatePassiveSkills.Count; i++)
        {
            AlternatePassiveSkill applicableAlternatePassiveSkill = ApplicableAlternatePassiveSkills[i];
            currentSpawnWeight += applicableAlternatePassiveSkill.SpawnWeight;
            uint roll = randomNumberGenerator.Generate(currentSpawnWeight);
            if (roll < applicableAlternatePassiveSkill.SpawnWeight)
                rolledAlternatePassiveSkill = applicableAlternatePassiveSkill;
        }

        return rolledAlternatePassiveSkill;
    }

    private static int RollStat(RandomNumberGenerator randomNumberGenerator, int minimumRoll, int maximumRoll)
    {
        if (maximumRoll <= minimumRoll)
            return minimumRoll;
        return minimumRoll + (int)randomNumberGenerator.Generate((uint)(maximumRoll - minimumRoll + 1));
    }
}

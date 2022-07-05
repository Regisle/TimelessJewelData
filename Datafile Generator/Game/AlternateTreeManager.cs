// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Game.AlternateTreeManager
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

using System;
using System.Collections.Generic;
using System.Linq;
using DatafileGenerator.Data;
using DatafileGenerator.Data.Models;
using DatafileGenerator.Random;

namespace DatafileGenerator.Game
{
    public class AlternateTreeManager
    {
        public PassiveSkill PassiveSkill { get; private set; }

        public TimelessJewel TimelessJewel { get; private set; }

        public AlternateTreeManager(PassiveSkill passiveSkill, TimelessJewel timelessJewel)
        {
            ArgumentNullException.ThrowIfNull((object)passiveSkill, nameof(passiveSkill));
            ArgumentNullException.ThrowIfNull((object)timelessJewel, nameof(timelessJewel));
            this.PassiveSkill = passiveSkill;
            this.TimelessJewel = timelessJewel;
        }

        public bool IsPassiveSkillReplaced()
        {
            if (this.PassiveSkill.IsKeyStone)
                return true;
            if (this.PassiveSkill.IsNotable)
                return this.TimelessJewel.AlternateTreeVersion.NotableReplacementSpawnWeight >= 100U || new RandomNumberGenerator(this.PassiveSkill, this.TimelessJewel).Generate(0U, 100U) < this.TimelessJewel.AlternateTreeVersion.NotableReplacementSpawnWeight;
            if (this.PassiveSkill.StatIndices.Count == 1)
            {
                uint num = (uint)((int)this.PassiveSkill.StatIndices.ElementAt<uint>(0) + 1 - 574);
                if (num <= 6U && (73 & 1 << (int)num) != 0)
                    return this.TimelessJewel.AlternateTreeVersion.AreSmallAttributePassiveSkillsReplaced;
            }
            return this.TimelessJewel.AlternateTreeVersion.AreSmallNormalPassiveSkillsReplaced;
        }

        public AlternatePassiveSkillInformation ReplacePassiveSkill()
        {
            if (this.PassiveSkill.IsKeyStone)
                return new AlternatePassiveSkillInformation(DataManager.GetAlternatePassiveSkillKeyStone(this.TimelessJewel), (IReadOnlyDictionary<uint, uint>)new Dictionary<uint, uint>(), (IReadOnlyCollection<AlternatePassiveAdditionInformation>)new List<AlternatePassiveAdditionInformation>());
            List<AlternatePassiveSkill> alternatePassiveSkills = DataManager.GetApplicableAlternatePassiveSkills(this.PassiveSkill, this.TimelessJewel);
            AlternatePassiveSkill alternatePassiveSkill1 = (AlternatePassiveSkill)null;
            RandomNumberGenerator randomNumberGenerator = new RandomNumberGenerator(this.PassiveSkill, this.TimelessJewel);
            uint exclusiveMaximumValue = 0;
            if (DataManager.GetPassiveSkillType(this.PassiveSkill) == PassiveSkillType.Notable)
            {
                int num1 = (int)randomNumberGenerator.Generate(0U, 100U);
            }
            foreach (AlternatePassiveSkill alternatePassiveSkill2 in alternatePassiveSkills)
            {
                exclusiveMaximumValue += alternatePassiveSkill2.SpawnWeight;
                if (randomNumberGenerator.Generate(exclusiveMaximumValue) < alternatePassiveSkill2.SpawnWeight)
                    alternatePassiveSkill1 = alternatePassiveSkill2;
            }
            Dictionary<uint, (uint, uint)> dictionary1 = new Dictionary<uint, (uint, uint)>()
      {
        {
          0U,
          (alternatePassiveSkill1.StatAMinimumValue, alternatePassiveSkill1.StatAMaximumValue)
        },
        {
          1U,
          (alternatePassiveSkill1.StatBMinimumValue, alternatePassiveSkill1.StatBMaximumValue)
        },
        {
          2U,
          (alternatePassiveSkill1.StatCMinimumValue, alternatePassiveSkill1.StatCMaximumValue)
        },
        {
          3U,
          (alternatePassiveSkill1.StatDMinimumValue, alternatePassiveSkill1.StatDMaximumValue)
        }
      };
            Dictionary<uint, uint> statRolls1 = new Dictionary<uint, uint>();
            for (uint key = 0; (long)key < (long)Math.Min(alternatePassiveSkill1.StatIndices.Count, 4); ++key)
            {
                uint num2 = dictionary1[key].Item1;
                if (dictionary1[key].Item2 > dictionary1[key].Item1)
                    num2 = randomNumberGenerator.Generate(dictionary1[key].Item1, dictionary1[key].Item2);
                statRolls1.Add(key, num2);
            }
            if (alternatePassiveSkill1.MinimumAdditions == 0U && alternatePassiveSkill1.MaximumAdditions == 0U)
                return new AlternatePassiveSkillInformation(alternatePassiveSkill1, (IReadOnlyDictionary<uint, uint>)statRolls1, (IReadOnlyCollection<AlternatePassiveAdditionInformation>)new List<AlternatePassiveAdditionInformation>());
            uint minimumValue = this.TimelessJewel.AlternateTreeVersion.MinimumAdditions + alternatePassiveSkill1.MinimumAdditions;
            uint maximumValue = this.TimelessJewel.AlternateTreeVersion.MaximumAdditions + alternatePassiveSkill1.MaximumAdditions;
            uint num3 = minimumValue;
            if (maximumValue > minimumValue)
                num3 = randomNumberGenerator.Generate(minimumValue, maximumValue);
            List<AlternatePassiveAdditionInformation> additionInformationList = new List<AlternatePassiveAdditionInformation>();
            for (int index = 0; (long)index < (long)num3; ++index)
            {
                AlternatePassiveAddition rolledAlternatePassiveAddition = (AlternatePassiveAddition)null;
                while (rolledAlternatePassiveAddition == null || additionInformationList.Any<AlternatePassiveAdditionInformation>((Func<AlternatePassiveAdditionInformation, bool>)(q => q.AlternatePassiveAddition == rolledAlternatePassiveAddition)))
                    rolledAlternatePassiveAddition = this.RollAlternatePassiveAddition(randomNumberGenerator);
                Dictionary<uint, (uint, uint)> dictionary2 = new Dictionary<uint, (uint, uint)>()
        {
          {
            0U,
            (rolledAlternatePassiveAddition.StatAMinimumValue, rolledAlternatePassiveAddition.StatAMaximumValue)
          },
          {
            1U,
            (rolledAlternatePassiveAddition.StatBMinimumValue, rolledAlternatePassiveAddition.StatBMaximumValue)
          }
        };
                Dictionary<uint, uint> statRolls2 = new Dictionary<uint, uint>();
                for (uint key = 0; (long)key < (long)Math.Min(rolledAlternatePassiveAddition.StatIndices.Count, 2); ++key)
                {
                    uint num4 = dictionary2[key].Item1;
                    if (dictionary2[key].Item2 > dictionary2[key].Item1)
                        num4 = randomNumberGenerator.Generate(dictionary2[key].Item1, dictionary2[key].Item2);
                    statRolls2.Add(key, num4);
                }
                additionInformationList.Add(new AlternatePassiveAdditionInformation(rolledAlternatePassiveAddition, (IReadOnlyDictionary<uint, uint>)statRolls2));
            }
            return new AlternatePassiveSkillInformation(alternatePassiveSkill1, (IReadOnlyDictionary<uint, uint>)statRolls1, (IReadOnlyCollection<AlternatePassiveAdditionInformation>)additionInformationList);
        }

        public IReadOnlyCollection<AlternatePassiveAdditionInformation> AugmentPassiveSkill()
        {
            RandomNumberGenerator randomNumberGenerator = new RandomNumberGenerator(this.PassiveSkill, this.TimelessJewel);
            if (DataManager.GetPassiveSkillType(this.PassiveSkill) == PassiveSkillType.Notable)
            {
                int num1 = (int)randomNumberGenerator.Generate(0U, 100U);
            }
            uint minimumAdditions = this.TimelessJewel.AlternateTreeVersion.MinimumAdditions;
            uint maximumAdditions = this.TimelessJewel.AlternateTreeVersion.MaximumAdditions;
            uint num2 = minimumAdditions;
            if (maximumAdditions > minimumAdditions)
                num2 = randomNumberGenerator.Generate(minimumAdditions, maximumAdditions);
            List<AlternatePassiveAdditionInformation> source = new List<AlternatePassiveAdditionInformation>();
            for (int index = 0; (long)index < (long)num2; ++index)
            {
                AlternatePassiveAddition rolledAlternatePassiveAddition = (AlternatePassiveAddition)null;
                while (rolledAlternatePassiveAddition == null || source.Any<AlternatePassiveAdditionInformation>((Func<AlternatePassiveAdditionInformation, bool>)(q => q.AlternatePassiveAddition == rolledAlternatePassiveAddition)))
                    rolledAlternatePassiveAddition = this.RollAlternatePassiveAddition(randomNumberGenerator);
                Dictionary<uint, (uint, uint)> dictionary = new Dictionary<uint, (uint, uint)>()
        {
          {
            0U,
            (rolledAlternatePassiveAddition.StatAMinimumValue, rolledAlternatePassiveAddition.StatAMaximumValue)
          },
          {
            1U,
            (rolledAlternatePassiveAddition.StatBMinimumValue, rolledAlternatePassiveAddition.StatBMaximumValue)
          }
        };
                Dictionary<uint, uint> statRolls = new Dictionary<uint, uint>();
                for (uint key = 0; (long)key < (long)Math.Min(rolledAlternatePassiveAddition.StatIndices.Count, 2); ++key)
                {
                    uint num3 = dictionary[key].Item1;
                    if (dictionary[key].Item2 > dictionary[key].Item1)
                        num3 = randomNumberGenerator.Generate(dictionary[key].Item1, dictionary[key].Item2);
                    statRolls.Add(key, num3);
                }
                source.Add(new AlternatePassiveAdditionInformation(rolledAlternatePassiveAddition, (IReadOnlyDictionary<uint, uint>)statRolls));
            }
            return (IReadOnlyCollection<AlternatePassiveAdditionInformation>)source;
        }

        private AlternatePassiveAddition RollAlternatePassiveAddition(
          RandomNumberGenerator randomNumberGenerator)
        {
            ArgumentNullException.ThrowIfNull((object)randomNumberGenerator, nameof(randomNumberGenerator));
            List<AlternatePassiveAddition> passiveAdditions = DataManager.GetApplicableAlternatePassiveAdditions(this.PassiveSkill, this.TimelessJewel);
            uint exclusiveMaximumValue = (uint)passiveAdditions.Sum<AlternatePassiveAddition>((Func<AlternatePassiveAddition, long>)(q => (long)q.SpawnWeight));
            uint num = randomNumberGenerator.Generate(exclusiveMaximumValue);
            foreach (AlternatePassiveAddition alternatePassiveAddition in passiveAdditions)
            {
                if (alternatePassiveAddition.SpawnWeight > num)
                    return alternatePassiveAddition;
                num -= alternatePassiveAddition.SpawnWeight;
            }
            return (AlternatePassiveAddition)null;
        }
    }
}

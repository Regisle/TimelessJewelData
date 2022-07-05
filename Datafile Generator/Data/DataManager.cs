// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Data.DataManager
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DatafileGenerator.Data.Models;
using DatafileGenerator.Game;

namespace DatafileGenerator.Data
{
  public static class DataManager
  {
    public static IReadOnlyCollection<AlternatePassiveAddition> AlternatePassiveAdditions { get; private set; }

    public static IReadOnlyCollection<AlternatePassiveSkill> AlternatePassiveSkills { get; private set; }

    public static IReadOnlyCollection<AlternateTreeVersion> AlternateTreeVersions { get; private set; }

    public static IReadOnlyCollection<PassiveSkill> PassiveSkills { get; private set; }

    static DataManager()
    {
      DataManager.AlternatePassiveAdditions = (IReadOnlyCollection<AlternatePassiveAddition>) null;
      DataManager.AlternatePassiveSkills = (IReadOnlyCollection<AlternatePassiveSkill>) null;
      DataManager.AlternateTreeVersions = (IReadOnlyCollection<AlternateTreeVersion>) null;
      DataManager.PassiveSkills = (IReadOnlyCollection<PassiveSkill>) null;
    }

    public static bool Initialize()
    {
      DataManager.AlternatePassiveAdditions = DataManager.LoadFromFile<AlternatePassiveAddition>(Settings.AlternatePassiveAdditionsFilePath);
      DataManager.AlternatePassiveSkills = DataManager.LoadFromFile<AlternatePassiveSkill>(Settings.AlternatePassiveSkillsFilePath);
      DataManager.AlternateTreeVersions = DataManager.LoadFromFile<AlternateTreeVersion>(Settings.AlternateTreeVersionsFilePath);
      DataManager.PassiveSkills = DataManager.LoadFromFile<PassiveSkill>(Settings.PassiveSkillsFilePath);
      return DataManager.AlternatePassiveAdditions != null && DataManager.AlternatePassiveSkills != null && DataManager.AlternateTreeVersions != null && DataManager.PassiveSkills != null;
    }

    public static List<AlternatePassiveAddition> GetApplicableAlternatePassiveAdditions(
      PassiveSkill passiveSkill,
      TimelessJewel timelessJewel)
    {
      ArgumentNullException.ThrowIfNull((object) passiveSkill, nameof (passiveSkill));
      ArgumentNullException.ThrowIfNull((object) timelessJewel, nameof (timelessJewel));
      List<AlternatePassiveAddition> passiveAdditions = new List<AlternatePassiveAddition>();
      foreach (AlternatePassiveAddition alternatePassiveAddition in (IEnumerable<AlternatePassiveAddition>) DataManager.AlternatePassiveAdditions)
      {
        PassiveSkillType passiveSkillType = DataManager.GetPassiveSkillType(passiveSkill);
        if ((int) alternatePassiveAddition.AlternateTreeVersionIndex == (int) timelessJewel.AlternateTreeVersion.Index && alternatePassiveAddition.ApplicablePassiveTypes.Any<uint>((Func<uint, bool>) (q => (PassiveSkillType) q == passiveSkillType)))
          passiveAdditions.Add(alternatePassiveAddition);
      }
      return passiveAdditions;
    }

    public static AlternatePassiveSkill GetAlternatePassiveSkillKeyStone(
      TimelessJewel timelessJewel)
    {
      ArgumentNullException.ThrowIfNull((object) timelessJewel, nameof (timelessJewel));
      AlternatePassiveSkill alternatePassiveSkill = DataManager.AlternatePassiveSkills.FirstOrDefault<AlternatePassiveSkill>((Func<AlternatePassiveSkill, bool>) (q => (int) q.AlternateTreeVersionIndex == (int) timelessJewel.AlternateTreeVersion.Index && (int) q.ConquererIndex == (int) timelessJewel.TimelessJewelConquerer.Index && (int) q.ConquererVersion == (int) timelessJewel.TimelessJewelConquerer.Version));
      return !alternatePassiveSkill.ApplicablePassiveTypes.Any<uint>((Func<uint, bool>) (q => q == 4U)) ? (AlternatePassiveSkill) null : alternatePassiveSkill;
    }

    public static List<AlternatePassiveSkill> GetApplicableAlternatePassiveSkills(
      PassiveSkill passiveSkill,
      TimelessJewel timelessJewel)
    {
      ArgumentNullException.ThrowIfNull((object) passiveSkill, nameof (passiveSkill));
      ArgumentNullException.ThrowIfNull((object) timelessJewel, nameof (timelessJewel));
      List<AlternatePassiveSkill> alternatePassiveSkills = new List<AlternatePassiveSkill>();
      foreach (AlternatePassiveSkill alternatePassiveSkill in (IEnumerable<AlternatePassiveSkill>) DataManager.AlternatePassiveSkills)
      {
        PassiveSkillType passiveSkillType = DataManager.GetPassiveSkillType(passiveSkill);
        if ((int) alternatePassiveSkill.AlternateTreeVersionIndex == (int) timelessJewel.AlternateTreeVersion.Index && alternatePassiveSkill.ApplicablePassiveTypes.Any<uint>((Func<uint, bool>) (q => (PassiveSkillType) q == passiveSkillType)))
          alternatePassiveSkills.Add(alternatePassiveSkill);
      }
      return alternatePassiveSkills;
    }

    public static PassiveSkill GetPassiveSkillByFuzzyValue(string fuzzyValue)
    {
      ArgumentNullException.ThrowIfNull((object) fuzzyValue, nameof (fuzzyValue));
      return DataManager.PassiveSkills == null ? (PassiveSkill) null : DataManager.PassiveSkills.FirstOrDefault<PassiveSkill>((Func<PassiveSkill, bool>) (q => q.Identifier.ToLowerInvariant() == fuzzyValue.ToLowerInvariant() || q.Name.ToLowerInvariant() == fuzzyValue.ToLowerInvariant()));
    }

    public static PassiveSkillType GetPassiveSkillType(PassiveSkill passiveSkill)
    {
      ArgumentNullException.ThrowIfNull((object) passiveSkill, nameof (passiveSkill));
      if (passiveSkill.IsJewelSocket)
        return PassiveSkillType.JewelSocket;
      if (passiveSkill.IsKeyStone)
        return PassiveSkillType.KeyStone;
      if (passiveSkill.IsNotable)
        return PassiveSkillType.Notable;
      if (passiveSkill.StatIndices.Count == 1)
      {
        uint num = (uint) ((int) passiveSkill.StatIndices.ElementAt<uint>(0) + 1 - 574);
        if (num <= 6U && (73 & 1 << (int) num) != 0)
          return PassiveSkillType.SmallAttribute;
      }
      return PassiveSkillType.SmallNormal;
    }

    public static bool IsPassiveSkillValidForAlteration(PassiveSkill passiveSkill)
    {
      ArgumentNullException.ThrowIfNull((object) passiveSkill, nameof (passiveSkill));
      PassiveSkillType passiveSkillType = DataManager.GetPassiveSkillType(passiveSkill);
      return passiveSkillType != PassiveSkillType.None && passiveSkillType != PassiveSkillType.JewelSocket;
    }

    private static IReadOnlyCollection<T> LoadFromFile<T>(string filePath)
    {
      ArgumentNullException.ThrowIfNull((object) filePath, nameof (filePath));
      if (!File.Exists(filePath))
        return (IReadOnlyCollection<T>) null;
      using (FileStream utf8Json = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        return JsonSerializer.Deserialize<IReadOnlyCollection<T>>((Stream) utf8Json);
    }
  }
}

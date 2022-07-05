// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Game.AlternatePassiveSkillInformation
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

using System;
using System.Collections.Generic;
using DatafileGenerator.Data.Models;

namespace DatafileGenerator.Game
{
  public class AlternatePassiveSkillInformation
  {
    public AlternatePassiveSkill AlternatePassiveSkill { get; private set; }

    public IReadOnlyDictionary<uint, uint> StatRolls { get; private set; }

    public IReadOnlyCollection<AlternatePassiveAdditionInformation> AlternatePassiveAdditionInformations { get; private set; }

    public AlternatePassiveSkillInformation(
      AlternatePassiveSkill alternatePassiveSkill,
      IReadOnlyDictionary<uint, uint> statRolls,
      IReadOnlyCollection<AlternatePassiveAdditionInformation> alternatePassiveAdditionInformations)
    {
      ArgumentNullException.ThrowIfNull((object) alternatePassiveSkill, nameof (alternatePassiveSkill));
      ArgumentNullException.ThrowIfNull((object) statRolls, nameof (statRolls));
      ArgumentNullException.ThrowIfNull((object) alternatePassiveAdditionInformations, nameof (alternatePassiveAdditionInformations));
      this.AlternatePassiveSkill = alternatePassiveSkill;
      this.StatRolls = statRolls;
      this.AlternatePassiveAdditionInformations = alternatePassiveAdditionInformations;
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Game.AlternatePassiveAdditionInformation
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

using System;
using System.Collections.Generic;
using DatafileGenerator.Data.Models;

namespace DatafileGenerator.Game
{
  public class AlternatePassiveAdditionInformation
  {
    public AlternatePassiveAddition AlternatePassiveAddition { get; private set; }

    public IReadOnlyDictionary<uint, uint> StatRolls { get; private set; }

    public AlternatePassiveAdditionInformation(
      AlternatePassiveAddition alternatePassiveAddition,
      IReadOnlyDictionary<uint, uint> statRolls)
    {
      ArgumentNullException.ThrowIfNull((object) alternatePassiveAddition, nameof (alternatePassiveAddition));
      this.AlternatePassiveAddition = alternatePassiveAddition;
      this.StatRolls = statRolls;
    }
  }
}

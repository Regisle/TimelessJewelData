// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Game.TimelessJewel
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

using System;
using DatafileGenerator.Data.Models;

namespace DatafileGenerator.Game
{
  public class TimelessJewel
  {
    public AlternateTreeVersion AlternateTreeVersion { get; private set; }

    public TimelessJewelConquerer TimelessJewelConquerer { get; private set; }

    public uint Seed { get; private set; }

    public TimelessJewel(
      AlternateTreeVersion alternateTreeVersion,
      TimelessJewelConquerer timelessJewelConquerer,
      uint seed)
    {
      ArgumentNullException.ThrowIfNull((object) alternateTreeVersion, nameof (alternateTreeVersion));
      ArgumentNullException.ThrowIfNull((object) timelessJewelConquerer, nameof (timelessJewelConquerer));
      this.AlternateTreeVersion = alternateTreeVersion;
      this.TimelessJewelConquerer = timelessJewelConquerer;
      this.Seed = seed;
      this.TransformSeed();
    }

    private void TransformSeed()
    {
      if (this.AlternateTreeVersion.Index != 5U)
        return;
      this.Seed /= 20U;
    }
  }
}

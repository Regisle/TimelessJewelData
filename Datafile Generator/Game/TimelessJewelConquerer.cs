// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Game.TimelessJewelConquerer
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

namespace DatafileGenerator.Game
{
  public class TimelessJewelConquerer
  {
    public uint Index { get; private set; }

    public uint Version { get; private set; }

    public TimelessJewelConquerer(uint index, uint version)
    {
      this.Index = index;
      this.Version = version;
    }
  }
}

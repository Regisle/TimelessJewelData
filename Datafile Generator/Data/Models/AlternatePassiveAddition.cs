// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Data.Models.AlternatePassiveAddition
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DatafileGenerator.Data.Models
{
  public class AlternatePassiveAddition
  {
    [JsonPropertyName("_rid")]
    public uint Index { get; init; }

    [JsonPropertyName("Id")]
    public string Identifier { get; init; }

    [JsonPropertyName("AlternateTreeVersionsKey")]
    public uint AlternateTreeVersionIndex { get; init; }

    [JsonPropertyName("StatsKeys")]
    public IReadOnlyCollection<uint> StatIndices { get; init; }

    [JsonPropertyName("Stat1Min")]
    public uint StatAMinimumValue { get; init; }

    [JsonPropertyName("Stat1Max")]
    public uint StatAMaximumValue { get; init; }

    [JsonPropertyName("Unknown7")]
    public uint StatBMinimumValue { get; init; }

    [JsonPropertyName("Unknown8")]
    public uint StatBMaximumValue { get; init; }

    [JsonPropertyName("PassiveType")]
    public IReadOnlyCollection<uint> ApplicablePassiveTypes { get; init; }

    [JsonPropertyName("SpawnWeight")]
    public uint SpawnWeight { get; init; }

    public AlternatePassiveAddition()
    {
      this.Index = 0U;
      this.Identifier = (string) null;
      this.AlternateTreeVersionIndex = 0U;
      this.StatIndices = (IReadOnlyCollection<uint>) null;
      this.StatAMinimumValue = 0U;
      this.StatAMaximumValue = 0U;
      this.StatBMinimumValue = 0U;
      this.StatBMaximumValue = 0U;
      this.ApplicablePassiveTypes = (IReadOnlyCollection<uint>) null;
      this.SpawnWeight = 0U;
    }
  }
}

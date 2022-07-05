// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Data.Models.AlternatePassiveSkill
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DatafileGenerator.Data.Models
{
  public class AlternatePassiveSkill
  {
    [JsonPropertyName("_rid")]
    public uint Index { get; init; }

    [JsonPropertyName("Id")]
    public string Identifier { get; init; }

    [JsonPropertyName("Name")]
    public string Name { get; init; }

    [JsonPropertyName("AlternateTreeVersionsKey")]
    public uint AlternateTreeVersionIndex { get; init; }

    [JsonPropertyName("StatsKeys")]
    public IReadOnlyCollection<uint> StatIndices { get; init; }

    [JsonPropertyName("Stat1Min")]
    public uint StatAMinimumValue { get; init; }

    [JsonPropertyName("Stat1Max")]
    public uint StatAMaximumValue { get; init; }

    [JsonPropertyName("Stat2Min")]
    public uint StatBMinimumValue { get; init; }

    [JsonPropertyName("Stat2Max")]
    public uint StatBMaximumValue { get; init; }

    [JsonPropertyName("Unknown10")]
    public uint StatCMinimumValue { get; init; }

    [JsonPropertyName("Unknown11")]
    public uint StatCMaximumValue { get; init; }

    [JsonPropertyName("Unknown12")]
    public uint StatDMinimumValue { get; init; }

    [JsonPropertyName("Unknown13")]
    public uint StatDMaximumValue { get; init; }

    [JsonPropertyName("PassiveType")]
    public IReadOnlyCollection<uint> ApplicablePassiveTypes { get; init; }

    [JsonPropertyName("SpawnWeight")]
    public uint SpawnWeight { get; init; }

    [JsonPropertyName("RandomMin")]
    public uint MinimumAdditions { get; init; }

    [JsonPropertyName("RandomMax")]
    public uint MaximumAdditions { get; init; }

    [JsonPropertyName("Unknown19")]
    public uint ConquererIndex { get; init; }

    [JsonPropertyName("Unknown25")]
    public uint ConquererVersion { get; init; }

    public AlternatePassiveSkill()
    {
      this.Index = 0U;
      this.Identifier = (string) null;
      this.Name = (string) null;
      this.AlternateTreeVersionIndex = 0U;
      this.StatIndices = (IReadOnlyCollection<uint>) null;
      this.StatAMinimumValue = 0U;
      this.StatAMaximumValue = 0U;
      this.StatBMinimumValue = 0U;
      this.StatBMaximumValue = 0U;
      this.StatCMinimumValue = 0U;
      this.StatCMaximumValue = 0U;
      this.StatDMinimumValue = 0U;
      this.StatDMaximumValue = 0U;
      this.ApplicablePassiveTypes = (IReadOnlyCollection<uint>) null;
      this.SpawnWeight = 0U;
      this.MinimumAdditions = 0U;
      this.MaximumAdditions = 0U;
      this.ConquererIndex = 0U;
      this.ConquererVersion = 0U;
    }
  }
}

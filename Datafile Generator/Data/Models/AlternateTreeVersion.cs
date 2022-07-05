// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Data.Models.AlternateTreeVersion
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

using System.Text.Json.Serialization;

namespace DatafileGenerator.Data.Models
{
  public class AlternateTreeVersion
  {
    [JsonPropertyName("_rid")]
    public uint Index { get; init; }

    [JsonPropertyName("Id")]
    public string Identifier { get; init; }

    [JsonPropertyName("Unknown2")]
    public bool AreSmallAttributePassiveSkillsReplaced { get; init; }

    [JsonPropertyName("Unknown3")]
    public bool AreSmallNormalPassiveSkillsReplaced { get; init; }

    [JsonPropertyName("Unknown6")]
    public uint MinimumAdditions { get; init; }

    [JsonPropertyName("Unknown7")]
    public uint MaximumAdditions { get; init; }

    [JsonPropertyName("Unknown10")]
    public uint NotableReplacementSpawnWeight { get; init; }

    public AlternateTreeVersion()
    {
      this.Index = 0U;
      this.Identifier = (string) null;
      this.AreSmallAttributePassiveSkillsReplaced = false;
      this.AreSmallNormalPassiveSkillsReplaced = false;
      this.MinimumAdditions = 0U;
      this.MaximumAdditions = 0U;
      this.NotableReplacementSpawnWeight = 0U;
    }
  }
}

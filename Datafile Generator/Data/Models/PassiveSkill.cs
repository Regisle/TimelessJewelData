// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Data.Models.PassiveSkill
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DatafileGenerator.Data.Models
{
  public class PassiveSkill
  {
    [JsonPropertyName("_rid")]
    public uint Index { get; init; }

    [JsonPropertyName("Id")]
    public string Identifier { get; init; }

    [JsonPropertyName("PassiveSkillGraphId")]
    public uint GraphIdentifier { get; init; }

    [JsonPropertyName("Name")]
    public string Name { get; init; }

    [JsonPropertyName("Stats")]
    public IReadOnlyCollection<uint> StatIndices { get; init; }

    [JsonPropertyName("IsJewelSocket")]
    public bool IsJewelSocket { get; init; }

    [JsonPropertyName("IsNotable")]
    public bool IsNotable { get; init; }

    [JsonPropertyName("IsKeystone")]
    public bool IsKeyStone { get; init; }

    public PassiveSkill()
    {
      this.Index = 0U;
      this.Identifier = (string) null;
      this.GraphIdentifier = 0U;
      this.Name = (string) null;
      this.StatIndices = (IReadOnlyCollection<uint>) null;
      this.IsJewelSocket = false;
      this.IsNotable = false;
      this.IsKeyStone = false;
    }
  }
}

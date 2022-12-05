using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DatafileGenerator.Data.Models;

public class PassiveSkill : IComparable<PassiveSkill>
{
    [JsonPropertyName("skill")]
    public uint GraphIdentifier { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("stats")]
    public IReadOnlyCollection<string> StatStrings { get; init; }

    [JsonPropertyName("isBlighted")]
    public bool IsBlight { get; init; }

    [JsonPropertyName("isJewelSocket")]
    public bool IsJewelSocket { get; init; }

    [JsonPropertyName("isNotable")]
    public bool IsNotable { get; init; }

    [JsonPropertyName("isKeystone")]
    public bool IsKeyStone { get; init; }

    [JsonPropertyName("isMastery")]
    public bool IsMastery { get; init; }

    [JsonPropertyName("isProxy")]
    public bool IsProxy { get; init; }

    [JsonPropertyName("ascendancyName")]
    public string AscName { get; init; }
    public bool IsAscendancy => !string.IsNullOrEmpty(AscName);

    [JsonPropertyName("orbit")]
    public uint? Orbit { get; init; }
    public bool IsCluster => Orbit == null;
    public bool IsAttribute => 
        StatStrings != null && StatStrings.Count == 1 &&
        (StatStrings.First() == "+10 to Strength" ||
         StatStrings.First() == "+10 to Dexterity" ||
         StatStrings.First() == "+10 to Intelligence");
    public bool IsModifiable => !(IsCluster || IsAscendancy || IsProxy || IsMastery || IsKeyStone || IsJewelSocket || IsBlight);

    public int CompareTo(PassiveSkill other)
    {
        if (other is null)
        {
            return -1;
        }
        return GraphIdentifier.CompareTo(other.GraphIdentifier);
    }
}

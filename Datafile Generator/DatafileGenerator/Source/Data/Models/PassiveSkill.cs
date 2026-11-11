using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DatafileGenerator.Data.Models;

public class PassiveSkill : IComparable<PassiveSkill>
{
    [JsonPropertyName("skill")]
    public uint GraphIdentifier { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("stats")]
    public IReadOnlyCollection<string> StatStrings { get; init; }

    [JsonPropertyName("out")]
    public IReadOnlyList<string> OutConnections { get; init; }

    [JsonPropertyName("in")]
    public IReadOnlyList<string> InConnections { get; init; }

    [JsonPropertyName("classStartIndex")]
    public uint? ClassStartIndex { get; init; }

    [JsonPropertyName("group")]
    public uint? Group { get; init; }

    [JsonPropertyName("isJustIcon")]
    public bool IsJustIcon { get; init; }

    [JsonPropertyName("isBlighted")]
    public bool IsBlight { get; init; }

    [JsonPropertyName("isJewelSocket")]
    public bool IsJewelSocket { get; init; }

    [JsonPropertyName("expansionJewel")]
    public JsonElement? ExpansionJewel { get; init; }

    [JsonPropertyName("isNotable")]
    public bool IsNotable { get; init; }

    [JsonPropertyName("isKeystone")]
    public bool IsKeyStone { get; init; }

    [JsonPropertyName("isMastery")]
    public bool IsMastery { get; init; }

    [JsonPropertyName("isProxy")]
    public bool IsProxy { get; init; }

    [JsonPropertyName("isAscendancyStart")]
    public bool IsAscendancyStart { get; init; }

    [JsonPropertyName("isMultipleChoice")]
    public bool IsMultipleChoice { get; init; }

    [JsonPropertyName("isMultipleChoiceOption")]
    public bool IsMultipleChoiceOption { get; init; }

    [JsonPropertyName("ascendancyName")]
    public string AscName { get; init; }
    public bool IsAscendancy => !string.IsNullOrEmpty(AscName);

    public bool IsCharacterStart => ClassStartIndex.HasValue;

    public bool IsExpansionJewelSocket => ExpansionJewel.HasValue;

    public bool IsClusterExpansionSocket =>
        ExpansionJewel.HasValue &&
        ExpansionJewel.Value.ValueKind == JsonValueKind.Object &&
        ExpansionJewel.Value.TryGetProperty("parent", out _);

    [JsonPropertyName("orbit")]
    public uint? Orbit { get; init; }

    [JsonPropertyName("orbitIndex")]
    public uint? OrbitIndex { get; init; }
    public bool IsCluster => Orbit == null;
    public bool IsAttribute =>
        StatStrings != null && StatStrings.Count == 1 &&
        Regex.IsMatch(StatStrings.First(), @"^\+\d+ to (Strength|Dexterity|Intelligence)$");
    public bool IsModifiable => !(IsCluster || IsAscendancy || IsProxy || IsMastery || IsKeyStone || IsJewelSocket || IsBlight);

    public bool IsAbyssTransformable =>
        !(IsCluster || IsAscendancy || IsProxy || IsMastery || IsJewelSocket || IsBlight || IsCharacterStart || IsJustIcon);

    public int CompareTo(PassiveSkill other)
    {
        if (other is null)
        {
            return -1;
        }
        return GraphIdentifier.CompareTo(other.GraphIdentifier);
    }
}

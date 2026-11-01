using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DatafileGenerator.Data.Models;

public class AlternatePassiveSkill
{
    [JsonPropertyName("Id")]
    public string Id { get; init; }

    [JsonPropertyName("Name")]
    public string Name { get; init; }

    [JsonPropertyName("_rid")]
    public uint Index { get; init; }

    [JsonPropertyName("AlternateTreeVersionsKey")]
    public uint AlternateTreeVersionIndex { get; init; }

    [JsonPropertyName("StatsKeys")]
    public IReadOnlyCollection<uint> StatIndices { get; init; }

    [JsonPropertyName("Stat1Min")]
    public int StatAMinimumValue { get; init; }

    [JsonPropertyName("Stat1Max")]
    public int StatAMaximumValue { get; init; }

    [JsonPropertyName("Stat2Min")]
    public int StatBMinimumValue { get; init; }

    [JsonPropertyName("Stat2Max")]
    public int StatBMaximumValue { get; init; }

    [JsonPropertyName("Unknown10")]
    public int StatCMinimumValue { get; init; }

    [JsonPropertyName("Unknown11")]
    public int StatCMaximumValue { get; init; }

    [JsonPropertyName("Unknown12")]
    public int StatDMinimumValue { get; init; }

    [JsonPropertyName("Unknown13")]
    public int StatDMaximumValue { get; init; }

    [JsonPropertyName("PassiveType")]
    public IReadOnlyCollection<uint> ApplicablePassiveTypes { get; init; }

    [JsonPropertyName("SpawnWeight")]
    public uint SpawnWeight { get; init; }

    [JsonPropertyName("RandomMin")]
    public uint MinimumAdditions { get; init; }

    [JsonPropertyName("RandomMax")]
    public uint MaximumAdditions { get; init; }
}

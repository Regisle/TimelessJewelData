using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DatafileGenerator.Data.Models;

public class AlternatePassiveSkill
{
    [JsonPropertyName("_rid")]
    public uint Index { get; init; }

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
}

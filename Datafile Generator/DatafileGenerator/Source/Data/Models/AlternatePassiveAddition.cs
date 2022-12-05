using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DatafileGenerator.Data.Models;

public class AlternatePassiveAddition
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

    [JsonPropertyName("Unknown7")]
    public uint StatBMinimumValue { get; init; }

    [JsonPropertyName("Unknown8")]
    public uint StatBMaximumValue { get; init; }

    [JsonPropertyName("PassiveType")]
    public IReadOnlyCollection<uint> ApplicablePassiveTypes { get; init; }

    [JsonPropertyName("SpawnWeight")]
    public uint SpawnWeight { get; init; }
}

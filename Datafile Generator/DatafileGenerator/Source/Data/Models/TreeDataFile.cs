using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DatafileGenerator.Data.Models
{
    public class SkillGroup
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public class SkillTreeConstants
    {
        [JsonPropertyName("skillsPerOrbit")]
        public int[] SkillsPerOrbit { get; set; }

        [JsonPropertyName("orbitRadii")]
        public int[] OrbitRadii { get; set; }
    }

    public class TreeDataFile
    {
        [JsonPropertyName("nodes")]
        public Dictionary<string, PassiveSkill> PassiveSkills { get; set; }

        [JsonPropertyName("groups")]
        public Dictionary<int, SkillGroup> Groups { get; set; }

        [JsonPropertyName("min_x")]
        public double MinX { get; set; }

        [JsonPropertyName("min_y")]
        public double MinY { get; set; }

        [JsonPropertyName("constants")]
        public SkillTreeConstants Constants { get; set; }

        [JsonPropertyName("jewelSlots")]
        public IReadOnlyList<uint> JewelSlots { get; set; }
    }
}

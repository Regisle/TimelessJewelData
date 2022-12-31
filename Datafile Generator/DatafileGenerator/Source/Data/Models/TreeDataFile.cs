using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DatafileGenerator.Data.Models
{
    public class TreeDataFile
    {
        [JsonPropertyName("nodes")]
        public Dictionary<string, PassiveSkill> PassiveSkills { get; set; }
    }
}

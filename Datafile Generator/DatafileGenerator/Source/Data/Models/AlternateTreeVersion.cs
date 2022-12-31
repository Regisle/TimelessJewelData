namespace DatafileGenerator.Data.Models;
public class AlternateTreeVersion
{
    public uint Index { get; init; }
    public bool AreSmallAttributePassiveSkillsReplaced => Index switch
    {
        1 => true,
        2 => false,
        3 => false,
        4 => true,
        5 => true,
        _ => false
    };
    public bool AreSmallNormalPassiveSkillsReplaced => Index switch
    {
        1 => true,
        2 => false,
        3 => false,
        4 => false,
        5 => true,
        _ => false
    };
    public uint MinimumAdditions => Index switch
    {
        1 => 0,
        2 => 1,
        3 => 1,
        4 => 1,
        5 => 0,
        _ => 0
    };
    public uint MaximumAdditions => Index switch
    {
        1 => 0,
        2 => 1,
        3 => 1,
        4 => 1,
        5 => 0,
        _ => 0
    };
    public uint NotableReplacementSpawnWeight => Index switch
    {
        1 => 100,
        2 => 0,
        3 => 0,
        4 => 20,
        5 => 100,
        _ => 0
    };

    public AlternateTreeVersion(uint index)
    {
        Index = index;
    }
}

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
        6 => false,
        7 => true,
        8 => true,
        9 => true,
        10 => true,
        11 => true,
        _ => false
    };
    public bool AreSmallNormalPassiveSkillsReplaced => Index switch
    {
        1 => true,
        2 => false,
        3 => false,
        4 => false,
        5 => true,
        6 => false,
        7 => true,
        8 => true,
        9 => true,
        10 => true,
        11 => true,
        _ => false
    };
    public uint MinimumAdditions => Index switch
    {
        1 => 0,
        2 => 1,
        3 => 1,
        4 => 1,
        5 => 0,
        6 => 1,
        7 => 1,
        8 => 1,
        9 => 1,
        10 => 1,
        11 => 1,
        _ => 0
    };
    public uint MaximumAdditions => Index switch
    {
        1 => 0,
        2 => 1,
        3 => 1,
        4 => 1,
        5 => 0,
        6 => 1,
        7 => 1,
        8 => 1,
        9 => 1,
        10 => 1,
        11 => 1,
        _ => 0
    };
    public uint NotableReplacementSpawnWeight => Index switch
    {
        1 => 100,
        2 => 0,
        3 => 0,
        4 => 20,
        5 => 100,
        6 => 100,
        _ => 0
    };

    public AlternateTreeVersion(uint index)
    {
        Index = index;
    }
}

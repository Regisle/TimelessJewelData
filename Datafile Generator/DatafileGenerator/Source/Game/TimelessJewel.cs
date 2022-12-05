using System;
using DatafileGenerator.Data.Models;

namespace DatafileGenerator.Game;

public class TimelessJewel
{
    public AlternateTreeVersion AlternateTreeVersion { get; private set; }

    public uint Seed { get; private set; }

    public TimelessJewel(AlternateTreeVersion alternateTreeVersion, uint seed)
    {
        ArgumentNullException.ThrowIfNull(alternateTreeVersion, nameof(alternateTreeVersion));
        AlternateTreeVersion = alternateTreeVersion;
        Seed = seed;
        if (AlternateTreeVersion.Index == 5)
        {
            Seed /= 20;
        }
    }
}

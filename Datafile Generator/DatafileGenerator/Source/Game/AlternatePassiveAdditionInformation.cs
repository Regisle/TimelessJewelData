using System;
using System.Collections.Generic;
using DatafileGenerator.Data.Models;

namespace DatafileGenerator.Game;

public class AlternatePassiveAdditionInformation
{
    public AlternatePassiveAddition AlternatePassiveAddition { get; private set; }

    public IReadOnlyDictionary<uint, uint> StatRolls { get; private set; }

    public AlternatePassiveAdditionInformation(AlternatePassiveAddition alternatePassiveAddition, IReadOnlyDictionary<uint, uint> statRolls)
    {
        ArgumentNullException.ThrowIfNull(alternatePassiveAddition, nameof(alternatePassiveAddition));

        AlternatePassiveAddition = alternatePassiveAddition;
        StatRolls = statRolls;
    }
}

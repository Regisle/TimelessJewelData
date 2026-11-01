using System;
using System.Collections.Generic;
using DatafileGenerator.Data.Models;

namespace DatafileGenerator.Game;

public class AlternatePassiveSkillInformation
{
    public AlternatePassiveSkill AlternatePassiveSkill { get; private set; }

    public IReadOnlyList<int> StatRolls { get; private set; }

    public IReadOnlyList<AlternatePassiveAdditionInformation> AlternatePassiveAdditionInformations { get; private set; }

    public AlternatePassiveSkillInformation(AlternatePassiveSkill alternatePassiveSkill, IReadOnlyList<int> statRolls, IReadOnlyList<AlternatePassiveAdditionInformation> alternatePassiveAdditionInformations)
    {
        ArgumentNullException.ThrowIfNull(alternatePassiveSkill, nameof(alternatePassiveSkill));
        ArgumentNullException.ThrowIfNull(statRolls, nameof(statRolls));
        ArgumentNullException.ThrowIfNull(alternatePassiveAdditionInformations, nameof(alternatePassiveAdditionInformations));

        AlternatePassiveSkill = alternatePassiveSkill;
        StatRolls = statRolls;
        AlternatePassiveAdditionInformations = alternatePassiveAdditionInformations;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using DatafileGenerator.Data;
using DatafileGenerator.Data.Models;
using DatafileGenerator.Random;

namespace DatafileGenerator.Game;

public sealed class AbyssAscendancyManager
{
    public IReadOnlyList<PassiveSkill> SelectAffectedNotables(string ascendancyName, uint jewelSeed, int requestedCount = 1)
    {
        if (string.IsNullOrWhiteSpace(ascendancyName))
            throw new ArgumentException("An ascendancy name is required.", nameof(ascendancyName));
        if (requestedCount < 0)
            throw new ArgumentOutOfRangeException(nameof(requestedCount));

        PassiveSkill ascendancyStart = DataManager.PassiveSkills.FirstOrDefault(q =>
            q.IsAscendancy &&
            string.Equals(q.AscName, ascendancyName, StringComparison.Ordinal) &&
            HasCharacterStartConnection(q));
        if (ascendancyStart == null)
            return Array.Empty<PassiveSkill>();
        uint classStartGraphIdentifier = GetCharacterStartConnection(ascendancyStart);
        PassiveSkill classStart = DataManager.GetPassiveSkill(classStartGraphIdentifier);
        if (classStart == null)
            return Array.Empty<PassiveSkill>();

        Dictionary<uint, int> ascendancyCosts = new Dictionary<uint, int>() { [ascendancyStart.GraphIdentifier] = 0 };
        Queue<PassiveSkill> costFrontier = new Queue<PassiveSkill>();
        costFrontier.Enqueue(ascendancyStart);
        while (costFrontier.Count > 0)
        {
            PassiveSkill selected = costFrontier.Dequeue();
            foreach (string connection in EnumerateConnections(selected))
            {
                if (!uint.TryParse(connection, out uint graphIdentifier) || ascendancyCosts.ContainsKey(graphIdentifier))
                    continue;
                PassiveSkill connected = DataManager.GetPassiveSkill(graphIdentifier);
                if (connected == null || !string.Equals(connected.AscName, ascendancyName, StringComparison.Ordinal))
                    continue;
                ascendancyCosts.Add(graphIdentifier, ascendancyCosts[selected.GraphIdentifier] + 1);
                costFrontier.Enqueue(connected);
            }
        }

        SingleSeedRandomNumberGenerator randomNumberGenerator = new SingleSeedRandomNumberGenerator(jewelSeed);
        // The client's synthetic root connects to both nodes in this order. It
        // marks them seen before randomly walking either entry into the wheel.
        List<PassiveSkill> frontier = new List<PassiveSkill>() { classStart, ascendancyStart };
        HashSet<uint> seenConnections = new HashSet<uint>() { classStart.GraphIdentifier, ascendancyStart.GraphIdentifier };
        List<PassiveSkill> affected = new List<PassiveSkill>(requestedCount);

        while (frontier.Count > 0 && affected.Count < requestedCount)
        {
            int selectedIndex = frontier.Count == 1 ? 0 : checked((int)randomNumberGenerator.Generate((uint)frontier.Count));
            PassiveSkill selected = frontier[selectedIndex];
            frontier.RemoveAt(selectedIndex);
            VisitConnections(selected.InConnections, ascendancyName, ascendancyCosts, seenConnections, frontier, affected);
            VisitConnections(selected.OutConnections, ascendancyName, ascendancyCosts, seenConnections, frontier, affected);
        }
        return affected;
    }

    public IReadOnlyList<string> GetSupportedAscendancies()
    {
        return DataManager.PassiveSkills
            .Where(q => q.IsAscendancy && HasCharacterStartConnection(q))
            .Select(q => q.AscName)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(q => q, StringComparer.Ordinal)
            .ToArray();
    }

    private static void VisitConnections(
        IReadOnlyList<string> connections,
        string ascendancyName,
        IReadOnlyDictionary<uint, int> ascendancyCosts,
        HashSet<uint> seenConnections,
        List<PassiveSkill> frontier,
        List<PassiveSkill> affected)
    {
        if (connections == null)
            return;
        // Connection order is load-bearing: the client consumes incoming then
        // outgoing graph edges without sorting before its random frontier draw.
        for (int i = 0; i < connections.Count; i++)
        {
            if (!uint.TryParse(connections[i], out uint graphIdentifier) || !seenConnections.Add(graphIdentifier))
                continue;

            PassiveSkill connected = DataManager.GetPassiveSkill(graphIdentifier);
            if (connected == null)
                continue;
            if (!connected.IsAscendancy || !string.Equals(connected.AscName, ascendancyName, StringComparison.Ordinal))
                continue;

            if (connected.IsNotable && IsEligibleAscendancyNotable(connected) &&
                ascendancyCosts.TryGetValue(graphIdentifier, out int cost) && cost < 4)
                affected.Add(connected);
            frontier.Add(connected);
        }
    }

    private static bool IsEligibleAscendancyNotable(PassiveSkill passiveSkill) =>
        !passiveSkill.IsProxy &&
        !passiveSkill.IsMastery &&
        !passiveSkill.IsJewelSocket &&
        !passiveSkill.IsBlight &&
        !passiveSkill.IsJustIcon &&
        !passiveSkill.IsAscendancyStart &&
        !passiveSkill.IsMultipleChoice &&
        !passiveSkill.IsMultipleChoiceOption;

    private static bool HasCharacterStartConnection(PassiveSkill passiveSkill) => GetCharacterStartConnection(passiveSkill) != 0;

    private static uint GetCharacterStartConnection(PassiveSkill passiveSkill)
    {
        foreach (string connection in EnumerateConnections(passiveSkill))
        {
            if (uint.TryParse(connection, out uint graphIdentifier) && DataManager.GetPassiveSkill(graphIdentifier)?.IsCharacterStart == true)
                return graphIdentifier;
        }
        return 0;
    }

    private static IEnumerable<string> EnumerateConnections(PassiveSkill passiveSkill)
    {
        if (passiveSkill.InConnections != null)
        {
            for (int i = 0; i < passiveSkill.InConnections.Count; i++)
                yield return passiveSkill.InConnections[i];
        }
        if (passiveSkill.OutConnections != null)
        {
            for (int i = 0; i < passiveSkill.OutConnections.Count; i++)
                yield return passiveSkill.OutConnections[i];
        }
    }
}

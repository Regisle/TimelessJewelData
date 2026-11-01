using System;
using System.Collections.Generic;
using DatafileGenerator.Data;
using DatafileGenerator.Data.Models;
using DatafileGenerator.Random;

namespace DatafileGenerator.Game;

public sealed class AbyssTreeManager
{
    public const int DefaultAbyssSize = 60;

    private sealed class WeightedPassive
    {
        public PassiveSkill PassiveSkill { get; }
        public uint Weight { get; }

        public WeightedPassive(PassiveSkill passiveSkill, uint weight)
        {
            PassiveSkill = passiveSkill;
            Weight = weight;
        }
    }

    public IReadOnlyList<PassiveSkill> SelectAffectedPassives(uint socketGraphIdentifier, uint jewelSeed, int abyssSize = DefaultAbyssSize)
    {
        if (abyssSize < 0)
            throw new ArgumentOutOfRangeException(nameof(abyssSize));

        PassiveSkill socket = DataManager.GetPassiveSkill(socketGraphIdentifier)
            ?? throw new ArgumentException($"No passive exists with graph identifier {socketGraphIdentifier}.", nameof(socketGraphIdentifier));
        if (!socket.IsJewelSocket)
            throw new ArgumentException($"Passive {socketGraphIdentifier} is not a jewel socket.", nameof(socketGraphIdentifier));

        RandomNumberGenerator randomNumberGenerator = new RandomNumberGenerator(socketGraphIdentifier, jewelSeed);
        List<WeightedPassive> frontier = new List<WeightedPassive>() { new WeightedPassive(socket, 1) };
        HashSet<uint> seenConnections = new HashSet<uint>();
        List<PassiveSkill> affectedPassives = new List<PassiveSkill>(abyssSize);
        uint totalWeight = 1;

        for (int selectionIndex = 0; selectionIndex < abyssSize && frontier.Count > 0; selectionIndex++)
        {
            uint roll = randomNumberGenerator.Generate(totalWeight);
            int selectedIndex = GetWeightedIndex(frontier, roll);
            WeightedPassive selected = frontier[selectedIndex];
            frontier.RemoveAt(selectedIndex);
            totalWeight -= selected.Weight;

            AddConnectedPassives(selected.PassiveSkill.InConnections, seenConnections, frontier, ref totalWeight);
            AddConnectedPassives(selected.PassiveSkill.OutConnections, seenConnections, frontier, ref totalWeight);

            if (selected.PassiveSkill.IsAbyssTransformable)
                affectedPassives.Add(selected.PassiveSkill);
        }

        return affectedPassives;
    }

    private static int GetWeightedIndex(IReadOnlyList<WeightedPassive> frontier, uint roll)
    {
        for (int i = 0; i < frontier.Count; i++)
        {
            if (frontier[i].Weight > roll)
                return i;
            roll -= frontier[i].Weight;
        }
        throw new InvalidOperationException("The Abyss frontier weight did not contain the generated roll.");
    }

    private static void AddConnectedPassives(
        IReadOnlyList<string> connections,
        HashSet<uint> seenConnections,
        List<WeightedPassive> frontier,
        ref uint totalWeight)
    {
        if (connections == null)
            return;

        for (int i = 0; i < connections.Count; i++)
        {
            if (!uint.TryParse(connections[i], out uint graphIdentifier) || !seenConnections.Add(graphIdentifier))
                continue;

            PassiveSkill connected = DataManager.GetPassiveSkill(graphIdentifier);
            // Masteries and class starts fail the client's traversal gate. Hidden
            // connector nodes still participate in the walk, even though they are
            // never emitted as transformed passives.
            if (connected == null || connected.IsMastery || connected.IsCharacterStart)
                continue;

            // Off-tree cluster scaffolding has no passive kind in the client
            // table used by this walk, so it always receives the default weight.
            uint weight = connected.IsCluster ? 5u : connected.IsNotable ? 25u : connected.IsAttribute ? 1u : 5u;
            frontier.Add(new WeightedPassive(connected, weight));
            totalWeight += weight;
        }
    }
}

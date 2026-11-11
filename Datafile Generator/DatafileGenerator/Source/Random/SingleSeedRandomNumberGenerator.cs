using System;

namespace DatafileGenerator.Random;

public sealed class SingleSeedRandomNumberGenerator
{
    private const uint Mat1 = 0x8F7011EE;
    private const uint Mat2 = 0xFC78FF1F;
    private const uint Tmat = 0x3793FDFF;
    private readonly uint[] state = new uint[5];

    public SingleSeedRandomNumberGenerator(uint seed)
    {
        state[1] = seed;
        state[2] = Mat1;
        state[3] = Mat2;
        state[4] = Tmat;
        for (uint i = 1; i < 8; i++)
        {
            int current = (int)(i & 3) + 1;
            int previous = (int)((i - 1) & 3) + 1;
            state[current] ^= i + 0x6C078965u * (state[previous] ^ (state[previous] >> 30));
        }
        for (int i = 0; i < 8; i++)
            GenerateNextState();
    }

    public uint Generate(uint exclusiveMaximumValue)
    {
        if (exclusiveMaximumValue == 0)
            throw new ArgumentOutOfRangeException(nameof(exclusiveMaximumValue));
        if (exclusiveMaximumValue == 1)
            return 0;

        ulong limit = (1UL << 32) / exclusiveMaximumValue * exclusiveMaximumValue;
        uint value;
        do
        {
            value = GenerateUInt();
        }
        while ((ulong)value >= limit);

        return value % exclusiveMaximumValue;
    }

    private void GenerateNextState()
    {
        uint a = state[4];
        uint b = (state[1] & 0x7FFFFFFF) ^ state[2] ^ state[3];
        a ^= a << 1;
        b ^= (b >> 1) ^ a;
        state[1] = state[2];
        state[2] = state[3];
        state[3] = a ^ (b << 10);
        state[4] = b;
        uint mask = unchecked((uint)-(int)(b & 1));
        state[2] ^= mask & Mat1;
        state[3] ^= mask & Mat2;
        state[0]++;
    }

    private uint GenerateUInt()
    {
        GenerateNextState();
        uint a = state[4];
        uint b = state[1] + (state[3] >> 8);
        a ^= b;
        if ((b & 1) != 0)
            a ^= Tmat;
        return a;
    }
}

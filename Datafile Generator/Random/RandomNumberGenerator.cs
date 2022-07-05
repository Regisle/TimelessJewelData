// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Random.RandomNumberGenerator
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

using System;
using System.Collections.Generic;
using System.Linq;
using DatafileGenerator.Data.Models;
using DatafileGenerator.Game;

namespace DatafileGenerator.Random
{
  public class RandomNumberGenerator
  {
    private const uint INITIAL_STATE_CONSTANT_0 = 1077108816;
    private const uint INITIAL_STATE_CONSTANT_1 = 3483595324;
    private const uint INITIAL_STATE_CONSTANT_2 = 1017929583;
    private const uint INITIAL_STATE_CONSTANT_3 = 932445695;
    private uint[] state;

    public RandomNumberGenerator(PassiveSkill passiveSkill, TimelessJewel timelessJewel)
    {
      ArgumentNullException.ThrowIfNull((object) timelessJewel, nameof (timelessJewel));
      this.state = (uint[]) null;
      this.Initialize(((IEnumerable<uint>) Array.Empty<uint>()).Append<uint>(passiveSkill.GraphIdentifier).Append<uint>(timelessJewel.Seed).ToArray<uint>());
    }

    private static uint ManipulateAlpha(uint value) => (uint) (((int) value ^ (int) (value >> 27)) * 1664525);

    private static uint ManipulateBravo(uint value) => (uint) (((int) value ^ (int) (value >> 27)) * 1566083941);

    public uint Generate(uint exclusiveMaximumValue)
    {
      uint num1 = exclusiveMaximumValue - 1U;
      uint num2 = 0;
      uint num3 = 0;
      do
      {
        num3 = this.GenerateUInt() | (uint) (2 * ((int) num3 << 31));
        num2 = (uint) (-1 | 2 * ((int) num2 << 31));
      }
      while (num2 < num1 || num3 / exclusiveMaximumValue >= num2 && (int) (num2 % exclusiveMaximumValue) != (int) num1);
      return num3 % exclusiveMaximumValue;
    }

    public uint Generate(uint minimumValue, uint maximumValue)
    {
      uint num1 = minimumValue + 2147483648U;
      uint num2 = maximumValue + 2147483648U;
      if (minimumValue >= 2147483648U)
        num1 = minimumValue + 2147483648U;
      if (maximumValue >= 2147483648U)
        num2 = maximumValue + 2147483648U;
      return (uint) ((int) this.Generate((uint) ((int) num2 - (int) num1 + 1)) + (int) num1 + int.MinValue);
    }

    private void Initialize(uint[] seeds)
    {
      this.state = new uint[5]
      {
        0U,
        1077108816U,
        3483595324U,
        1017929583U,
        932445695U
      };
      uint num1 = 1;
      for (int index = 0; index < seeds.Length; ++index)
      {
        uint num2 = RandomNumberGenerator.ManipulateAlpha(this.state[(int) (num1 % 4U) + 1] ^ this.state[(int) ((num1 + 1U) % 4U) + 1] ^ this.state[(int) ((uint) ((int) num1 + 4 - 1) % 4U) + 1]);
        this.state[(int) ((num1 + 1U) % 4U) + 1] += num2;
        uint num3 = num2 + (seeds[index] + num1);
        this.state[(int) ((uint) ((int) num1 + 1 + 1) % 4U) + 1] += num3;
        this.state[(int) (num1 % 4U) + 1] = num3;
        num1 = (num1 + 1U) % 4U;
      }
      for (int index = 0; index < 5; ++index)
      {
        uint num4 = RandomNumberGenerator.ManipulateAlpha(this.state[(int) (num1 % 4U) + 1] ^ this.state[(int) ((num1 + 1U) % 4U) + 1] ^ this.state[(int) ((uint) ((int) num1 + 4 - 1) % 4U) + 1]);
        this.state[(int) ((num1 + 1U) % 4U) + 1] += num4;
        uint num5 = num4 + num1;
        this.state[(int) ((uint) ((int) num1 + 1 + 1) % 4U) + 1] += num5;
        this.state[(int) (num1 % 4U) + 1] = num5;
        num1 = (num1 + 1U) % 4U;
      }
      for (int index = 0; index < 4; ++index)
      {
        uint num6 = RandomNumberGenerator.ManipulateBravo(this.state[(int) (num1 % 4U) + 1] + this.state[(int) ((num1 + 1U) % 4U) + 1] + this.state[(int) ((uint) ((int) num1 + 4 - 1) % 4U) + 1]);
        this.state[(int) ((num1 + 1U) % 4U) + 1] ^= num6;
        uint num7 = num6 - num1;
        this.state[(int) ((uint) ((int) num1 + 1 + 1) % 4U) + 1] ^= num7;
        this.state[(int) (num1 % 4U) + 1] = num7;
        num1 = (num1 + 1U) % 4U;
      }
      for (int index = 0; index < 8; ++index)
        this.GenerateNextState();
    }

    private void GenerateNextState()
    {
      uint num1 = this.state[4];
      uint num2 = this.state[1] & (uint) int.MaxValue ^ this.state[2] ^ this.state[3];
      uint num3 = num1 ^ num1 << 1;
      uint num4 = num2 ^ num2 >> 1 ^ num3;
      this.state[1] = this.state[2];
      this.state[2] = this.state[3];
      this.state[3] = num3 ^ num4 << 10;
      this.state[4] = num4;
      this.state[2] ^= (uint) (int) ((long) -((int) num4 & 1) & 2406486510L);
      this.state[3] ^= (uint) (int) ((long) -((int) num4 & 1) & 4235788063L);
      ++this.state[0];
    }

    private uint Temper()
    {
      uint num1 = this.state[4];
      uint num2 = this.state[1] + (this.state[3] >> 8);
      uint num3 = num1 ^ num2;
      if (((int) num2 & 1) != 0)
        num3 ^= 932445695U;
      return num3;
    }

    private uint GenerateUInt()
    {
      this.GenerateNextState();
      return this.Temper();
    }
  }
}

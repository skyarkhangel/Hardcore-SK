// Decompiled with JetBrains decompiler
// Type: ModCommon.Calc
// Assembly: RimWorld_AtomicPowerMod, Version=0.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3BA90F32-070E-4A95-8557-409C6CD87E36
// Assembly location: E:\Downloads\RimWorld671Win\Mods\AtomicPower\Assemblies\RimWorld_AtomicPowerMod.dll

using System;

namespace SK_NPP
{
  internal class Calc
  {
    public int ValueIntCeiling(float value)
    {
      return (int) Math.Ceiling((double) value);
    }

    public int ValueIntRound(float value)
    {
      return (int) Math.Round((double) value);
    }
  }
}

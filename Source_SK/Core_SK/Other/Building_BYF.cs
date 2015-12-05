using RimWorld;
using Verse;

namespace SK_PushHeater
{
    public class Building_Furnace : Building_WorkTable, IBillGiverWithTickAction
  {
    private const int HeatPushInterval = 30;

    public void BillTick()
    {
      if (Find.TickManager.TicksGame % 30 != 4)
        return;
      GenTemperature.PushHeat((Thing) this, this.def.building.heatPerTickWhileWorking / 30f);
      MoteThrower.ThrowSmoke(base.Position.ToVector3Shifted(), 2f);
    }
  }
}
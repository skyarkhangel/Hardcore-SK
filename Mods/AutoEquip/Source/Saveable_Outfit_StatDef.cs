using RimWorld;
using Verse;

namespace AutoEquip
{
    public class Saveable_Outfit_StatDef : IExposable
    {
        public Saveable_Outfit_StatDef()
        {
            Log.Message("Saveable_Outfit Constructor");
        }        

        public StatDef statDef;
        public float strength;

        public void ExposeData()
        {
            Scribe_Defs.LookDef(ref this.statDef, "statDef");
            Scribe_Values.LookValue(ref this.strength, "strength");
        }
    }
}

using UnityEngine;
using System.Collections;
using Verse;


namespace RimWorld{
public class CompLifespanDebris : ThingComp
{
	public int age = -1;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.LookValue(ref age, "age");
	}

	public override void CompTick()
	{
		age++;
		if( age >= props.lifespanTicks )
            this.Debris();
	}


    public override void PostDestroy(DestroyMode mode)
    {
        if (mode != DestroyMode.Kill)
            return;
        this.Debris();
        GenSpawn.Spawn(ThingDef.Named("ExtractorDebris"), this.parent.Position);
    }

	public override string CompInspectStringExtra() 
	{
		string descStr = base.CompInspectStringExtra();

		int ticksLeft = props.lifespanTicks - age;
		if ( ticksLeft > 0 )
			descStr = "LifespanExpiry".Translate() + " " + ticksLeft.TickstoDaysAndHoursString() + "\n" + descStr;

		return descStr; 
	}
            protected void Debris()
        {
            if (!this.parent.Destroyed)
                this.parent.Destroy(DestroyMode.Kill);
        }
}}
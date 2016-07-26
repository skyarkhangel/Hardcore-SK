using RimWorld;
using Verse;

namespace Fluffy_Breakdowns
{
    public class CompBreakdownableMaintenance : CompBreakdownable
    {
        #region Properties

        private int componentLifetime => MapComponent_Durability.componentLifetime;

        private int checkinterval => BreakdownManager.CheckIntervalTicks;

        private float durability
        {
            get
            {
                return MapComponent_Durability.GetDurability( this );
            }
            set
            {
                MapComponent_Durability.SetDurability( this, value );
            }
        }

        #endregion Properties

        #region Methods

        public void _checkForBreakdown()
        {
            if ( !BrokenDown )
            {
                durability -= (float)checkinterval / (float)componentLifetime;
                //Log.Message( parent.LabelCap +": "+ durability + ", " + ( (float)checkinterval / componentLifetime ) + ", " + checkinterval + "/" + componentLifetime );

                // durability below 50%, increasing chance of breakdown ( up to almost guaranteed at 1% (minimum) maintenance.
                if ( durability < .5 && Rand.MTBEventOccurs( GenDate.TicksPerYear * durability, 1f, checkinterval ) )
                    DoBreakdown();
            }
        }

        public new void DoBreakdown()
        {
            base.DoBreakdown();

            // reset durability
            durability = 1f;
        }


        public string _compInspectStringExtra()
        {
            string text = "";
            if ( this.BrokenDown )
            {
                text += "BrokenDown".Translate() + "\n";
            }

            text += "FluffyBreakdowns.Maintenance".Translate( durability.ToStringPercent() );

            return text;
        }

        #endregion Methods
    }
}
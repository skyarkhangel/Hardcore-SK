// Manager/Comp_ManagerStation.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:30

using Verse;

namespace FluffyManager
{
    public class Comp_ManagerStation : ThingComp
    {
        // todo add automatic work setup.
        // todo add research and more workstations.
        public CompProperties_ManagerStation props;

        public override void Initialize( CompProperties vprops )
        {
            base.Initialize( vprops );
            props = vprops as CompProperties_ManagerStation;
            if ( props == null )
            {
                Log.Warning( "props went horribly wrong." );
                props = new CompProperties_ManagerStation { Speed = 250 };
            }
        }
    }
}
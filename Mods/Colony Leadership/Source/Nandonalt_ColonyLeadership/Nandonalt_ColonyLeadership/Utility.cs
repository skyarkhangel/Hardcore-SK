using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Nandonalt_ColonyLeadership
{
    class Utility
    {

        public static GameComponent_ColonyLeadership getCLComp()
        {
            GameComponent_ColonyLeadership comp = Current.Game.GetComponent<GameComponent_ColonyLeadership>();
                   return comp;         
              }

        public static GovType getGov()
        {
            GameComponent_ColonyLeadership comp = getCLComp();
                if (comp != null ) return comp.chosenLeadership;
            return null;
        }

        public static bool isDictatorship
        {
            get
            {
                GameComponent_ColonyLeadership comp = getCLComp();
                if (comp != null && comp.chosenLeadership.name == "Dictatorship".Translate()) return true;
                return false;
            }
        }


        public static bool isDemocracy
        {
            get
            {
                GameComponent_ColonyLeadership comp = getCLComp();
                if (comp != null && comp.chosenLeadership.name == "Democracy".Translate()) return true;
                return false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace Nandonalt_ColonyLeadership
{
    public class GameComponent_ColonyLeadership : GameComponent
    {

        public int lastLessonTick = -999999;
        public GovType chosenLeadership;
        public int chosenGov = 0;

        public GameComponent_ColonyLeadership(Game game)
        {

        }

        public GameComponent_ColonyLeadership()
        {

        }

        public GameComponent_ColonyLeadership(Map map)
        {

        }
        public override void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.chosenGov, "chosenGov", 0, false);
            chosenLeadership = ColonyLeadership.govtypes[chosenGov];
            Scribe_Values.Look<int>(ref this.lastLessonTick, "lastLessonTick", -999999, false);
            base.ExposeData();
        }

        public override void StartedNewGame()
        {
            checkGovs();
        }

        public override void LoadedGame()
        {
            checkGovs();
        }

        public void checkGovs()
        {              

                if (chosenLeadership == null || (chosenLeadership.name == "" && !Find.WindowStack.WindowsForcePause))
                {
                    if (ColonyLeadership.tempGov != null && Find.GameInfo == ColonyLeadership.gameInfoTemp)
                    {
                        chosenLeadership = ColonyLeadership.tempGov;
                        chosenGov = ColonyLeadership.govtypes.IndexOf(ColonyLeadership.tempGov);
                    }
                    else { Find.WindowStack.Add(new Dialog_ChooseRules()); }
                }
            ColonyLeadership.tempGov = this.chosenLeadership;
            ColonyLeadership.gameInfoTemp = Find.GameInfo;
            }

        }
    
}

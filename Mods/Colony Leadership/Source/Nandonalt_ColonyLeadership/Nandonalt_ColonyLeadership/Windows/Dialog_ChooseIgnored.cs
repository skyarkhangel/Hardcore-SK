using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Collections;

namespace Nandonalt_ColonyLeadership
{
        public class Dialog_ChooseIgnored : Window
        {
            protected string curName;
            public List<Pawn> ignoreTemp;
            public List<TempPawn> tempPawnList = new List<TempPawn>();
        public int MaxSize;
            public int MinSize;
            public string MaxSizebuf;
            public string MinSizebuf;
            public bool permanent;
        private Building_TeachingSpot spot;



            public override Vector2 InitialSize
            {
                get
                {
                    return new Vector2(280f, 130f + (tempPawnList.Count * 25f));
                }
            }

        public Dialog_ChooseIgnored(Building_TeachingSpot spot)
        {
            this.forcePause = true;
            this.doCloseX = true;
            this.closeOnEscapeKey = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
            this.spot = spot;
            this.ignoreTemp = IncidentWorker_LeaderElection.getAllColonists();
         

            foreach (Pawn p in this.ignoreTemp)
            {
                if (this.spot.ignored.Contains(p))
                {
                    tempPawnList.Add(new TempPawn(p, true));
                }
                else
                {
                    tempPawnList.Add(new TempPawn(p, false));
                }
            }
          
        }

            public override void DoWindowContents(Rect inRect)
            {
          
            

                Text.Font = GameFont.Small;
                bool flag = false;

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                {
                    flag = true;
                    Event.current.Use();
                }
                Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);

            listing_Standard.Label("IgnoreLectures".Translate());
            listing_Standard.Gap(5f);
            foreach (TempPawn temp in tempPawnList)
            {
                Pawn p = temp.reference;
                if (!p.Dead) {
                     listing_Standard.CheckboxLabeled(p.LabelShort, ref temp.value);
                    //ignoreTempValue[ignoreTemp.IndexOf(p)] = b;
                }
            }
            if (listing_Standard.ButtonText("OK".Translate(), null) || flag)
            {
                foreach(TempPawn temp in tempPawnList)
                {
                    Pawn p = temp.reference;
                    if (spot.ignored.Contains(p))
                    {
                        if (temp.value == false)
                        {
                            spot.ignored.Remove(p);
                        }
                                         }
                    else
                    {
                        if (temp.value == true)
                        {
                            spot.ignored.Add(p);
                        }
                    }
                }
                Find.WindowStack.TryRemove(this, true);
            }
            listing_Standard.Gap(10f);
            listing_Standard.End();
            }

        }

    public class TempPawn
    {
        public bool value;
        public Pawn reference;

        public TempPawn(Pawn re, bool val = false)
        {
            this.value = val;
            this.reference = re;
        }

    }
    
}

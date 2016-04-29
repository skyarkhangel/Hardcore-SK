using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;

namespace kNumbers
{
    public abstract class MainTabWindow_ThingWithComp: MainTabWindow
	{

        public const float PawnRowHeight = 30f;

        protected const float NameColumnWidth = 175f;

        protected const float NameLeftMargin = 15f;

        protected Vector2 scrollPosition = Vector2.zero;

        protected List<ThingWithComps> things = new List<ThingWithComps>();
/*
        protected List<Pawn> pawns
        {
            set
            {
                this.things = value.Select(p=>p as ThingWithComps).ToList();
            }
        }
*/
		protected int ThingsCount
		{
			get
			{
                return this.things.Count;
			}
		}

        protected abstract void DrawPawnRow(Rect r, ThingWithComps p);

        public override void PreOpen()
        {
            base.PreOpen();
            this.BuildPawnList();
        }

        public override void PostOpen()
        {
            base.PostOpen();
            this.currentWindowRect.size = this.InitialWindowSize;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            this.currentWindowRect.size = this.InitialWindowSize;
        }

		protected virtual void BuildPawnList()
		{
			this.things.Clear();
    	}

        public void Notify_PawnsChanged()
        {
            this.BuildPawnList();
        }

		protected void DrawRows(Rect outRect)
		{
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)this.things.Count * 30f);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);
			float num = 0f;
			for (int i = 0; i < this.things.Count; i++)
			{
                ThingWithComps p = this.things[i];
				Rect rect = new Rect(0f, num, viewRect.width, 30f);
				if (num - this.scrollPosition.y + 30f >= 0f && num - this.scrollPosition.y <= outRect.height)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.2f);
					Widgets.DrawLineHorizontal(0f, num, viewRect.width);
					GUI.color = Color.white;
					this.PreDrawPawnRow(rect, p);
					this.DrawPawnRow(rect, p);
					this.PostDrawPawnRow(rect, p);
				}
				num += 30f;
			}
			Widgets.EndScrollView();
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void PreDrawPawnRow(Rect rect, ThingWithComps p)
		{
			Rect rect2 = new Rect(0f, rect.y, rect.width, 30f);
			if (Mouse.IsOver(rect2))
			{
				GUI.DrawTexture(rect2, TexUI.HighlightTex);
			}
			Rect rect3 = new Rect(0f, rect.y, 175f, 30f);
			Rect position = rect3.ContractedBy(3f);
            if (p is Pawn)
            {
			    if ((p as Pawn).health.summaryHealth.SummaryHealthPercent < 0.99f)
			    {
				    Rect rect4 = new Rect(rect3);
				    rect4.xMin -= 4f;
				    rect4.yMin += 4f;
				    rect4.yMax -= 6f;
				    Widgets.FillableBar(rect4, (p as Pawn).health.summaryHealth.SummaryHealthPercent, PawnUIOverlay.OverlayHealthTex, BaseContent.ClearTex, false);
			    }
            }
			if (Mouse.IsOver(rect3))
			{
				GUI.DrawTexture(position, TexUI.HighlightTex);
			}
			string label;
            Pawn p1 = (p is Corpse) ? (p as Corpse).innerPawn : p as Pawn;
			if (!p1.RaceProps.Humanlike && p1.Name != null && !p1.Name.Numerical)
			{
				label = p1.Name.ToStringShort.CapitalizeFirst() + ", " + p1.KindLabel;
			}
			else
			{
				label = p1.LabelCap;
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			Rect rect5 = new Rect(rect3);
			rect5.xMin += 15f;
			Widgets.Label(rect5, label);
			Text.WordWrap = true;
			if (Widgets.InvisibleButton(rect3))
			{
				Find.MainTabsRoot.EscapeCurrentTab(true);
				Find.CameraMap.JumpTo(p.PositionHeld);
				Find.Selector.ClearSelection();
				if (p.Spawned)
				{
					Find.Selector.Select(p, true, true);
				}
				return;
			}
			TipSignal tooltip = p.GetTooltip();
			tooltip.text = "ClickToJumpTo".Translate() + "\n\n" + tooltip.text;
			TooltipHandler.TipRegion(rect3, tooltip);
		}

        private void PostDrawPawnRow(Rect rect, ThingWithComps p)
		{
            if (p is Pawn)
            {
                if ((p as Pawn).Downed)
                {
                    GUI.color = new Color(1f, 0f, 0f, 0.5f);
                    Widgets.DrawLineHorizontal(rect.x, rect.center.y, rect.width);
                    GUI.color = Color.white;
                }
            }
		}
	}


    public class MainTabWindow_Numbers : MainTabWindow_ThingWithComp
    {

        public enum pawnType
        {
            Colonists,
            Prisoners,
            Guests,
            Enemies,    //assuming humanlike enemies, animals somewhat worked, but mechanoids will crash the tab
            Animals,    
            WildAnimals,
            Corpses,
            AnimalCorpses,
        }

        public enum orderBy
        {
            Name,
            Column
        }

       // List<ThingWithComps> things;
        public static bool pawnListDescending = false;
        public static bool isDirty = true;
        int pawnListUpdateNext = 0;

        //global lists
        List<StatDef> pawnHumanlikeStatDef = new List<StatDef>();
        List<StatDef> pawnAnimalStatDef = new List<StatDef>();
        List<NeedDef> pawnHumanlikeNeedDef = new List<NeedDef>();
        List<NeedDef> pawnAnimalNeedDef = new List<NeedDef>();
        List<SkillDef> pawnSkillDef = new List<SkillDef>();

        //local lists - content depends on pawn type
        List<StatDef> pStatDef;
        List<NeedDef> pNeedDef;

        List<KListObject> kList = new List<KListObject>();

        public static Dictionary<pawnType, List<KListObject>> savedKLists = new Dictionary<pawnType, List<KListObject>>(5);
        public static pawnType chosenPawnType = pawnType.Colonists;
        orderBy chosenOrderBy = orderBy.Name;
        KListObject sortObject;

        float maxWindowWidth = 1060f;

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(maxWindowWidth, 90f + (float)base.ThingsCount * PawnRowHeight + 65f);
            }
        }

        float kListDesiredWidth = 0f;
        
        

        public MainTabWindow_Numbers()
        {
            Pawn tmpPawn;

            MethodInfo statsToDraw = typeof(StatsReportUtility).GetMethod("StatsToDraw", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, new Type[] { typeof(Thing) }, null);

            tmpPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfColony);
            pawnHumanlikeStatDef = (from s in ((IEnumerable<StatDrawEntry>)statsToDraw.Invoke(null, new[] { tmpPawn })) where s.ShouldDisplay && s.stat != null select s.stat).ToList();
            pawnHumanlikeNeedDef.AddRange(tmpPawn.needs.AllNeeds.Where(x => x.def.label == "mood").Select(x => x.def).ToList()); //why it's not normally returned is beyond me
            pawnHumanlikeNeedDef.AddRange(tmpPawn.needs.AllNeeds.Where(x => x.def.showOnNeedList).Select(x => x.def).ToList());

            tmpPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Thrumbo, null);
            pawnAnimalStatDef = (from s in ((IEnumerable<StatDrawEntry>)statsToDraw.Invoke(null, new[] { tmpPawn })) where s.ShouldDisplay && s.stat != null select s.stat).ToList();
            pawnAnimalNeedDef = tmpPawn.needs.AllNeeds.Where(x => x.def.showOnNeedList).Select(x => x.def).ToList();

            savedKLists = new Dictionary<MainTabWindow_Numbers.pawnType, List<KListObject>>(5);
            foreach (MainTabWindow_Numbers.pawnType pType in Enum.GetValues(typeof(MainTabWindow_Numbers.pawnType)))
            {
                savedKLists.Add(pType, new List<KListObject>());
            }

            MapComponent_Numbers.InitMapComponent();

        }

        public override void PreOpen()
        {
            base.PreOpen();
            isDirty = true;
            if (MapComponent_Numbers.hasData)
            {
                savedKLists = MapComponent_Numbers.savedKLists;
                chosenPawnType = MapComponent_Numbers.chosenPawnType;
                kList = savedKLists[chosenPawnType];
                MapComponent_Numbers.hasData = false;
            }
        }

        bool fits(float desiredSize)
        {
            return (kListDesiredWidth + desiredSize + 70 < maxWindowWidth);
        }


        bool isEnemy(Pawn p)
        {
            return
                   !p.IsPrisoner &&
                   (
                    ((p.Faction != null) && p.Faction.HostileTo(Faction.OfColony)) ||
                    (!p.RaceProps.Animal && (!p.RaceProps.Humanlike || p.RaceProps.IsMechanoid)) 
                   ) && 
                   !p.Position.Fogged() && (p.Position != IntVec3.Invalid);
        }

        bool isWildAnimal(Pawn p)
        {
            return p.RaceProps.Animal && (p.Faction != Faction.OfColony) && !p.Position.Fogged() && (p.Position != IntVec3.Invalid);
        }

        bool isGuest(Pawn p)
        {
            return
                   (p.guest != null) && !p.guest.IsPrisoner &&
                   (p.Faction != null) && !p.Faction.HostileTo(Faction.OfColony) && p.Faction != Faction.OfColony && 
                   !p.Position.Fogged() && (p.Position != IntVec3.Invalid);
        }

        void UpdatePawnList()
        {
            savedKLists[chosenPawnType] = kList;

            this.things.Clear();
            IEnumerable<ThingWithComps> tempPawns = new List<ThingWithComps>();

            switch (chosenPawnType)
            {
                default:
                case pawnType.Colonists:
                    tempPawns = Find.MapPawns.FreeColonists.Select(p=>p as ThingWithComps).ToList();
                    pStatDef = pawnHumanlikeStatDef;
                    pNeedDef = pawnHumanlikeNeedDef;
                    break;

                case pawnType.Prisoners:
                    tempPawns = Find.MapPawns.PrisonersOfColony.Select(p => p as ThingWithComps).ToList();
                    pStatDef = pawnHumanlikeStatDef;
                    pNeedDef = pawnHumanlikeNeedDef;
                    break;

                case pawnType.Guests:
                    tempPawns = Find.MapPawns.AllPawns.Where(p => isGuest(p)).Select(p => p as ThingWithComps).ToList();
                    pStatDef = pawnHumanlikeStatDef;
                    pNeedDef = pawnHumanlikeNeedDef;
                    break;

                case pawnType.Enemies:
                   // tempPawns = Find.MapPawns.PawnsHostileToColony.Select(p => p as ThingWithComps).ToList();
                    tempPawns = (from p in Find.MapPawns.AllPawns where isEnemy(p) select p).Select(p => p as ThingWithComps).ToList();
                    pStatDef = pawnHumanlikeStatDef;
                    pNeedDef = pawnHumanlikeNeedDef;
                    break;

                case pawnType.Animals:
                    tempPawns = (from p in Find.MapPawns.PawnsInFaction(Faction.OfColony) where p.RaceProps.Animal select p).Select(p => p as ThingWithComps).ToList();
                    pStatDef = pawnAnimalStatDef;
                    pNeedDef = pawnAnimalNeedDef;
                    break;

                case pawnType.WildAnimals:
                    tempPawns = (from p in Find.MapPawns.AllPawns where isWildAnimal(p) select p).Select(p => p as ThingWithComps).ToList();
                    pStatDef = pawnAnimalStatDef;
                    pNeedDef = pawnAnimalNeedDef;
                    break;

                case pawnType.Corpses:
                    tempPawns = Find.ListerThings.AllThings.Where(p => (p is Corpse) && (!(p as Corpse).innerPawn.RaceProps.Animal)).Select(p => p as ThingWithComps).ToList();
                    pStatDef = new List<StatDef>();
                    pNeedDef = new List<NeedDef>();
                    break;
                case pawnType.AnimalCorpses:
                    tempPawns = Find.ListerThings.AllThings.Where(p => (p is Corpse) && (p as Corpse).innerPawn.RaceProps.Animal && !p.Position.Fogged()).Select(p => p as ThingWithComps).ToList();
                    pStatDef = new List<StatDef>();
                    pNeedDef = new List<NeedDef>();
                    break;
            }

            switch (chosenOrderBy)
            {
                default:
                case orderBy.Name:
                    this.things = (from p in tempPawns
                                  orderby p.LabelCap ascending
                                  select p).ToList();
                    break;

                case orderBy.Column:
                    switch (sortObject.oType)
                    {
                        case KListObject.objectType.Stat:
                            this.things = (from p in tempPawns
                                          orderby p.GetStatValue((StatDef)sortObject.displayObject, true) ascending
                                          select p).ToList();
                            break;

                        case KListObject.objectType.Need:
                            //pause game if we're sorting by needs because these things change
                            /*
                            if (!Find.TickManager.Paused)
                                Find.TickManager.TogglePaused();
                                */
                            this.things = (from p in tempPawns
                                          where (p is Pawn) && !(p as Pawn).RaceProps.IsMechanoid && ((p as Pawn).needs != null)
                                          orderby (p as Pawn).needs.TryGetNeed((NeedDef)sortObject.displayObject).CurLevel ascending
                                          select p).ToList();
                            break;

                        case KListObject.objectType.Skill:
                            this.things = (from p in tempPawns
                                          where (p is Pawn) && (p as Pawn).RaceProps.Humanlike && ((p as Pawn).skills != null)
                                          orderby (p as Pawn).skills.GetSkill((SkillDef)sortObject.displayObject).XpTotalEarned ascending
                                          select p).ToList();
                            break;

                        case KListObject.objectType.Gear:
                            this.things = tempPawns.Where(p=>(p is Pawn)||((p is Corpse)&&(!(p as Corpse).innerPawn.RaceProps.Animal))).OrderBy(p => {
                                                    Pawn p1 = (p is Pawn)?(p as Pawn):(p as Corpse).innerPawn;
                                                    return (p1.equipment != null) ? ((p1.equipment.AllEquipment.Count() > 0) ? p1.equipment.AllEquipment.First().LabelCap : "") : "";
                                                    }).ToList();
                            break;

                        case KListObject.objectType.ControlPrisonerGetsFood:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(p => (p as Pawn).guest.GetsFood).ToList();
                            break;

                        case KListObject.objectType.ControlPrisonerInteraction:
                            this.things = tempPawns.Where(p=>p is Pawn).OrderBy(p => (p as Pawn).guest.interactionMode).ToList();
                            break;

                        case KListObject.objectType.ControlMedicalCare:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(p => (p as Pawn).playerSettings.medCare).ToList();
                            break;

                        default:
                            //no way to sort
                            this.things = tempPawns.ToList();
                            break;
                    }
                    
                    break;
            }

            if (pawnListDescending)
            {
                this.things.Reverse();
            }

            isDirty = false;
            pawnListUpdateNext = Find.TickManager.TicksGame + Verse.GenTicks.TickRareInterval;

        }

        public void PawnSelectOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (pawnType pawn in Enum.GetValues(typeof(pawnType)))
            {
                Action action = delegate
                {
                    if (pawn != chosenPawnType)
                    { 
                        savedKLists[chosenPawnType] = kList;
                        savedKLists.TryGetValue(pawn, out kList);
                        if (kList == null)
                            kList = new List<KListObject>();
                        chosenPawnType = pawn;
                        isDirty = true;
                    }
                };

                list.Add(new FloatMenuOption(("koisama.pawntype."+pawn.ToString()).Translate(), action, MenuOptionPriority.Medium, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list, false));
        }

        public void StatsOptionsMaker()
        {
            
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach(StatDef stat in pStatDef)
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.Stat, stat.LabelCap, stat);
                    if(fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption(stat.LabelCap, action, MenuOptionPriority.Medium, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list, false));
        }

        public void SkillsOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (SkillDef skill in DefDatabase<SkillDef>.AllDefsListForReading)
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.Skill, skill.LabelCap, skill);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption(skill.LabelCap, action, MenuOptionPriority.Medium, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list, false));
        }

        public void NeedsOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (NeedDef need in pNeedDef)
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.Need, need.LabelCap, need);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption(need.LabelCap, action, MenuOptionPriority.Medium, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list, false));
        }

        //other hardcoded options
        public void OtherOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
                        
            if (new[] { pawnType.Colonists, pawnType.Prisoners, pawnType.Enemies, pawnType.Corpses }.Contains(chosenPawnType))
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.Gear, "Equipment".Translate(), null);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption("Equipment".Translate(), action, MenuOptionPriority.Medium, null, null));
            }

            if (chosenPawnType == pawnType.Prisoners) {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.ControlPrisonerGetsFood, "GetsFood".Translate(), null);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption("GetsFood".Translate(), action, MenuOptionPriority.Medium, null, null));

                Action action2 = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.ControlPrisonerInteraction, "koisama.Interaction".Translate(), null);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.Interaction".Translate(), action2, MenuOptionPriority.Medium, null, null));
            }

            if (new[] { pawnType.Colonists, pawnType.Prisoners }.Contains(chosenPawnType))
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.ControlMedicalCare, "koisama.MedicalCare".Translate(), null);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.MedicalCare".Translate(), action, MenuOptionPriority.Medium, null, null));
            }


                Find.WindowStack.Add(new FloatMenu(list, false));
        }

        public override void DoWindowContents(Rect r)
        {
            base.DoWindowContents(r);

            if (pawnListUpdateNext < Find.TickManager.TicksGame)
                isDirty = true;

            if (isDirty)
            {
                UpdatePawnList();
            }

            Rect position = new Rect(0f, 0f, r.width, 110f);
            GUI.BeginGroup(position);

            float x = 0f;
            Text.Font = GameFont.Small;

            //pawn/prisoner list switch
            Rect sourceButton = new Rect(x, 0f, 180f, PawnRowHeight);
            if (Widgets.TextButton(sourceButton, ("koisama.pawntype." + chosenPawnType.ToString()).Translate()))
            {
                PawnSelectOptionsMaker();
            }
            x += 190;

            //stats btn
            Rect addColumnButton = new Rect(x, 0f, 180f, PawnRowHeight);
            if (Widgets.TextButton(addColumnButton, "koisama.Numbers.AddColumnLabel".Translate()))
            {
                StatsOptionsMaker();
            }
            x += 190;

            //skills btn
            if (new[] { pawnType.Colonists, pawnType.Prisoners, pawnType.Enemies }.Contains(chosenPawnType))
            {
                Rect skillColumnButton = new Rect(x, 0f, 180f, PawnRowHeight);
                if (Widgets.TextButton(skillColumnButton, "koisama.Numbers.AddSkillColumnLabel".Translate()))
                {
                    SkillsOptionsMaker();
                }
                x += 190;
            }

            //needs btn
            Rect needsColumnButton = new Rect(x, 0f, 180f, PawnRowHeight);
            if (Widgets.TextButton(needsColumnButton, "koisama.Numbers.AddNeedsColumnLabel".Translate()))
            {
                NeedsOptionsMaker();
            }
            x += 190;

            Rect otherColumnBtn = new Rect(x, 0f, 180f, PawnRowHeight);
            if (Widgets.TextButton(otherColumnBtn, "koisama.Numbers.AddOtherColumnLabel".Translate()))
            {
                OtherOptionsMaker();
            }
            x += 190;

            x = 0;
            //names
            Rect nameLabel = new Rect(x, 75f, NameColumnWidth, PawnRowHeight);
            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(nameLabel, "koisama.Numbers.Name".Translate());
            if (Widgets.InvisibleButton(nameLabel))
            {
                if (chosenOrderBy == orderBy.Name)
                {
                    pawnListDescending = !pawnListDescending;
                }
                else
                {
                    chosenOrderBy = orderBy.Name;
                    pawnListDescending = false;
                }
                isDirty = true;
            }

            TooltipHandler.TipRegion(nameLabel, "koisama.Numbers.SortByTooltip".Translate("koisama.Numbers.Name".Translate()));
            Widgets.DrawHighlightIfMouseover(nameLabel);
            x += NameColumnWidth;

            //header
            //TODO: better interface - auto width calculation
            bool offset = true;
            kListDesiredWidth = 175f;

            for (int i=0;i<kList.Count; i++)
            {
                float colWidth = kList[i].minWidthDesired;
                kListDesiredWidth += colWidth;

                Rect defLabel = new Rect(x-35, 20f + (offset ? 10f : 50f), colWidth+70 , 40f);
                Widgets.DrawLine(new Vector2(x + colWidth/2 , 52f + (offset ? 15f : 55f)), new Vector2(x + colWidth/2 , 110f), Color.gray, 1);
                Widgets.Label(defLabel, kList[i].label);

                StringBuilder labelSB = new StringBuilder();
                labelSB.AppendLine("koisama.Numbers.SortByTooltip".Translate(kList[i].label));
                labelSB.AppendLine("koisama.Numbers.RemoveTooltip".Translate());
                TooltipHandler.TipRegion(defLabel, labelSB.ToString());
                Widgets.DrawHighlightIfMouseover(defLabel);

                if (Widgets.InvisibleButton(defLabel))
                {
                    if (Event.current.button == 1)
                    {
                        kList.RemoveAt(i);
                    }
                    else
                    {

                        if (chosenOrderBy == orderBy.Column && kList[i].Equals(sortObject))
                        {
                            pawnListDescending = !pawnListDescending;
                        }
                        else
                        {
                            sortObject = kList[i];
                            chosenOrderBy = orderBy.Column;
                            pawnListDescending = false;
                        }
                    }
                    isDirty = true;
                }
                offset = !offset;
                x += colWidth;
            }
            GUI.EndGroup();

            //content
            Rect content = new Rect(0f, position.yMax, r.width, r.height - position.yMax);
            GUI.BeginGroup(content);
            base.DrawRows(new Rect(0f, 0f, content.width, content.height));
            GUI.EndGroup();
        }

        protected override void DrawPawnRow(Rect r, ThingWithComps p)
        {
            float x = 175f;
            float y = r.yMin;

            Text.Anchor = TextAnchor.MiddleCenter;

            //TODO: better interface - auto width calculation, make sure columns won't overlap
            for (int i = 0; i < kList.Count; i++)
            {
                float colWidth = kList[i].minWidthDesired;
                Rect capCell = new Rect(x, y, colWidth, 30f);
                kList[i].Draw(capCell, p);
                x += colWidth;
            }

            /*
            if (p.health.Downed) {
                Widgets.DrawLine(new Vector2(5f, y + PawnRowHeight / 2), new Vector2(r.xMax - 5f, y + PawnRowHeight / 2), Color.red, 1);
            }*/

        }

    }
}

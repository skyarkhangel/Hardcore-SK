using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;

namespace kNumbers
{
    public abstract class MainTabWindow_ThingWithComp: MainTabWindow
	{
        public const int cFreeSpaceAtTheEnd = 50;

        public const float buttonWidth = 160f;

        public const float PawnRowHeight = 35f;

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
            this.windowRect.size = this.InitialSize;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            this.windowRect.size = this.InitialSize;
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
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)this.things.Count * PawnRowHeight);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);
			float num = 0f;
			for (int i = 0; i < this.things.Count; i++)
			{
                ThingWithComps p = this.things[i];
				Rect rect = new Rect(0f, num, viewRect.width, PawnRowHeight);
				if (num - this.scrollPosition.y + PawnRowHeight >= 0f && num - this.scrollPosition.y <= outRect.height)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.2f);
					Widgets.DrawLineHorizontal(0f, num, viewRect.width);
					GUI.color = Color.white;
					this.PreDrawPawnRow(rect, p);
					this.DrawPawnRow(rect, p);
					this.PostDrawPawnRow(rect, p);
				}
				num += PawnRowHeight;
			}
			Widgets.EndScrollView();
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void PreDrawPawnRow(Rect rect, ThingWithComps p)
		{
			Rect rect2 = new Rect(0f, rect.y, rect.width, PawnRowHeight);
			if (Mouse.IsOver(rect2))
			{
				GUI.DrawTexture(rect2, TexUI.HighlightTex);
			}
			Rect rect3 = new Rect(0f, rect.y, 175f, PawnRowHeight);
			Rect position = rect3.ContractedBy(3f);
            if (p is Pawn)
            {
			    if ((p as Pawn).health.summaryHealth.SummaryHealthPercent < 0.999f)
			    {
				    Rect rect4 = new Rect(rect3);
				    rect4.xMin -= 4f;
				    rect4.yMin += 4f;
				    rect4.yMax -= 6f;
				    Widgets.FillableBar(rect4, (p as Pawn).health.summaryHealth.SummaryHealthPercent, GenWorldUI.OverlayHealthTex, BaseContent.ClearTex, false);
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
			if (Widgets.ButtonInvisible(rect3))
			{
				Find.MainTabsRoot.EscapeCurrentTab(true);
                Find.CameraDriver.JumpTo(p.PositionHeld);
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


            tmpPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, Faction.OfPlayer);
            pawnHumanlikeStatDef = (from s in ((IEnumerable<StatDrawEntry>)statsToDraw.Invoke(null, new[] { tmpPawn })) where s.ShouldDisplay && s.stat != null select s.stat).OrderBy( stat => stat.LabelCap ).ToList();
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

        String numbersXMLPath
        {
            get
            {
                //TODO: FIX!!!
                return Path.Combine(GenFilePaths.ConfigFilePath, "kNumbers.config");
            }
        }

        public void writePresets()
        {
            
        }

        public void readPresets()
        {

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
                    ((p.Faction != null) && p.Faction.HostileTo(Faction.OfPlayer)) ||
                    (!p.RaceProps.Animal && (!p.RaceProps.Humanlike || p.RaceProps.IsMechanoid)) 
                   ) && 
                   !p.Position.Fogged() && (p.Position != IntVec3.Invalid);
        }

        bool isWildAnimal(Pawn p)
        {
            return p.RaceProps.Animal && (p.Faction != Faction.OfPlayer) && !p.Position.Fogged() && (p.Position != IntVec3.Invalid);
        }

        bool isGuest(Pawn p)
        {
            return
                   (p.guest != null) && !p.guest.IsPrisoner &&
                   (p.Faction != null) && !p.Faction.HostileTo(Faction.OfPlayer) && p.Faction != Faction.OfPlayer && 
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
                    tempPawns = (from p in Find.MapPawns.PawnsInFaction(Faction.OfPlayer) where p.RaceProps.Animal select p).Select(p => p as ThingWithComps).ToList();
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

                        case KListObject.objectType.Age:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(p => (p as Pawn).ageTracker.AgeBiologicalYearsFloat).ToList();
                            break;

                        case KListObject.objectType.ControlMedicalCare:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(p => (p as Pawn).playerSettings.medCare).ToList();
                            break;

                        case KListObject.objectType.CurrentJob:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(p => (p as Pawn).jobs.curDriver.GetReport()).ToList();
                            break;

                        case KListObject.objectType.AnimalMilkFullness:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(
                                p => {
                                    float f = -1;
                                    if ((p as Pawn).ageTracker.CurLifeStage.milkable)
                                    {
                                        var comp = (p as Pawn).AllComps.Where<ThingComp>(x => x is CompMilkable).FirstOrDefault();
                                        if (comp != null)
                                            f = ((CompMilkable)comp).Fullness;
                                    }
                                    return f;
                                }
                            ).ToList();
                            break;

                        case KListObject.objectType.AnimalWoolGrowth:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(
                                p => {
                                    float f = -1;
                                    if ((p as Pawn).ageTracker.CurLifeStage.milkable)
                                    {
                                        var comp = (p as Pawn).AllComps.Where<ThingComp>(x => x is CompShearable).FirstOrDefault();
                                        if (comp != null)
                                            f = ((CompShearable)comp).Fullness;
                                    }
                                    return f;
                                }
                            ).ToList();
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
            Find.WindowStack.Add(new FloatMenu(list));
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
            Find.WindowStack.Add(new FloatMenu(list));
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
            Find.WindowStack.Add(new FloatMenu(list));
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
            Find.WindowStack.Add(new FloatMenu(list));
        }

        //presets
        public void PresetOptionsMaker()
        {

        }

        //other hardcoded options
        public void OtherOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            
            //equipment bearers            
            if (new[] { pawnType.Colonists, pawnType.Prisoners, pawnType.Enemies, pawnType.Corpses }.Contains(chosenPawnType))
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.Gear, "koisama.Equipment".Translate(), null);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.Equipment".Translate(), action, MenuOptionPriority.Medium, null, null));
            }

            //all living things
            if (new[] { pawnType.Colonists, pawnType.Prisoners, pawnType.Enemies, pawnType.Animals, pawnType.WildAnimals, pawnType.Guests }.Contains(chosenPawnType))
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.Age, "koisama.Age".Translate(), null);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.Age".Translate(), action, MenuOptionPriority.Medium, null, null));
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

            if (chosenPawnType == pawnType.Animals)
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.AnimalMilkFullness, "MilkFullness".Translate(), null);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption("MilkFullness".Translate(), action, MenuOptionPriority.Medium, null, null));

                Action action2 = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.AnimalWoolGrowth, "WoolGrowth".Translate(), null);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption("WoolGrowth".Translate(), action2, MenuOptionPriority.Medium, null, null));
            }

            //healable
            if (new[] { pawnType.Colonists, pawnType.Prisoners, pawnType.Animals }.Contains(chosenPawnType))
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.ControlMedicalCare, "koisama.MedicalCare".Translate(), null);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.MedicalCare".Translate(), action, MenuOptionPriority.Medium, null, null));
            }

            if (! new[] { pawnType.Corpses, pawnType.AnimalCorpses }.Contains(chosenPawnType))
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.objectType.CurrentJob, "koisama.CurrentJob".Translate(), null);
                    if (fits(kl.minWidthDesired))
                        kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.CurrentJob".Translate(), action, MenuOptionPriority.Medium, null, null));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        public override void DoWindowContents(Rect r)
        {
            maxWindowWidth = Screen.width;
            base.DoWindowContents(r);

            if (pawnListUpdateNext < Find.TickManager.TicksGame)
                isDirty = true;

            if (isDirty)
            {
                UpdatePawnList();
            }

            Rect position = new Rect(0f, 0f, r.width, 115f);
            GUI.BeginGroup(position);

            float x = 0f;
            Text.Font = GameFont.Small;

            //pawn/prisoner list switch
            Rect sourceButton = new Rect(x, 0f, buttonWidth, PawnRowHeight);
            if (Widgets.ButtonText(sourceButton, ("koisama.pawntype." + chosenPawnType.ToString()).Translate()))
            {
                PawnSelectOptionsMaker();
            }
            x += buttonWidth + 10;
            TooltipHandler.TipRegion(sourceButton, new TipSignal("koisama.Numbers.ClickToToggle".Translate(), sourceButton.GetHashCode()));

            //stats btn
            Rect addColumnButton = new Rect(x, 0f, buttonWidth, PawnRowHeight);
            if (Widgets.ButtonText(addColumnButton, "koisama.Numbers.AddColumnLabel".Translate()))
            {
                StatsOptionsMaker();
            }
            x += buttonWidth + 10;

            //skills btn
            if (new[] { pawnType.Colonists, pawnType.Prisoners, pawnType.Enemies }.Contains(chosenPawnType))
            {
                Rect skillColumnButton = new Rect(x, 0f, buttonWidth, PawnRowHeight);
                if (Widgets.ButtonText(skillColumnButton, "koisama.Numbers.AddSkillColumnLabel".Translate()))
                {
                    SkillsOptionsMaker();
                }
                x += buttonWidth + 10;
            }

            //needs btn
            Rect needsColumnButton = new Rect(x, 0f, buttonWidth, PawnRowHeight);
            if (Widgets.ButtonText(needsColumnButton, "koisama.Numbers.AddNeedsColumnLabel".Translate()))
            {
                NeedsOptionsMaker();
            }
            x += buttonWidth + 10;

            Rect otherColumnBtn = new Rect(x, 0f, buttonWidth, PawnRowHeight);
            if (Widgets.ButtonText(otherColumnBtn, "koisama.Numbers.AddOtherColumnLabel".Translate()))
            {
                OtherOptionsMaker();
            }
            x += buttonWidth + 10;

            //TODO: implement
            /*
            Rect addPresetBtn = new Rect(x, 0f, buttonWidth, PawnRowHeight);
            if (Widgets.ButtonText(addPresetBtn, "koisama.Numbers.SetPresetLabel".Translate()))
            {
                PresetOptionsMaker();
            }
            x += buttonWidth + 10;
            */

            Rect thingCount = new Rect(10f, 45f, 200f, 30f);
            Widgets.Label(thingCount, "koisama.Numbers.Count".Translate() + ": " + this.things.Count().ToString());

            x = 0;
            //names
            Rect nameLabel = new Rect(x, 75f, NameColumnWidth, PawnRowHeight);
            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(nameLabel, "koisama.Numbers.Name".Translate());
            if (Widgets.ButtonInvisible(nameLabel))
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
            Text.Anchor = TextAnchor.MiddleCenter;

            for (int i=0;i<kList.Count; i++)
            {
                float colWidth = kList[i].minWidthDesired;

                if(colWidth + kListDesiredWidth + cFreeSpaceAtTheEnd > maxWindowWidth)
                {
                    break;
                }

                kListDesiredWidth += colWidth;

                Rect defLabel = new Rect(x-35, 25f + (offset ? 10f : 50f), colWidth+70 , 40f);
                Widgets.DrawLine(new Vector2(x + colWidth/2 , 55f + (offset ? 15f : 55f)), new Vector2(x + colWidth/2 , 113f), Color.gray, 1);
                Widgets.Label(defLabel, kList[i].label);

                StringBuilder labelSB = new StringBuilder();
                labelSB.AppendLine("koisama.Numbers.SortByTooltip".Translate(kList[i].label));
                labelSB.AppendLine("koisama.Numbers.RemoveTooltip".Translate());
                TooltipHandler.TipRegion(defLabel, labelSB.ToString());
                Widgets.DrawHighlightIfMouseover(defLabel);

                if (Widgets.ButtonInvisible(defLabel))
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
                if (colWidth + x + cFreeSpaceAtTheEnd > maxWindowWidth)
                {
                    //soft break
                    break;
                }
                Rect capCell = new Rect(x, y, colWidth, PawnRowHeight);
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

using RimWorld.Planet;

using System;
using Verse;
using Verse.AI.Group;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nandonalt_ColonyLeadership
{
    public class IncidentWorker_LeaderElection : IncidentWorker
    {
        
        public override bool TryExecute(IncidentParms parms)
        {
            List<Pawn> pawns = new List<Pawn>();
            pawns.AddRange(getAllColonists());
            int count;
            currentLeaders(out count);
          
            if (pawns.Count >= 5){
                if (count < 1)
                {
                    ElectLeader(new List<Pawn>());
                    ElectLeader(new List<Pawn>());
                }
                else if (count == 1)
                {
                    ElectLeader(new List<Pawn>());
                }
            }
            else
            {
                if (count < 1)
                {
                    ElectLeader(new List<Pawn>());
                }
            }
      
            return true;
        }

        public List<Pawn> currentLeaders(out int count)
        {
            List<Pawn> pawns = new List<Pawn>();
            int c = 0;
            foreach (Pawn current in getAllColonists())
            {
                Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                Hediff h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                Hediff h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                Hediff h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
                if (h1 != null || h2 != null || h3 != null || h4 != null) {pawns.Add(current); c++; }
         
            }
            count = c;
            return pawns;
        }

        public static List<Pawn> getAllColonists()
        {
          
            List<Pawn> pawns = new List<Pawn>();
            pawns.AddRange(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_FreeColonists);
            return pawns;
        }


        public bool ElectLeader(List<Pawn> toBeIgnored)
        {
            List<Pawn> toBeIgnoredt = toBeIgnored;
            List<Pawn> pawns = new List<Pawn>();
            List<Pawn> canBeVoted = new List<Pawn>();
            List<Pawn> tpawns = new List<Pawn>();
            List<Pawn> votes = new List<Pawn>();
            pawns.AddRange(getAllColonists());
            canBeVoted.AddRange(getAllColonists());
            List<Pawn> tpawns2 = new List<Pawn>();

            foreach (Pawn current in canBeVoted)
            {
                Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                Hediff h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                Hediff h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                Hediff h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
                Hediff h5 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leaderExpired"));
                if (h1 != null || h2 != null || h3 != null || h4 != null || h5 != null) { tpawns2.Add(current); }
                if (current.story.WorkTagIsDisabled(WorkTags.Social)) { tpawns2.Add(current); }
           }
            foreach (Pawn current in tpawns2)
            {
                canBeVoted.Remove(current);
            }
            foreach (Pawn current in toBeIgnored)
            {
                canBeVoted.Remove(current);
            }

            List<Pawn> bestOf = new List<Pawn>();
            Pawn bestBotanist = getBestOf(canBeVoted, getBotanistScore);
            Pawn bestWarrior = getBestOf(canBeVoted, getWarriorScore);
            Pawn bestCarpenter = getBestOf(canBeVoted, getCarpenterScore);
            Pawn bestScientist = getBestOf(canBeVoted, getScientistScore);


            if(bestBotanist != null) bestOf.Add(bestBotanist);
            if (bestWarrior != null) bestOf.Add(bestWarrior);
            if (bestCarpenter != null) bestOf.Add(bestCarpenter);
            if (bestScientist != null) bestOf.Add(bestScientist);

               
            String targetLeader = null;

            if (bestOf.NullOrEmpty() || canBeVoted.NullOrEmpty()) {
                Messages.Message("NoColonistAbleLeader".Translate(), MessageSound.Negative);
                return false;
            }

            foreach (Pawn current in pawns)
            {
                Pawn temp = current;
                int lastopinion = 0;
                foreach (Pawn p2 in bestOf)
                {
                    if (p2 != current)
                    {
                        int opinion = current.relations.OpinionOf(p2);
                        if (opinion > lastopinion)
                        {
                            lastopinion = opinion;
                            temp = p2;
                        }
                    }
                }
                votes.Add(temp);

            }

            Pawn most = (from i in votes
                         group i by i into grp
                         orderby grp.Count() descending
                         select grp.Key).First();

            Pawn pawn = most;

            float maxValue = new float[] { getBotanistScore(most), getWarriorScore(most), getCarpenterScore(most), getScientistScore(most) }.Max() ;

            /*  if (most == bestBotanist && maxValue == getBotanistScore(most)) targetLeader = "leader1";
             if (most == bestWarrior && maxValue == getWarriorScore(most)) targetLeader = "leader2";
              if (most == bestCarpenter && maxValue == getCarpenterScore(most)) targetLeader = "leader3";
              if (most == bestScientist && maxValue == getScientistScore(most)) targetLeader = "leader4";*/

            if (maxValue == getBotanistScore(most)) targetLeader = "leader1";
            if (maxValue == getWarriorScore(most)) targetLeader = "leader2";
            if (maxValue == getCarpenterScore(most)) targetLeader = "leader3";
            if (maxValue == getScientistScore(most)) targetLeader = "leader4";

            Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named(targetLeader), pawn, null);
            int count;

            if (toBeIgnored.Contains(pawn))
            {
                Messages.Message("Something bad happened on the election code. Try adding the leaders manually using dev mode.", MessageSound.Negative);
                return false;
            }

            foreach(Pawn p in currentLeaders(out count))
            {
                if (p.health.hediffSet.GetFirstHediffOfDef(hediff.def) != null)
                {
                    toBeIgnoredt.Add(pawn);
                    ElectLeader(toBeIgnoredt);
                    return false;
                } 
            }
            doElect(pawn, hediff);


           

            return true;
        }

        public static void doElect(Pawn pawn, Hediff hediff, bool forced = false)
        {
         hediff.Severity = 0.1f;
            pawn.health.AddHediff(hediff, null, null);
            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("ElectedLeader"));



            StringBuilder stringBuilder = new StringBuilder();
            if (forced) { stringBuilder.AppendLine("LeaderChosen".Translate(new object[] { pawn.Name.ToStringFull, hediff.LabelBase })); }
            else
            {
                
                    stringBuilder.AppendLine("LeaderElected".Translate(new object[] { pawn.Name.ToStringFull, hediff.LabelBase }));
             }
            if (Prefs.DevMode)
            {
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("--DEBUG-DEV---");
                stringBuilder.AppendLine("Botanist Score: " + getBotanistScore(pawn));
                stringBuilder.AppendLine("Warrior Score: " + getWarriorScore(pawn));
                stringBuilder.AppendLine("Carpenter Score: " + getCarpenterScore(pawn));
                stringBuilder.AppendLine("Scientist Score: " + getScientistScore(pawn));
            }
            if (Utility.getGov() != null)
            {
                Find.LetterStack.ReceiveLetter("NewLeaderLetterTitle".Translate(new object[] { Utility.getGov().nameMale }), stringBuilder.ToString(), LetterDefOf.Good, pawn, null);
            }
            else
            {
                Find.LetterStack.ReceiveLetter("New Leader", stringBuilder.ToString(), LetterDefOf.Good, pawn, null);
            }

            foreach (Pawn p in getAllColonists())
            {
                if (p != pawn)
                {
                    int num2 = p.relations.OpinionOf(pawn);
                    if (num2 <= -20)
                    {
                        p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("RivalLeader"), null);
                    }
                    if (p.story.traits.HasTrait(TraitDef.Named("Jealous")) && TeachingUtility.leaderH(p) == null)
                    { p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("ElectedLeaderJealous"), null); }
                }
            }
}

        public static bool TryStartGathering(Map map)
        {
            Pawn pawn = PartyUtility.FindRandomPartyOrganizer(Faction.OfPlayer, map);
            if (pawn == null)
            {
                Messages.Message("ElectionFail_ColonistsNotFound".Translate(), MessageSound.RejectInput);
                return false;
            }
            IntVec3 intVec;
            if (!RCellFinder.TryFindPartySpot(pawn, out intVec))
            {
                Messages.Message("Couldn't find a suitable safe spot for the election.", MessageSound.RejectInput);
                return false;
            }
            LordMaker.MakeNewLord(pawn.Faction, new LordJob_Joinable_LeaderElection(intVec), map, null);
            Find.LetterStack.ReceiveLetter("Election".Translate(), "ElectionGathering".Translate(), LetterDefOf.Good, new TargetInfo(intVec, map, false), null);
            return true;
        }


        public Pawn getBestOf(List<Pawn> pawns, Func<Pawn, float> getScore)
        {
            Pawn selected = null;
            float lastScore = 0;
            foreach (Pawn current in pawns)
            {           
                float score = getScore(current); 
                if(score > lastScore)
                {
                    selected = current;
                    lastScore = score;
                }                                       
            }
            if (lastScore == 0) return null;       
            return selected;
        }
        
        public static float getBotanistScore(Pawn pawn)
        {
            if (pawn.story.WorkTagIsDisabled(WorkTags.PlantWork)) return 0;
            if (pawn.story.WorkTypeIsDisabled(WorkTypeDefOf.Growing)) return 0;
            float a = pawn.skills.GetSkill(SkillDefOf.Growing).Level * getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Growing)) * 0.4f;
            float b = pawn.skills.GetSkill(SkillDefOf.Medicine).Level * getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Medicine)) * 0.3f;
            float c = pawn.skills.GetSkill(SkillDefOf.Animals).Level * getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Animals)) * 0.3f;
            return (a + b + c);
        }

        public static float getWarriorScore(Pawn pawn)
        {
            if (pawn.story.WorkTagIsDisabled(WorkTags.Violent)) return 0;
            float a = pawn.skills.GetSkill(SkillDefOf.Shooting).Level * getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Shooting));
            float b = pawn.skills.GetSkill(SkillDefOf.Melee).Level * getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Melee));
            return (a + b) / 2;
        }

        public static float getScientistScore(Pawn pawn)
        {
            if (pawn.story.WorkTagIsDisabled(WorkTags.Intellectual)) return 0;
            float a = pawn.skills.GetSkill(SkillDefOf.Intellectual).Level * getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Intellectual)) *  1.5f;
            return a;
        }

        public static float getCarpenterScore(Pawn pawn)
        {
            if (pawn.story.WorkTagIsDisabled(WorkTags.Crafting)) return 0;
            if (pawn.story.WorkTagIsDisabled(WorkTags.ManualSkilled)) return 0;
            float a = pawn.skills.GetSkill(SkillDefOf.Construction).Level * getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Construction)) * 0.4f;
            float b = pawn.skills.GetSkill(SkillDefOf.Crafting).Level * getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Crafting)) * 0.4f;
            float c = pawn.skills.GetSkill(SkillDefOf.Artistic).Level * getPassionFactor(pawn.skills.GetSkill(SkillDefOf.Artistic)) * 0.2f;
            return (a + b + c);
        }

        public static float getOverallScore(Pawn pawn)
        {
            return getBotanistScore(pawn) + getWarriorScore(pawn) + getScientistScore(pawn) + getCarpenterScore(pawn);
        }

        public static float getPassionFactor(SkillRecord skill)
        {
            if (skill.passion == Passion.Major) return 1.5f;
            else if (skill.passion == Passion.Minor) return 1.25f; 
                return 1f;
          }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace JTFieldSurgery
{

    class Recipe_FieldAmputate : Recipe_Surgery
    {
        ///How much should relationship be damaged if we decapitate or otherwise mutilate? (Terrorism 101)
        const float ViolationGoodwillImpact = -40.0f;

        //True if the body part has nothing wrong with it, false otherwise
        static bool IsClean (Pawn pawn, BodyPartRecord part)
        {
            return (Verse.BodyPartRemovalIntent.Harvest == HealthUtility.PartRemovalIntent (pawn, part));
        }

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn (Pawn pawn, RecipeDef recipe)
        {
            var severable = new List<BodyPartRecord> ();

            //Allow severing every malignant natural body part that is accessible at surface depth (arms, legs, toes, fingers, eyes, head, etc.)
            var parts = pawn.RaceProps.body.AllParts;
            foreach (BodyPartRecord part in parts) {
                if (part == pawn.RaceProps.body.corePart) continue;
                if (pawn.health.hediffSet.PartIsMissing (part)) continue;
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts (part)) continue;
                if (part.def.isConceptual) continue;

                //New in version 4, we can now selectively ignore ethics by holding Shift
                if (!UnityEngine.Input.GetKey (UnityEngine.KeyCode.RightShift) && !UnityEngine.Input.GetKey (UnityEngine.KeyCode.LeftShift)) {
                    //We no longer check to see if the pawn is a prisoner -- prisoners are entitled to the same human rights as colonists
                    if (IsClean (pawn, part)) continue;
                    if (part.def.dontSuggestAmputation) continue;
                }

                if (part.depth == BodyPartDepth.Outside) severable.Add (part);
            }

            //Allow subtargetting out any non-external body part that is known to be malignant (lungs, kidney, liver, etc.)
            //TODO: Perhaps gate this on the existence of some kind of research? search for EPOE/RBSE?
            var injuries = pawn.health.hediffSet.GetInjuredParts ();
            foreach (BodyPartRecord part in injuries) {
                if (part == pawn.RaceProps.body.corePart) continue;
                if (part.def.dontSuggestAmputation) continue;
                if (part.depth != BodyPartDepth.Outside) severable.Add (part);
            }

            //Even if C# does automatic garbage collection, I don't like returning a new object without knowing it'll get nuked at some point...
            return severable;
        }

        public override bool IsViolationOnPawn (Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
        {
            return pawn.Faction != billDoerFaction && IsClean (pawn, part);
        }

        public void SpawnThingsFromHediffs (Pawn pawn, BodyPartRecord part, IntVec3 pos, Map map)
        {
            if (!pawn.health.hediffSet.GetNotMissingParts ().Contains (part)) {
                return;
            }

            foreach (Hediff current in pawn.health.hediffSet.hediffs) {
                if (current.Part != part) continue;
                if (current.def.spawnThingOnRemoved != null)
                    GenSpawn.Spawn (current.def.spawnThingOnRemoved, pos, map);
            }
            for (int i = 0; i < part.parts.Count; i++) {
                SpawnThingsFromHediffs (pawn, part.parts [i], pos, map);
            }
        }

        public override void ApplyOnPawn (Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            //If the part has no injuries, it's torture -- otherwise it's emergency surgery
            bool torture = IsClean (pawn, part);
            //Technically, we could refine this a bit more... it's not necessary to chop a limb off because they
            // have a broken bone.  However, this is a "good enough" solution.  It starts to get to the point of
            // subjectiveness on whether the surgery was valid or not.

            ///Amount of pain and suffering caused by removal of this part.
            float maxDamage = (float)part.def.hitPoints / 37.5f;

            bool failed = false;
            if (billDoer != null) {
                failed = CheckSurgeryFail(billDoer, pawn, ingredients, part, bill);
                if (!failed) {
                    TaleRecorder.RecordTale (TaleDefOf.DidSurgery, new object [] { billDoer, pawn });
                }
                SpawnThingsFromHediffs (pawn, part, billDoer.Position, billDoer.Map);
            }

            //Always dismember the part regardless of success
            if (!pawn.health.hediffSet.PartIsMissing (part)) { //make sure any surgery failure didn't already chop off the part for us... yay...?
                var dinfo = new DamageInfo (failed ? DamageDefOf.Cut : DamageDefOf.SurgicalCut, part.def.hitPoints, -1f, null, part, null);
                pawn.TakeDamage (dinfo); //[imagine sound effect: dull, sticky thud]
            }

            if (failed) {
                //whoops, I think I snagged an artery
                float severity = UnityEngine.Random.Range (0f, maxDamage);
                if (!pawn.health.hediffSet.HasHediff (HediffDefOf.BloodLoss))
                    pawn.health.AddHediff (HediffDefOf.BloodLoss, null, null);
                HealthUtility.AdjustSeverity (pawn, HediffDefOf.BloodLoss, severity);

                //New in version 4: more filth! yay!
                int arterial_bleeding = (int)Math.Sqrt (severity * 50f);
                while (arterial_bleeding-- > 0) {
                    if (pawn.RaceProps.IsFlesh) pawn.health.DropBloodFilth ();
                }
            }

            ///// Version 4 additions /////

            //Add wounds to body part(s) containing the part in question (either the next higher joint for a limb, or the surrounding body part for an organ)
            //If removing a cube, we need an opening as large as one of its sides (which is the 2/3 power -- 1/3 power is incision, 2/3 power is hole, 3/3 power is full avulsion)
            //Since most body parts in real life tend toward flat and oblong this isn't the best rule of thumb to follow, but it works great for its inherent simplicity.
            //e.g., to remove lung (20 HP) we will cause 1+floor(20**2/3) = 8 HP of damage to the torso to get it out
            bool reachedsurface = false; //(part.depth != BodyPartDepth.Inside);
            var currentpart = part.parent;
            int partdamage = 1 + (int)(Math.Pow (part.def.hitPoints * part.def.hitPoints, 0.33333));
            while (partdamage > 0 && !reachedsurface && currentpart != null) {
                var dinfo2 = new DamageInfo (failed ? DamageDefOf.Cut : DamageDefOf.SurgicalCut, partdamage, -1f, null, currentpart, null);
                pawn.TakeDamage (dinfo2);

                if (currentpart.depth == BodyPartDepth.Outside) reachedsurface = true;
                currentpart = currentpart.parent;
            }

            //Produce blood
            if (pawn.RaceProps.IsFlesh) pawn.health.DropBloodFilth ();

            //Produce waste chunks of meat (getting into Mad Max (2015) levels of squick here) except when removing solid parts (bones)
            int meatCount = (int)(part.def.hitPoints / 2.0f);
            if (meatCount > 0 && pawn.RaceProps.IsFlesh && pawn.RaceProps.meatDef != null && !part.def.IsSolid (part, pawn.health.hediffSet.hediffs)) {
                var meat = ThingMaker.MakeThing (pawn.RaceProps.meatDef, null);
                meat.stackCount = Verse.Rand.Range (Math.Max (meatCount / 2, 1), meatCount);
                GenPlace.TryPlaceThing (meat, pawn.Position, billDoer.Map, ThingPlaceMode.Near, null);
            }

            //Cause pain from the procedure
            var pain = LocalDefOf.JTFieldSurgeryPain;
            if (pain != null) {
                HealthUtility.AdjustSeverity (pawn, pain, maxDamage);

                var removed_parts = new List<BodyPartRecord> ();
                removed_parts.Add (part);

                //Increase pain based on every extremity also lost (phantom pain, so only by half)
                foreach (BodyPartRecord child in pawn.RaceProps.body.AllParts) {
                    BodyPartRecord core = pawn.RaceProps.body.corePart;

                    //I presume no guarantees that this list is serialised, so parts will all attempt to trace back to the
                    // core to see if they can find any other removed parts along the way.
                    BodyPartRecord tracetocore = child.parent;
                    while (tracetocore != core && tracetocore is BodyPartRecord) {
                        if (removed_parts.Contains (tracetocore)) {
                            removed_parts.Add (child);
                            HealthUtility.AdjustSeverity (pawn, pain, (float)part.def.hitPoints / 75f);
                            break;
                        }

                        tracetocore = tracetocore.parent;
                    }
                }
            }

            //Apply thoughts
            if (!pawn.RaceProps.Humanlike) {
                return;
            }

            if (pawn.Dead) {
                if (torture) { //A particularly brutal execution
                    ThoughtUtility.GiveThoughtsForPawnExecuted (pawn, PawnExecutionKind.GenericBrutal);

                    if (billDoer.story.traits.HasTrait (TraitDefOf.Psychopath)) {
                        Thought_Memory torturerSadist = (Thought_Memory)ThoughtMaker.MakeThought (LocalDefOf.JTFieldSurgeryTortureSadist);
                        if (torturerSadist != null)
                            billDoer.needs.mood.thoughts.memories.TryGainMemory (torturerSadist, pawn);
                    } else {
                        Thought_Memory torturerNormal = (Thought_Memory)ThoughtMaker.MakeThought (LocalDefOf.JTFieldSurgeryTorturer);
                        if (torturerNormal != null)
                            billDoer.needs.mood.thoughts.memories.TryGainMemory (torturerNormal, pawn);
                    }
                } else { //Emergency surgery			
                         //Each non-psychopathic colonist will feel bad that someone died because of their lack of preparation
                    var colonistDiedSurgery = (Thought_Memory)ThoughtMaker.MakeThought (LocalDefOf.JTFieldSurgeryColonistDiedSurgery);
                    if (colonistDiedSurgery != null) {
                        Map current_map = billDoer.Map;
                        foreach (Pawn current in current_map.mapPawns.PawnsInFaction (billDoer.Faction)) {
                            if (current != pawn && current.IsColonist && !current.story.traits.HasTrait (TraitDefOf.Psychopath))
                                current.needs.mood.thoughts.memories.TryGainMemory (colonistDiedSurgery, pawn);
                        }
                    }

                    //Non-psychopathic doctors will feel bad if someone dies under the knife because they didn't use anaesthetic
                    var killedFieldSurgery = (Thought_Memory)ThoughtMaker.MakeThought (LocalDefOf.JTFieldSurgeryKilledFieldSurgery);
                    if (killedFieldSurgery != null && !billDoer.story.traits.HasTrait (TraitDefOf.Psychopath))
                        billDoer.needs.mood.thoughts.memories.TryGainMemory (killedFieldSurgery, pawn);
                }
            } else {
                //Always occurs: if tortured, upset about torture; if not tortured, upset about lack of anaesthetic
                var harmedMe = (Thought_Memory)ThoughtMaker.MakeThought (ThoughtDefOf.HarmedMe);
                pawn.needs.mood.thoughts.memories.TryGainMemory (harmedMe, billDoer);

                if (torture) {
                    var tortureMasochist = (Thought_Memory)ThoughtMaker.MakeThought (LocalDefOf.JTFieldSurgeryTortureMasochist);
                    if (tortureMasochist != null && pawn.story.traits.HasTrait (LocalDefOf.Masochist) && pawn.story.traits.HasTrait (LocalDefOf.Prosthophile)) {
                        pawn.needs.mood.thoughts.memories.TryGainMemory (tortureMasochist, billDoer);
                    } else {
                        var tortured = (Thought_Memory)ThoughtMaker.MakeThought (LocalDefOf.JTFieldSurgeryTortureVictim);
                        if (tortured != null) {
                            pawn.needs.mood.thoughts.memories.TryGainMemory (tortured, billDoer);
                        }
                    }

                    //Every colonist who is part of the colony is an accessory to torture
                    var tortureWitness = (Thought_Memory)ThoughtMaker.MakeThought (LocalDefOf.JTFieldSurgeryTortureWitness);
                    if (tortureWitness != null) {
                        foreach (Pawn current in pawn.Map.mapPawns.FreeColonists) {
                            if (current != pawn && current.IsColonist &&
                                !current.story.traits.HasTrait (TraitDefOf.Psychopath) &&
                                !current.story.traits.HasTrait (TraitDefOf.Cannibal))

                                current.needs.mood.thoughts.memories.TryGainMemory (tortureWitness, billDoer);
                        }
                    }

                    //The person committing the torture will feel good if psychopath or cannibal, bad if normal
                    //New in version 4, this also applies to cannibals (as they have obtained some meat chunks)
                    if (billDoer.story.traits.HasTrait (TraitDefOf.Psychopath) ||
                        billDoer.story.traits.HasTrait (TraitDefOf.Cannibal)) {

                        var torturerSadist = (Thought_Memory)ThoughtMaker.MakeThought (LocalDefOf.JTFieldSurgeryTortureSadist);
                        if (torturerSadist != null)
                            billDoer.needs.mood.thoughts.memories.TryGainMemory (torturerSadist, pawn);
                    } else {
                        var torturerNormal = (Thought_Memory)ThoughtMaker.MakeThought (LocalDefOf.JTFieldSurgeryTorturer);
                        if (torturerNormal != null)
                            billDoer.needs.mood.thoughts.memories.TryGainMemory (torturerNormal, pawn);
                    }
                }
            }
            if (IsViolationOnPawn (pawn, part, Faction.OfPlayer)) pawn.Faction.AffectGoodwillWith (billDoer.Faction, ViolationGoodwillImpact);
        }

    }

}

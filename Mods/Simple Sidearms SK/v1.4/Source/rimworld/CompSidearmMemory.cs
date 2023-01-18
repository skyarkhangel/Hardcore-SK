using PeteTimesSix.SimpleSidearms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

using static PeteTimesSix.SimpleSidearms.SimpleSidearms;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;

namespace SimpleSidearms.rimworld
{
    public class CompSidearmMemory : ThingComp
    {

        public List<ThingDefStuffDefPair> rememberedWeapons = new List<ThingDefStuffDefPair>();
        public List<ThingDefStuffDefPair> RememberedWeapons
        {
            get
            {
                //Log.Message("RememberedWeapons Called");
                if (rememberedWeapons == null)
                    generateRememberedWeaponsFromEquipped();
                if (!nullchecked)
                    NullChecks();
                List<ThingDefStuffDefPair> fakery = new List<ThingDefStuffDefPair>();
                foreach (var wep in rememberedWeapons)
                    fakery.Add(wep);
                return fakery;
            }
        }

        public bool forcedUnarmedEx = false;
        public ThingDefStuffDefPair? forcedWeaponEx = null;
        public bool forcedUnarmedWhileDraftedEx = false;
        public ThingDefStuffDefPair? forcedWeaponWhileDraftedEx = null;

        public bool preferredUnarmedEx = false;
        public ThingDefStuffDefPair? defaultRangedWeaponEx = null;
        public ThingDefStuffDefPair? preferredMeleeWeaponEx = null;

        public PrimaryWeaponMode primaryWeaponMode = PrimaryWeaponMode.BySkill;

        public bool currentJobWeaponReequipDelayed;
        public JobDef autotoolJob = null;
        public Toil autotoolToil = null;
        public int delayIdleSwitchTimestamp = -60;

        public bool ForcedUnarmed
        {
            get
            {
                return forcedUnarmedEx;
            }
            set
            {
                if (value == true)
                    ForcedWeapon = null;
                forcedUnarmedEx = value;
            }
        }
        public ThingDefStuffDefPair? ForcedWeapon
        {
            get
            {
                if (forcedWeaponEx == null)
                    return null;
                return forcedWeaponEx.Value;
            }
            set
            {
                if (value == null)
                    forcedWeaponEx = null;
                else
                    forcedWeaponEx = value;
            }
        }

        public bool ForcedUnarmedWhileDrafted
        {
            get
            {
                return forcedUnarmedWhileDraftedEx;
            }
            set
            {
                if (value == true)
                    ForcedWeaponWhileDrafted = null;
                forcedUnarmedWhileDraftedEx = value;
            }
        }
        public ThingDefStuffDefPair? ForcedWeaponWhileDrafted
        {
            get
            {
                if (forcedWeaponWhileDraftedEx != null)
                    return forcedWeaponWhileDraftedEx.Value;
                else
                    return null;
            }
            set
            {
                if (value == null)
                    forcedWeaponWhileDraftedEx = null;
                else
                    forcedWeaponWhileDraftedEx = value;
            }
        }


        public bool PreferredUnarmed
        {
            get
            {
                return preferredUnarmedEx;
            }
            set
            {
                if (value == true)
                    PreferredMeleeWeapon = null;
                preferredUnarmedEx = value;
            }
        }
        public ThingDefStuffDefPair? DefaultRangedWeapon
        {
            get
            {
                if (defaultRangedWeaponEx == null)
                    return null;
                return defaultRangedWeaponEx.Value;
            }
            set
            {
                if (value == null)
                    defaultRangedWeaponEx = null;
                else
                    defaultRangedWeaponEx = value;
            }
        }
        public ThingDefStuffDefPair? PreferredMeleeWeapon
        {
            get
            {
                if (preferredMeleeWeaponEx == null)
                    return null;
                return preferredMeleeWeaponEx.Value;
            }
            set
            {
                if (value == null)
                    preferredMeleeWeaponEx = null;
                else
                    preferredMeleeWeaponEx = value;
            }
        }

        public Pawn Owner { get { return this.parent as Pawn; } }

        public CompSidearmMemory() : base()
        {
            if (Owner != null)
            {
                if (Owner.IsColonist)
                    primaryWeaponMode = Settings.ColonistDefaultWeaponMode;
                else
                    primaryWeaponMode = Settings.NPCDefaultWeaponMode;

                if (primaryWeaponMode == PrimaryWeaponMode.ByGenerated)
                {
                    if (Owner.equipment == null || Owner.equipment.Primary == null)
                        primaryWeaponMode = PrimaryWeaponMode.BySkill;
                    else if (Owner.equipment.Primary.def.IsRangedWeapon)
                        primaryWeaponMode = PrimaryWeaponMode.Ranged;
                    else if (Owner.equipment.Primary.def.IsMeleeWeapon)
                        primaryWeaponMode = PrimaryWeaponMode.Melee;
                    else
                        primaryWeaponMode = PrimaryWeaponMode.BySkill;
                }
            }
        }

        public CompSidearmMemory(bool fillExisting)
        {
            this.rememberedWeapons = new List<ThingDefStuffDefPair>();
            if (fillExisting)
            {
                generateRememberedWeaponsFromEquipped();
            }
            if (Owner != null) //null owner should only come up when loading from savegames
            {
                
            }
        }

        public void generateRememberedWeaponsFromEquipped()
        {
            this.rememberedWeapons = new List<ThingDefStuffDefPair>();
            IEnumerable<ThingWithComps> carriedWeapons = Owner.getCarriedWeapons(includeTools: true);
            foreach (ThingWithComps weapon in carriedWeapons)
            {
                ThingDefStuffDefPair pair = new ThingDefStuffDefPair(weapon.def, weapon.Stuff);
                rememberedWeapons.Add(pair);
            }
        }

        public override void PostExposeData()
        {
            Scribe_Collections.Look<ThingDefStuffDefPair>(ref rememberedWeapons, "rememberedWeapons", LookMode.Deep);

            Scribe_Deep.Look<ThingDefStuffDefPair?>(ref forcedWeaponEx, "forcedWeapon");
            Scribe_Values.Look<bool>(ref forcedUnarmedEx, "forcedUnarmed");
            Scribe_Deep.Look<ThingDefStuffDefPair?>(ref forcedWeaponWhileDraftedEx, "forcedWeaponWhileDrafted");
            Scribe_Values.Look<bool>(ref forcedUnarmedWhileDraftedEx, "forcedUnarmedWhileDrafted");

            Scribe_Values.Look<bool>(ref preferredUnarmedEx, "preferredUnarmed");
            Scribe_Deep.Look<ThingDefStuffDefPair?>(ref defaultRangedWeaponEx, "defaultRangedWeapon");
            Scribe_Deep.Look<ThingDefStuffDefPair?>(ref preferredMeleeWeaponEx, "preferredMeleeWeapon");

            Scribe_Values.Look<PrimaryWeaponMode>(ref primaryWeaponMode, "primaryWeaponMode");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                NullChecks();
            }
        }

        public static CompSidearmMemory GetMemoryCompForPawn(Pawn pawn, bool fillExistingIfCreating = true)
        {
            if (pawn == null)
                return null;
            if (!pawn.RaceProps.Humanlike)
            {
                Log.Warning("CompSidearmMemory accessed for non-humanlike pawn " + pawn.Label);
                return null;

            }
            CompSidearmMemory memory = pawn.TryGetComp<CompSidearmMemory>();
            if (memory == null)
                return null;

            return memory;
        }

        public bool IsCurrentWeaponForced(bool alsoCountPreferredOrDefault)
        {
            if (Owner == null || Owner.Dead || Owner.equipment == null)
                return false;
            ThingDefStuffDefPair? currentWeapon = Owner.equipment.Primary?.toThingDefStuffDefPair();
            if (currentWeapon == null)
            {
                if (Owner.Drafted && ForcedUnarmedWhileDrafted)
                    return true;
                else if (ForcedUnarmed)
                    return true;
                else if (alsoCountPreferredOrDefault && PreferredUnarmed)
                    return true;
                else
                    return false;
            }
            else
            {
                if (Owner.Drafted && ForcedWeaponWhileDrafted == currentWeapon)
                    return true;
                else if (ForcedWeapon == currentWeapon)
                    return true;
                else if (alsoCountPreferredOrDefault)
                {
                    if (currentWeapon.Value.thing.IsMeleeWeapon && PreferredMeleeWeapon == currentWeapon)
                        return true;
                    else if (currentWeapon.Value.thing.IsRangedWeapon && DefaultRangedWeapon == currentWeapon)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        public void SetUnarmedAsForced(bool drafted)
        {
            if (drafted)
            {
                ForcedUnarmedWhileDrafted = true;
                ForcedWeaponWhileDrafted = null;
            }
            else
            {
                ForcedUnarmed = true;
                ForcedWeapon = null;
            }
        }

        public void SetWeaponAsForced(ThingDefStuffDefPair weapon, bool drafted)
        {
            if (drafted)
            {
                ForcedUnarmedWhileDrafted = false;
                ForcedWeaponWhileDrafted = weapon;
            }
            else
            {
                ForcedUnarmed = false;
                ForcedWeapon = weapon;
            }
        }


        public void UnsetUnarmedAsForced(bool drafted)
        {
            if (drafted)
            {
                ForcedUnarmedWhileDrafted = false;
                ForcedWeaponWhileDrafted = null;
            }
            else
            {
                ForcedUnarmed = false;
                ForcedWeapon = null;
            }
        }

        public void UnsetForcedWeapon(bool drafted)
        {
            if (drafted)
            {
                ForcedUnarmedWhileDrafted = false;
                ForcedWeaponWhileDrafted = null;
            }
            else
            {
                ForcedUnarmed = false;
                ForcedWeapon = null;
            }
        }

        public void SetRangedWeaponTypeAsDefault(ThingDefStuffDefPair rangedWeapon)
        {
            this.DefaultRangedWeapon = rangedWeapon;
            if (this.ForcedWeapon != null && this.ForcedWeapon != rangedWeapon && this.ForcedWeapon.Value.thing.IsRangedWeapon)
                UnsetForcedWeapon(false);
        }
        public void SetMeleeWeaponTypeAsPreferred(ThingDefStuffDefPair meleeWeapon)
        {
            this.preferredUnarmedEx = false;
            this.PreferredMeleeWeapon = meleeWeapon;
            if (this.ForcedWeapon != null && this.ForcedWeapon != meleeWeapon && this.ForcedWeapon.Value.thing.IsMeleeWeapon)
                UnsetForcedWeapon(false);
            if (ForcedUnarmed)
                UnsetUnarmedAsForced(false);
        }
        public void SetUnarmedAsPreferredMelee()
        {
            PreferredUnarmed = true;
            PreferredMeleeWeapon = null;
            if (this.ForcedWeapon != null && this.ForcedWeapon.Value.thing.IsMeleeWeapon)
                UnsetForcedWeapon(false);
        }

        public void UnsetRangedWeaponDefault()
        {
            DefaultRangedWeapon = null;
        }
        public void UnsetMeleeWeaponPreference()
        {
            PreferredMeleeWeapon = null;
            PreferredUnarmed = false;
        }

        public void InformOfUndraft()
        {
            ForcedWeaponWhileDrafted = null;
            ForcedUnarmedWhileDrafted = false;
        }

        public void InformOfAddedPrimary(Thing weapon)
        {
            if (weapon != null)
            {
                InformOfAddedSidearm(weapon);

                if (weapon.def.IsRangedWeapon)
                    SetRangedWeaponTypeAsDefault(weapon.toThingDefStuffDefPair());
                else
                    SetMeleeWeaponTypeAsPreferred(weapon.toThingDefStuffDefPair());
            }
        }
        public void InformOfAddedSidearm(Thing weapon)
        {
            if (weapon != null)
            {
                ThingDefStuffDefPair weaponType = weapon.toThingDefStuffDefPair();
                var carriedOfType = Owner.getCarriedWeapons(includeTools: true).Where(w => w.toThingDefStuffDefPair() == weaponType);
                var rememberedOfType = rememberedWeapons.Where(w => w == weaponType);

                if (rememberedOfType.Count() < carriedOfType.Sum(c => c.stackCount))
                    rememberedWeapons.Add(weapon.toThingDefStuffDefPair());
            }
        }

        public void InformOfDroppedSidearm(Thing weapon, bool intentional)
        {
            if (weapon != null)
            {
                if (intentional)
                    ForgetSidearmMemory(weapon.toThingDefStuffDefPair());
            }
        }

        public void ForgetSidearmMemory(ThingDefStuffDefPair weaponMemory)
        {
            if (rememberedWeapons.Contains(weaponMemory))
                rememberedWeapons.Remove(weaponMemory);

            if (!rememberedWeapons.Contains(weaponMemory)) //only remove if this was the last instance
            {
                if (weaponMemory == ForcedWeapon)
                    ForcedWeapon = null;
                if (weaponMemory == PreferredMeleeWeapon)
                    PreferredMeleeWeapon = null;
                if (weaponMemory == DefaultRangedWeapon)
                    DefaultRangedWeapon = null;
            }
        }

        public bool IsUsingAutotool(bool withDelay, bool withJobLag)
        {
            if (!Settings.ToolAutoSwitch)
                return false;
            if (this.autotoolToil != null)
            {
                return true;
            }
            CheckIfStillOnAutotoolJob();
            if (this.autotoolJob != null)
            {
                return true;
            }
            if (withDelay)
            {
                if ((Find.TickManager.TicksGame - this.delayIdleSwitchTimestamp) < 60)
                {
                    return true;
                }
            }
            if (withJobLag)
            {
                if (this.currentJobWeaponReequipDelayed)
                {
                    return true;
                }
            }
            return false;
        }

        public void CheckIfStillOnAutotoolJob()
        {
            if (Owner.CurJobDef == null)
            {
                return;
            }
            if (Owner.CurJobDef != autotoolJob)
            {
                //Log.Message("autotool job no longer active, new job is: " + (Owner.CurJob == null ? "null" : Owner.CurJob.def.defName));
                autotoolJob = null;
            }
        }

        public bool nullchecked = false;

        public void NullChecks()
        {
            //Log.Message("NullChecks Run");
            if (nullchecked)
                return;
            if (rememberedWeapons == null)
            {
                //Log.Warning("Remembered weapons list of " + this.Owner.LabelCap + " was missing, regenerating...");
                generateRememberedWeaponsFromEquipped();
            }
            for (int i = rememberedWeapons.Count() - 1; i >= 0; i--)
            {
                try
                {
                    var disposed = rememberedWeapons[i];
                }
                catch (Exception ex)
                {
                    Log.Warning("A memorised weapon of " + this.Owner.LabelCap + " had a missing def or malformed data, removing. Exception:" + ex.Message);
                    rememberedWeapons.RemoveAt(i);
                }
            }
            if (PreferredMeleeWeapon != null)
            {
                try
                {
                    var disposed = PreferredMeleeWeapon.Value;
                }
                catch (Exception ex)
                {
                    Log.Warning("Melee weapon preference of " + this.Owner.LabelCap + " had a missing def or malformed data, removing. Exception:" + ex.Message);
                    PreferredMeleeWeapon = null;
                }
            }
            if (DefaultRangedWeapon != null)
            {
                try
                {
                    var disposed = DefaultRangedWeapon.Value;
                }
                catch (Exception ex)
                {
                    Log.Warning("Ranged weapon preference of " + this.Owner.LabelCap + " had a missing def or malformed data, removing. Exception:" + ex.Message);
                    DefaultRangedWeapon = null;
                }
            }
            if (ForcedWeapon != null)
            {
                try
                {
                    var disposed = ForcedWeapon.Value;
                }
                catch (Exception ex)
                {
                    Log.Warning("Forced weapon of " + this.Owner.LabelCap + " had a missing def or malformed data, removing. Exception:" + ex.Message);
                    ForcedWeapon = null;
                }
            }
            if (ForcedWeaponWhileDrafted != null)
            {
                try
                {
                    var disposed = ForcedWeaponWhileDrafted.Value;
                }
                catch (Exception ex)
                {
                    Log.Warning("Forced drafted weapon of " + this.Owner.LabelCap + " had a missing def or malformed data, removing. Exception:" + ex.Message);
                    ForcedWeaponWhileDrafted = null;
                }
            }
            checkRememberedWeaponsForNulls();
            nullchecked = true;
        }


        public void checkRememberedWeaponsForNulls()
        {
            for (int i = rememberedWeapons.Count - 1; i >= 0; i--)
            {
                if (rememberedWeapons[i].thing == null)
                {
                    rememberedWeapons.RemoveAt(i);
                    Log.Warning(Owner.Label + " had null weapon memory, removing...");
                }
            }
        }
    }


    public class CompProperties_SidearmMemory : CompProperties
    {
        // Token: 0x06004E14 RID: 19988 RVA: 0x0019FC73 File Offset: 0x0019DE73
        public CompProperties_SidearmMemory()
        {
            this.compClass = typeof(CompSidearmMemory);
        }
    }
}

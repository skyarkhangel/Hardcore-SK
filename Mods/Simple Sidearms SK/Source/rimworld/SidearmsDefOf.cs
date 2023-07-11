using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.rimworld
{
    [DefOf]
    public static class SidearmsDefOf
    {
        public static JobDef EquipSecondary;

        public static JobDef ReequipSecondary;
        public static JobDef ReequipSecondaryCombat;

        public static ConceptDef Concept_SimpleSidearmsBasic;

        public static ConceptDef Concept_SimpleSidearmsPreference;

        public static ConceptDef Concept_SidearmsDropping;

        public static ConceptDef Concept_SidearmsMissing;

        public static ConceptDef Concept_SimpleSidearmsAdvancedRanged;
        public static ConceptDef Concept_SimpleSidearmsAdvancedMelee;
        public static ConceptDef Concept_SimpleSidearmsAdvancedDrafted;

        public static ConceptDef Concept_SimpleSidearmsLosingWeapons;

        public static ConceptDef Concept_CEOverride;
    }
}

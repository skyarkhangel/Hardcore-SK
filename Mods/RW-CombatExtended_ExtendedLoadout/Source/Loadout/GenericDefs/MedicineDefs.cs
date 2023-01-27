using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using System;

namespace CombatExtended.ExtendedLoadout;

public class MedicineDefs
{
    public static void Initialize()
    {
        // They made Lambda private and inacessible. Needs a harmony patch to work.
        List<ThingDef> allowedMedicineDefs = DefDatabase<ThingDef>.AllDefs.Where(x => 
            x.thingCategories != null &&
            x.thingCategories.Contains(ThingCategoryDefOf.Medicine) &&
            x.IsMedicine)
            .ToList();
        allowedMedicineDefs.Sort((a, b) =>
        {
            if (StatExtension.GetStatValueAbstract(a, StatDefOf.MedicalPotency) < StatExtension.GetStatValueAbstract(b, StatDefOf.MedicalPotency))
                return -1;
            if (StatExtension.GetStatValueAbstract(a, StatDefOf.MedicalPotency) > StatExtension.GetStatValueAbstract(b, StatDefOf.MedicalPotency))
                return 1;
            return 0;
        });

        LoadoutGenericDef generic;
        for (int i = 0; i < allowedMedicineDefs.Count; i++)
        {
            ThingDef medicineDef = allowedMedicineDefs[i];
            generic = new LoadoutGenericDef();
            generic.defName = "CEEL_GenericMedicine_" + medicineDef.defName;
            generic.defaultCount = 5;
            generic.defaultCountType = LoadoutCountType.pickupDrop;
            generic.description = "Generic Loadout for Medicine.  Intended for pawns which will handle triage activities.";
            generic.label = "CE_Extended.Medicines".Translate(medicineDef.LabelCap);
            generic.thingRequestGroup = ThingRequestGroup.Medicine;
            var allowedMeds = allowedMedicineDefs.Take(i + 1).ToList();
            Traverse traverse = new Traverse(generic);
            Predicate<ThingDef> predicate = td => td != null && td.IsMedicine && allowedMeds.Contains(td);
            traverse.Field("_lambda").SetValue(predicate);
            //generic._lambda = td => td != null && td.IsMedicine && allowedMeds.Contains(td);
            generic.isBasic = false;
            DefDatabase<LoadoutGenericDef>.Add(generic);
        }
    }
}
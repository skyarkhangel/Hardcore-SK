namespace Numbers
{
    using System.Collections.Generic;
    using Verse;

    public class Numbers_Settings : ModSettings
    {

        public static float maxHeight = 1f;

        public static bool showMoreInfoThanVanilla = true;
        public static bool coolerThanTheWildlifeTab = true; // don't deny it.
        public static bool coolerThanTheAnimalTab = false; // you flatter me.

        public static bool pawnTableHighlightSelected = true;
        public static bool pawnTableClickSelect = true;

        public static bool useTinyFont = false;

        public List<string> storedPawnTableDefs = new List<string>();
        private readonly List<string> workingList = new List<string>();

        public void StoreNewPawnTableDef(string pawnTableDeftoSave)
        {
            workingList.Clear();
            foreach (string ptd in storedPawnTableDefs)
            {
                workingList.Add(ptd.Split(',')[0] + ptd.Split(',')[1]);
            }
            //overwrite old one
            string label = pawnTableDeftoSave.Split(',')[0] + pawnTableDeftoSave.Split(',')[1];

            if (workingList.Contains(label))
                storedPawnTableDefs.RemoveAll(x => x.Split(',')[0] + x.Split(',')[1] == label);
            
            if (label == "Default")
                storedPawnTableDefs.Insert(0, pawnTableDeftoSave);
            else
                storedPawnTableDefs.Add(pawnTableDeftoSave);

            Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref showMoreInfoThanVanilla, "showMoreInfoThanVanilla", true);
            Scribe_Values.Look(ref coolerThanTheWildlifeTab, "coolerThanTheWildlifeTab", true);
            Scribe_Values.Look(ref coolerThanTheAnimalTab, "coolerThanTheAnimalTab");
            Scribe_Values.Look(ref pawnTableHighlightSelected, "pawnTableHighlightSelected", true);
            Scribe_Values.Look(ref pawnTableClickSelect, "pawnTableClickSelect", true);
            Scribe_Values.Look(ref maxHeight, "maxHeight", 1f);
            Scribe_Values.Look(ref useTinyFont, "useTinyFont");
            Scribe_Collections.Look(ref storedPawnTableDefs, "numbersPawnTableDefs");
        }
    }
}

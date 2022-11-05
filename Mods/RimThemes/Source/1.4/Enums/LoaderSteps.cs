namespace aRandomKiwi.RimThemes
{
    public enum LoaderSteps
    {
        //----------------------Game Boot
        loadingXML=0,           //10
        CombineXML,             //5
        Patching,               //5
        ParsingXML,             //50  (with progressive advance by defs)
        ResolvingReferences,    //10
        loadingTheme,
        FinishUp,               //10
        //----------------------Save loading 
        LoadWorldMap,           //10
        FillWorldMap,           //10
        FinalizeWorld,          //10
        MapsInitComps,          //5
        MapsLoadComps,          //25
        MapsLoadData,           //30
        SetCamera,              //5
        ResolveSaveFileCrossReferences,  //5
        SpawnThings,                     //5
        FinalizeLoad,                    //5
        Idle,                            // No output
        //----------------------World/Planet loading
        GeneratingPlanet,                //10
        FinalizeGeneratingPlanet,        //90
        CreateWorldGeneratingWorld,      //10
        CreateWorldFinalizeWorld, //90
        //----------------------Save saving
        InitSaveSaving,
        FinalizeSaveSaving,
        //----------------------New map
        InitGeneratingMap,
        //----------------------Gen map for new encounter
        InitGeneratingMapForNewEncounter

    }
}
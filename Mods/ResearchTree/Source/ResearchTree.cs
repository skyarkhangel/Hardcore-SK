// ResearchTree.cs
// Copyright Karel Kroeze, 2020-2020

using System.Reflection;
using HarmonyLib;
using Verse;
//using Multiplayer.API;

namespace FluffyResearchTree
{
    public class ResearchTree : Mod
    {
        public ResearchTree( ModContentPack content ) : base( content )
        {
            var harmony = new Harmony( "Fluffy.ResearchTree" );
            harmony.PatchAll( Assembly.GetExecutingAssembly() );

//            if ( MP.enabled )
//                MP.RegisterAll();
        }
    }
}
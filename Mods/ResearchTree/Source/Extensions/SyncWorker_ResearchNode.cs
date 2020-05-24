// SyncWorker_ResearchNode.cs
// Copyright Karel Kroeze, 2020-2020

//using System.Collections.Generic;
//using System.Linq;
//using Multiplayer.API;
//using Verse;
//
//namespace FluffyResearchTree
//{
//    public class SyncWorker_ResearchNode
//    {
//        [SyncWorker]
//        static void SyncResearchNode( SyncWorker worker, ref ResearchNode node )
//        {
//            Log.Debug( $"Syncing" );
//            if ( worker.isWriting )
//            {
//                Log.Debug( $"writing" );
//                worker.Write( node.Research.defName );
//            }
//            else
//            {
//                Log.Debug( $"reading" );
//                string researchDef = worker.Read<string>();
//                node = ( DefDatabase<ResearchProjectDef>.GetNamed( researchDef ) ).ResearchNode();
//            }
//        }
//
//        [SyncWorker]
//        static void SyncResearchNodes( SyncWorker worker, ref IEnumerable<ResearchNode> nodes )
//        {
//
//            Log.Debug( $"Syncing" );
//            if ( worker.isWriting )
//            {
//                Log.Debug( $"writing" );
//                worker.Write( nodes.Select( node => node.Research.defName ) );
//            }
//            else
//            {
//                Log.Debug( $"reading" );
//                var defNames = worker.Read<IEnumerable<string>>();
//                nodes = defNames.Select( name => ( DefDatabase<ResearchProjectDef>.GetNamed( name ) ).ResearchNode() );
//            }
//        }
//    }
//
//
//}


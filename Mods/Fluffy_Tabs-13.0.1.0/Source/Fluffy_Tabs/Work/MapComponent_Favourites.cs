using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Fluffy_Tabs
{
    public class MapComponent_Favourites : MapComponent
    {
        #region Fields

        public List<WorkFavourite> favourites = new List<WorkFavourite>();
        private static MapComponent_Favourites _instance;

        #endregion Fields

        #region Properties

        public static MapComponent_Favourites Instance
        {
            get
            {
                // if cache is null, get instance from game
                if ( _instance == null )
                    _instance = Find.Map.GetComponent<MapComponent_Favourites>();

                // if game has no instance, create a new one and inject it
                if ( _instance == null )
                {
                    _instance = new MapComponent_Favourites();
                    Find.Map.components.Add( _instance );
                }

                // return cache
                return _instance;
            }
        }

        #endregion Properties

        #region Methods

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.LookList( ref favourites, "favourites", LookMode.Deep );
        }

        #endregion Methods

        // debug screen overlay
        //public override void MapComponentOnGUI()
        //{
        //    string label = favourites.Count.ToString();

        //    if ( favourites.Any() )
        //        label += favourites.Select( fav => fav.label ).StringJoin();

        //    float width = label.NoWrapWidth();

        //    Rect labelRect = new Rect( 0f, 0f, width, 30f );
        //    labelRect.center = new Vector2( Screen.width * 1/2f, Screen.height * 1/2f );

        //    Widgets.Label( labelRect, label );
        //}
    }
}
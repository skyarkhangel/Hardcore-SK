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
    public class WorkFavourite : IExposable, ILoadReferenceable
    {
        #region Fields

        public static int count = 0;
        
        public int ID;
        public string label;
        public PawnPrioritiesTracker workgiverPriorities = new PawnPrioritiesTracker();
        private Texture2D _icon;
        private string _iconpath;

        #endregion Fields

        #region Constructors

        // scribe
        public WorkFavourite()
        {
            ID = count++;
        }

        public WorkFavourite( Pawn pawn, string label, string iconpath ) : this()
        {
            this.label = label;
            this._icon = ContentFinder<Texture2D>.Get( iconpath );
            this._iconpath = iconpath;

            workgiverPriorities = pawn.Priorities();
        }

        #endregion Constructors

        #region Properties

        public Texture2D Icon
        {
            get
            {
                if ( _icon == null )
                    _icon = ContentFinder<Texture2D>.Get( _iconpath );
                return _icon;
            }
        }

        #endregion Properties

        #region Methods

        public void ExposeData()
        {
            Scribe_Values.LookValue( ref label, "label" );
            Scribe_Values.LookValue( ref ID, "ID" );
            Scribe_Values.LookValue( ref _iconpath, "iconpath" );
            Scribe_Deep.LookDeep( ref workgiverPriorities, "workgiverPriorities" );
        }

        public string GetUniqueLoadID()
        {
            return "Favourite_" + label + "_" + ID;
        }

        #endregion Methods
    }
}
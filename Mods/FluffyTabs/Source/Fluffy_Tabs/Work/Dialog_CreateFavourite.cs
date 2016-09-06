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
    public class Dialog_CreateFavourite : Window
    {
        #region Fields

        private Vector2 _size = new Vector2( 350f, 500f );
        private bool dwarfTherapistMode;
        private string label;
        private Pawn pawn;
        private TextureChooser textureChooser;

        #endregion Fields

        #region Constructors

        public Dialog_CreateFavourite( Pawn pawn, bool dwarfTherapistMode )
        {
            this.pawn = pawn;
            this.dwarfTherapistMode = dwarfTherapistMode;
            this.label = "FluffyTabs.DefaultFavouriteLabel".Translate();
            this.textureChooser = new TextureChooser( Resources.Icons );
        }

        #endregion Constructors

        #region Properties

        public override Vector2 InitialSize => _size;

        #endregion Properties

        #region Methods

        public override void DoWindowContents( Rect canvas )
        {
            Vector2 curPos = Vector2.zero;

            // title and a little text
            Widgets.Paragraph( ref curPos, canvas.width, "FluffyTabs.CreateFavouritesDialogTitle".Translate(), Color.white, GameFont.Medium );
            Widgets.Paragraph( ref curPos, canvas.width, "FluffyTabs.CreateFavouritesDialog".Translate( pawn.NameStringShort ), Color.white );

            // favourites label
            string tmp = GUI.TextField( new Rect( curPos.x, curPos.y, ( canvas.width - Settings.Margin ) / 3f * 2f, 30f ), label );
            if ( IsValidLabel( tmp ) )
                label = tmp;

            // icon tumbler
            textureChooser.DrawAt( new Rect( curPos.x + ( canvas.width + Settings.Margin ) / 3f * 2f, curPos.y, ( canvas.width - Settings.Margin ) / 3f, 30f ) );
            curPos.y += 36f;

            // ok/cancel buttons.
            if ( Verse.Widgets.ButtonText( new Rect( curPos.x, curPos.y, ( canvas.width - Settings.Margin ) / 2f, 30f ), "Cancel".Translate() ) )
                Close();
            if ( Verse.Widgets.ButtonText( new Rect( curPos.x + ( canvas.width + Settings.Margin ) / 2f, curPos.y, ( canvas.width - Settings.Margin ) / 2f, 30f ), "OK".Translate() ) )
                ApplyAndClose();

            // set height
            _size.y = curPos.y + 30f;
        }

        public override void WindowUpdate()
        {
            windowRect.height = _size.y + Margin * 2;
        }

        private void ApplyAndClose()
        {
            // create favourite
            WorkFavourite favourite = new WorkFavourite( pawn, label, "UI/Icons/Various/" + textureChooser.Choice.name );

            // add to list
            MapComponent_Favourites.Instance.Add( favourite );

            // assign to pawn
            if ( dwarfTherapistMode )
                pawn.Priorities().AssignFavourite( favourite );
            else
                pawn.Priorities().AssignFavourite( favourite );

            // done!
            Close();
        }

        private bool IsValidLabel( string candidate )
        {
            // TODO: if we're going to export these we'll want to do some label checking. For now, keep them in the mapcomp, and labels are irrelevant.
            return true;
        }

        #endregion Methods
    }
}
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace EdB.ModOrder
{
	// A dialog that displays all of the information about the mod (author, description,
	// URL, etc.) that is normally displayed in the main Mods config screen in the vanilla
	// game.  We don't have room for it in the modded version of the screen, so we put it
	// in a dialog.
	public class Dialog_ModInfo : Window
	{
		static protected Vector2 WinSize = new Vector2(640, 640);
		static protected float Margin = 24;
		static protected Vector2 ContentSize = new Vector2(WinSize.x - Margin * 2, WinSize.y - Margin * 2);

		protected InstalledMod mod;

		public override Vector2 InitialWindowSize {
			get {
				return WinSize;
			}
		}

		// The constructor takes the mod for which it will display information as an argument.
		public Dialog_ModInfo(InstalledMod mod) : base()
		{
			// Standard configuration from the base class.
			this.doCloseButton = true;

			// Store the mod.
			this.mod = mod;
		}

		// Draw the dialog contents.
		public override void DoWindowContents(Rect inRect)
		{
			float width = ContentSize.x;
			float height = ContentSize.y;
			Text.Font = GameFont.Medium;
			Rect labelRect = new Rect(0, 0, width, 40);
			Widgets.Label(labelRect, this.mod.Name);
			Rect imageRect = new Rect(0, labelRect.yMax, 0, 20);
			if (this.mod.previewImage != null) {
				imageRect.width = (float)this.mod.previewImage.width;
				imageRect.height = (float)this.mod.previewImage.height;
				imageRect.x = width / 2 - imageRect.width / 2;
				GUI.DrawTexture(imageRect, this.mod.previewImage, ScaleMode.ScaleToFit);
			}
			Text.Font = GameFont.Small;
			float imageHeight = imageRect.yMax + 15;
			Rect descriptionRect = new Rect(0, imageHeight, width, height - imageHeight);
			string fullDescriptionText = string.Concat(new string[] {
				"Author".Translate(),
				": ",
				this.mod.Author,
				"\n\n",
				this.mod.Description
			});
			Widgets.Label(descriptionRect, fullDescriptionText);
			if (this.mod.Url != string.Empty) {
				float x = Text.CalcSize(this.mod.Url).x;
				Rect urlRect = new Rect(descriptionRect);
				urlRect.xMin += urlRect.xMax - x;
				urlRect.height = 25;
				if (Widgets.TextButton(urlRect, this.mod.Url, false, true)) {
					Application.OpenURL(this.mod.Url);
				}
			}

		}
	}
}


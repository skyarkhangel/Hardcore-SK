using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.ModOrder
{
	/* This is a modified version of the RimWorld.Page_ModsConfig class that is swapped
	 * in when that screen appears.
	 */
	public class Page_ModsConfig : Window
	{
		// Textures are defined here and loaded in the ResetTextures() static initializer below.
		protected static Texture2D ButtonTexReorderUp;
		protected static Texture2D ButtonTexReorderDown;
		protected static Texture2D ButtonTexReorderTop;
		protected static Texture2D ButtonTexReorderBottom;

		// Would be better to re-use the textures defined in Widget, but they are private, so we need
		// to define our own copies here.
		protected static Texture2D ButtonBGAtlas;
		protected static Texture2D ButtonBGAtlasMouseover;
		protected static Texture2D ButtonBGAtlasClick;

		// Define the window size.
		protected static readonly Vector2 WinSize = new Vector2(900, 750);

		// Keep track of which mod is selected and from which list: the list of
		// available mods or the list of active mods.
		protected int selectedIndex = -1;
		protected bool selectedFromAvailable = false;
		protected bool selectedFromActive = false;

		// The list of available mods and the variables that support its scrolling view.
		protected List<InstalledMod> availableMods = new List<InstalledMod>();
		protected Vector2 availableModListScrollPosition = Vector2.zero;
		protected float availableModListScrollHeight = 0;

		// The list of active mods and the variables that support its scrolling view.
		protected List<InstalledMod> activeMods = new List<InstalledMod>();
		protected Vector2 activeModListScrollPosition = Vector2.zero;
		protected float activeModListScrollHeight = 0;

		// The list of mods that were active when the screen is first displayed.
		// This is used when the screen is closed to determine if anything has changed.
		// If nothing has changed, it doesn't bother reloading the mods.
		protected List<InstalledMod> previousActiveMods;

		// Load the textures.  Use the LongEventHandler to make sure that the textures get loaded in the
		// main thread.  If you don't do this, you will crash on Linux.
		public static void ResetTextures() {
			LongEventHandler.ExecuteWhenFinished(() => {
				ButtonTexReorderUp = ContentFinder<Texture2D>.Get("EdB/ModOrder/ArrowUp", true);
				ButtonTexReorderDown = ContentFinder<Texture2D>.Get("EdB/ModOrder/ArrowDown", true);
				ButtonTexReorderTop = ContentFinder<Texture2D>.Get("EdB/ModOrder/ArrowTop", true);
				ButtonTexReorderBottom = ContentFinder<Texture2D>.Get("EdB/ModOrder/ArrowBottom", true);
				ButtonBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG", true);
				ButtonBGAtlasMouseover = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGMouseover", true);
				ButtonBGAtlasClick = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGClick", true);
			});
		}

		public override Vector2 InitialWindowSize {
			get {
				return Page_ModsConfig.WinSize;
			}
		}

		public Page_ModsConfig()
		{
			ResetTextures();

			// Set available options for the Layer base class.
			this.forcePause = true;
			this.doCloseButton = false;

			// Initialize the available mods list.  The installed mods list is alphabetical, so we
			// just iterate it to add non-active mods to the list of available mods.
			foreach (InstalledMod current in InstalledModLister.AllInstalledMods) {
				if (!current.Active) {
					this.availableMods.Add(current);
				}
			}

			// To set up the active mod list, we iterate through the mod config's list of mod
			// names.  To get the matching InstalledMod object, we first set up a dictionary
			// that we can use to look up the active mods by name.
			Dictionary<string, InstalledMod> dictionary = new Dictionary<string, InstalledMod>();
			foreach (InstalledMod mod in InstalledModLister.AllInstalledMods) {
				dictionary[mod.Identifier] = mod;
			}
			foreach (InstalledMod mod in ModsConfig.ActiveMods) {
				this.activeMods.Add(mod);
			}

			// Create a copy of the mods that were active when we started.  When we
			// close the window, we'll compare the final list against this one to
			// see if we need to bother reloading mods.
			previousActiveMods = new List<InstalledMod>(activeMods);
		}

		// Define UI size and color values
		protected static float ScrollBarWidth = 16;

		protected static float AvailableModRowMarginWidth = 16;
		protected static Vector2 AvailableModsRectSize = new Vector2(290, 580);
		protected static Rect AvailableModsRect = new Rect(20, 60, AvailableModsRectSize.x, AvailableModsRectSize.y);
		protected static Rect AvailableModsHeaderRect = new Rect(AvailableModsRect.x + 4, AvailableModsRect.y - 28, AvailableModsRectSize.x, 30);
		protected static Rect AvailableModRowRect = new Rect(0, 0, AvailableModsRectSize.x - 2, 34);
		protected static Rect AvailableModRowLabelRect = new Rect(AvailableModRowMarginWidth, 2, AvailableModsRectSize.x - (ScrollBarWidth + AvailableModRowMarginWidth) - 2, 34);

		protected static float ActiveModRowMarginWidth = 16;
		protected static Vector2 ActiveModsRectSize = new Vector2(290, 580);
		protected static Rect ActiveModsRect = new Rect(534, 60, ActiveModsRectSize.x, ActiveModsRectSize.y);
		protected static Rect ActiveModsHeaderRect = new Rect(ActiveModsRect.x + 4, ActiveModsRect.y - 28, ActiveModsRectSize.x, 30);
		protected static Rect ActiveModRowRect = new Rect(0, 0, ActiveModsRectSize.x - 2, 34);
		protected static Rect ActiveModRowLabelRect = new Rect(ActiveModRowMarginWidth, 2, ActiveModsRectSize.x - (ScrollBarWidth + ActiveModRowMarginWidth) - 2, 34);
		protected static Vector2 OrderButtonPadding = new Vector2(8, 12);
		protected static Vector2 OrderButtonSize = new Vector2(32, 32);
		protected static Rect ActiveModTopButtonRect = new Rect(ActiveModsRect.x + ActiveModsRect.width + OrderButtonPadding.x, ActiveModsRect.y, 
			OrderButtonSize.x, OrderButtonSize.y);
		protected static float ActiveModsRectVerticalCenter = ActiveModsRect.y + (ActiveModsRectSize.y / 2);
		protected static Rect ActiveModUpButtonRect = new Rect(ActiveModsRect.x + ActiveModsRect.width + OrderButtonPadding.x,
			ActiveModsRectVerticalCenter - OrderButtonSize.y - (OrderButtonPadding.y / 2), OrderButtonSize.x, OrderButtonSize.y);
		protected static Rect ActiveModDownButtonRect = new Rect(ActiveModsRect.x + ActiveModsRect.width + OrderButtonPadding.x,
			ActiveModsRectVerticalCenter + (OrderButtonPadding.y / 2), OrderButtonSize.x, OrderButtonSize.y);
		protected static Rect ActiveModBottomButtonRect = new Rect(ActiveModsRect.x + ActiveModsRect.width + OrderButtonPadding.x, 
			ActiveModsRect.y + ActiveModsRect.height - OrderButtonSize.y, OrderButtonSize.x, OrderButtonSize.y);

		protected static Rect AddRemoveButtonRect = new Rect(352, 288, 141, 40);
		protected static Rect InfoButtonRect = new Rect(352, 348, 141, 40);

		protected static Color ModRectHeaderTextColor = new Color(0.8f, 0.8f, 0.8f);
		protected static Color ModTextColor = new Color(0.7216f, 0.7647f, 0.902f);
		protected static Color SelectedModRowColor = new Color(0.2588f, 0.2588f, 0.2588f);
		protected static Color SelectedModTextColor = new Color(0.902f, 0.8314f, 0);
		protected static Color AlternateModRowColor = new Color(0.15f, 0.15f, 0.15f);
		protected static Color ArrowButtonColor = new Color(0.9137f, 0.9137f, 0.9137f);
		protected static Color ArrowButtonHighlightColor = Color.white;
		protected static Color InactiveButtonColor = new Color(1, 1, 1, 0.5f);

		// Draw the contents of the window.  There's some duplication of code here when drawing the
		// available mods box and the active mods box.  I probably could have consolidated some of this,
		// but I ended up keeping them separate.
		public override void DoWindowContents(Rect inRect)
		{
			// Figure out if this layer is the top layer.  If it isn't we'll disable some mouse-over effects later.
			bool isTopLayer = Find.WindowStack.Windows[Find.WindowStack.Windows.Count - 1] == this;

			// Draw the page header.
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0, 0, 300, 300), "Mods".Translate());
			Text.Font = GameFont.Small;

			// Draw the available mods header.
			GUI.color = ModRectHeaderTextColor;
			Text.Anchor = TextAnchor.LowerLeft;
			Widgets.Label(AvailableModsHeaderRect, "EdB.ModOrder.AvailableMods".Translate());
			Text.Anchor = TextAnchor.UpperLeft;

			// Draw the available mods list box.
			Widgets.DrawMenuSection(AvailableModsRect, true);

			// Surround the BeginGroup() and BeginScrollView() calls with try/finally blocks to
			// guarantee that their corresponding End...() methods get called even if there is
			// an exception thrown.
			try {
				GUI.BeginGroup(AvailableModsRect.ContractedBy(1));
				Rect scrollRect = new Rect(0, 0, AvailableModsRectSize.x - 2, AvailableModsRectSize.y - 2);
				Rect viewRect = new Rect(scrollRect.x, scrollRect.y, scrollRect.width - 16, availableModListScrollHeight);

				try {
					Widgets.BeginScrollView(scrollRect, ref availableModListScrollPosition, viewRect);

					Rect rowRect = AvailableModRowRect;
					Rect rowLabelRect = AvailableModRowLabelRect;
					Text.Anchor = TextAnchor.MiddleLeft;
					int index = 0;
					bool alternateRow = false;

					// Draw each mod row.
					foreach (InstalledMod mod in availableMods) {

						// The row background gets a different color every alternate row, or if that
						// row is selected.
						if (selectedFromAvailable && index == this.selectedIndex) {
							GUI.color = SelectedModRowColor;
							GUI.DrawTexture(rowRect, BaseContent.WhiteTex);
						}
						else if (alternateRow) {
							GUI.color = AlternateModRowColor;
							GUI.DrawTexture(rowRect, BaseContent.WhiteTex);
						}
						alternateRow = !alternateRow;
						GUI.color = Color.white;

						// Change the mod name color on mouse-over.  Only change it if we're the top layer.
						// If we're not the top layer, then the mod info dialog is showing, so we disable
						// mouse-over highlights.
						if (rowLabelRect.Contains(Event.current.mousePosition) && isTopLayer) {
							GUI.color = SelectedModTextColor;
						}
						else {
							GUI.color = ModTextColor;
						}

						// Draw the mod name label.
						Widgets.Label(rowLabelRect, mod.Name);
						GUI.color = Color.white;

						// If the user clicks on the mod name, set the selection.
						if (Widgets.InvisibleButton(rowRect)) {
							if (!selectedFromAvailable || selectedIndex != index) {
								SoundDefOf.TickTiny.PlayOneShotOnCamera();
								selectedFromAvailable = true;
								selectedFromActive = false;
								selectedIndex = index;
							}
						}

						// Move the rectangles down for the next row.
						rowRect.y += AvailableModRowRect.height;
						rowLabelRect.y += AvailableModRowRect.height;
						index++;
					}

					// Finish the scroll view by setting its height based on the
					// position of the last row.
					if (Event.current.type == EventType.Layout) {
						availableModListScrollHeight = rowRect.y;
					}
				}
				finally {
					Widgets.EndScrollView();
				}
			}
			finally {
				GUI.EndGroup();
			}
			Text.Anchor = TextAnchor.UpperLeft;

			// Draw the active mod list box.
			GUI.color = Color.white;
			Widgets.DrawMenuSection(ActiveModsRect, true);

			// Draw the active mod list header.
			GUI.color = ModRectHeaderTextColor;
			Text.Anchor = TextAnchor.LowerLeft;
			Widgets.Label(ActiveModsHeaderRect, "EdB.ModOrder.ActiveMods".Translate());
			Text.Anchor = TextAnchor.UpperLeft;

			try {
				GUI.BeginGroup(ActiveModsRect.ContractedBy(1));
				Rect scrollRect = new Rect(0, 0, ActiveModsRectSize.x - 2, ActiveModsRectSize.y - 2);
				Rect viewRect = new Rect(scrollRect.x, scrollRect.y, scrollRect.width - 16, activeModListScrollHeight);

				try {
					Widgets.BeginScrollView(scrollRect, ref activeModListScrollPosition, viewRect);

					// Draw each mod row.
					Rect rowRect = ActiveModRowRect;
					Rect rowLabelRect = ActiveModRowLabelRect;
					GUI.color = ModTextColor;
					Text.Anchor = TextAnchor.MiddleLeft;
					int lastIndex = activeMods.Count - 1;
					int index = 0;
					bool alternateRow = false;
					foreach (InstalledMod mod in activeMods) {

						// Set the row color based on whether or not the mod has been selected.
						// We alternate colors for each row, so if it's not selected, we check to
						// see if we're on an alternate row.
						if (selectedFromActive && index == this.selectedIndex) {
							GUI.color = SelectedModRowColor;
							GUI.DrawTexture(rowRect, BaseContent.WhiteTex);
						}
						else if (alternateRow) {
							GUI.color = AlternateModRowColor;
							GUI.DrawTexture(rowRect, BaseContent.WhiteTex);
						}
						GUI.color = Color.white;
						alternateRow = !alternateRow;

						// Change the mod name color on mouseover.  Only change it if we're the top layer.
						// If we're not the top layer, then the mod info dialog is showing, so we disable
						// mouseover highlights.
						if (rowLabelRect.Contains(Event.current.mousePosition) && isTopLayer) {
							GUI.color = SelectedModTextColor;
						}
						else {
							GUI.color = ModTextColor;
						}

						// Draw the mod name.
						Widgets.Label(rowLabelRect, mod.Name);
						GUI.color = Color.white;

						// If the user clicks on the mod name, set the selection.
						if (Widgets.InvisibleButton(rowLabelRect)) {
							if (!selectedFromActive || selectedIndex != index) {
								SoundDefOf.TickTiny.PlayOneShotOnCamera();
								selectedFromAvailable = false;
								selectedFromActive = true;
								selectedIndex = index;
							}
						}

						// Move the rectangles down for the next row.
						rowRect.y += ActiveModRowRect.height;
						rowLabelRect.y += ActiveModRowRect.height;
						index++;
					}

					// Finish the scroll view by setting its height based on the
					// position of the last row.
					if (Event.current.type == EventType.Layout) {
						activeModListScrollHeight = rowRect.y;
					}

				}
				finally {
					Widgets.EndScrollView();
				}
			}
			finally {
				GUI.EndGroup();
			}
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;

			// Draw the mod order buttons.
			if (IconButton(ActiveModTopButtonRect, ButtonTexReorderTop, ArrowButtonColor, ArrowButtonHighlightColor, selectedFromActive && selectedIndex > 0 && activeMods.Count > 1)) {
				MoveModToTop();
			}
			if (IconButton(ActiveModUpButtonRect, ButtonTexReorderUp, ArrowButtonColor, ArrowButtonHighlightColor, selectedFromActive && selectedIndex > 0 && activeMods.Count > 1)) {
				ReorderSelectedMod(-1);
			}
			if (IconButton(ActiveModDownButtonRect, ButtonTexReorderDown, ArrowButtonColor, ArrowButtonHighlightColor, selectedFromActive && selectedIndex < activeMods.Count - 1 && activeMods.Count > 1)) {
				ReorderSelectedMod(1);
			}
			if (IconButton(ActiveModBottomButtonRect, ButtonTexReorderBottom, ArrowButtonColor, ArrowButtonHighlightColor, selectedFromActive && selectedIndex < activeMods.Count - 1 && activeMods.Count > 1)) {
				MoveModToBottom();
			}
			GUI.color = Color.white;

			// Handle keyboard shortcuts.
			if (Event.current.type == EventType.KeyDown) {
				if (Event.current.keyCode == KeyCode.UpArrow) {
					ReorderSelectedMod(-1);
				}
				else if (Event.current.keyCode == KeyCode.DownArrow) {
					ReorderSelectedMod(1);
				}
				else if (Event.current.keyCode == KeyCode.Delete) {
					ConfirmModDeactivation();
				}
			}

			// If there's a selection in the available column, draw the add button and the info button.
			if (selectedFromAvailable) {
				if (Widgets.TextButton(AddRemoveButtonRect, "EdB.ModOrder.Add".Translate(), true, true)) {
					ActivateSelectedMod();
				}
				if (Widgets.TextButton(InfoButtonRect, "EdB.ModOrder.Info".Translate(), true, true)) {
					ShowInfoForSelectedMod();
				}
			}
			// If there's a selection in the active column, draw the remove button and the info button.
			else if (selectedFromActive) {
				if (Widgets.TextButton(AddRemoveButtonRect, "EdB.ModOrder.Remove".Translate(), true, true)) {
					int previousIndex = selectedIndex;
					ConfirmModDeactivation();
				}
				if (Widgets.TextButton(InfoButtonRect, "EdB.ModOrder.Info".Translate(), true, true)) {
					ShowInfoForSelectedMod();
				}
			}
			// If nothing is selected, draw semi-transparent, disabled buttons.
			else {
				GUI.color = InactiveButtonColor;
				Widgets.TextButton(AddRemoveButtonRect, "EdB.ModOrder.Add".Translate(), true, true);
				GUI.color = InactiveButtonColor;
				Widgets.TextButton(InfoButtonRect, "EdB.ModOrder.Info".Translate(), true, true);
			}
			GUI.color = Color.white;

			// Draw the bottom buttom row.
			float buttonHeight = this.CloseButSize.y;
			float buttonY = inRect.height - 58;
			Rect getModsButttonRect = new Rect(18, buttonY, 206, buttonHeight);

			// TODO: Include Steam Workshop changes when the Steam release happens.
			if (Widgets.TextButton(getModsButttonRect, "GetModsFromForum".Translate(), true, true)) {
				Application.OpenURL("http://rimworldgame.com/getmods");
			}

			Rect modFolderButtonRect = new Rect(240, buttonY, 206, buttonHeight);
			if (Widgets.TextButton(modFolderButtonRect, "OpenModsDataFolder".Translate(), true, true)) {
				Application.OpenURL(GenFilePaths.CoreModsFolderPath);
			}

			Rect closeButtonRect = new Rect(inRect.width - this.CloseButSize.x - 40, buttonY, this.CloseButSize.x, buttonHeight);
			if (Widgets.TextButton(closeButtonRect, "CloseButton".Translate(), true, true)) {
				ConfirmModOrder();
			}
		}

		// Try to select the mod from the active list with the specified index.  Used for
		// when a user removes a mod from the list, and we want to select another.  It
		// performs a few checks to determine which, if any, mod from the list should be
		// selected.
		protected void SelectActiveMod(int index)
		{
			if (activeMods.Count == 0 || index < 0) {
				selectedFromActive = false;
				selectedFromAvailable = false;
				return;
			}
			if (index < activeMods.Count) {
				selectedIndex = index;
				selectedFromActive = true;
				selectedFromAvailable = false;
			}
			else {
				selectedIndex = activeMods.Count - 1;
				selectedFromActive = true;
				selectedFromAvailable = false;
			}
		}

		// Reorder the active mods list.
		protected void ReorderSelectedMod(int order)
		{
			if (!selectedFromActive) {
				return;
			}
			int currentIndex = selectedIndex;
			int swapIndex = selectedIndex + order;
			if (swapIndex < 0 || swapIndex > activeMods.Count - 1) {
				return;
			}
			InstalledMod swappedMod = activeMods[swapIndex];
			activeMods[swapIndex] = activeMods[currentIndex];
			activeMods[currentIndex] = swappedMod;
			selectedIndex = swapIndex;
			selectedFromActive = true;
			selectedFromAvailable = false;
			if (order < 0) {
				SoundDefOf.TickHigh.PlayOneShotOnCamera();
			}
			else {
				SoundDefOf.TickLow.PlayOneShotOnCamera();
			}
		}

		// Move the selected mod to top of the active list.
		protected void MoveModToTop()
		{
			if (!selectedFromActive) {
				return;
			}
			int currentIndex = selectedIndex;
			InstalledMod movedMod = activeMods[currentIndex];
			for (int i = currentIndex; i > 0; i--) {
				activeMods[i] = activeMods[i - 1];
			}
			activeMods[0] = movedMod;
			selectedIndex = 0;
			selectedFromActive = true;
			selectedFromAvailable = false;
			SoundDefOf.TickHigh.PlayOneShotOnCamera();
		}

		// Move the selected mod to bottom of the active list.
		protected void MoveModToBottom()
		{
			if (!selectedFromActive) {
				return;
			}
			int currentIndex = selectedIndex;
			InstalledMod movedMod = activeMods[currentIndex];
			for (int i = currentIndex; i < activeMods.Count - 1; i++) {
				activeMods[i] = activeMods[i + 1];
			}
			activeMods[activeMods.Count - 1] = movedMod;
			selectedIndex = activeMods.Count - 1;
			selectedFromActive = true;
			selectedFromAvailable = false;
			SoundDefOf.TickHigh.PlayOneShotOnCamera();
		}

		// Move the selected available mod into the active list.
		protected void ActivateSelectedMod()
		{
			if (!selectedFromAvailable) {
				return;
			}
			activeMods.Add(availableMods[selectedIndex]);
			availableMods.RemoveAt(selectedIndex);
			selectedFromAvailable = false;
			selectedFromActive = true;
			selectedIndex = activeMods.Count - 1;
			SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
		}

		// Check to see if the user is trying to remove the Core mod and give
		// the corresponding warning if they are.  Otherwise, go ahead and
		// deactivate the selected mod.
		protected void ConfirmModDeactivation()
		{
			if (!selectedFromActive) {
				return;
			}
			if (activeMods[selectedIndex].Name == LoadedMod.CoreModFolderName) {
				Find.WindowStack.Add(new Dialog_Confirm("ConfirmDisableCoreMod".Translate(), delegate {
					DeactivateSelectedMod();
				}, false));
			}
			else {
				DeactivateSelectedMod();
			}
		}

		// Move the selected mod from the active list back into the available list.
		protected void DeactivateSelectedMod()
		{
			if (!selectedFromActive) {
				return;
			}
			InstalledMod mod = activeMods[selectedIndex];
			availableMods.Add(mod);

			// We want to keep the available mods alphabetical, so after we add the mod
			// back into the list, we sort the list.  Having to re-sort every time the
			// list changes isn't great, but it's fast enough that we don't have to look
			// for some other strategy.
			availableMods.Sort((InstalledMod a, InstalledMod b) => {
				return a.Name.CompareTo(b.Name);
			});

			activeMods.RemoveAt(selectedIndex);
			SelectActiveMod(selectedIndex);
			SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
		}

		// Perform a couple of validations before closing the window and reloading the
		// selected mods.
		protected void ConfirmModOrder()
		{
			// Make sure that at least one mod is active.  If not, show a warning.  If the user
			// proceeds anyway, activate the core mod before reloading the mods.
			if (activeMods.Count == 0) {
				Find.WindowStack.Add(new Dialog_Confirm("EdB.ModOrder.NoModWarning".Translate(), delegate {
					ActivateCoreMod();
					this.Close(true);
					Event.current.Use();
				}, false));
				return;
			}

			// Check to see if the core mod is not at the top of the active mod list.  If it's
			// active but not at the top of the list, show a warning.
			bool coreIsActive = false;
			foreach (InstalledMod mod in activeMods) {
				if (mod.Name == LoadedMod.CoreModFolderName) {
					coreIsActive = true;
					break;
				}
			}
			if (coreIsActive && activeMods[0].Name != LoadedMod.CoreModFolderName) {
				Find.WindowStack.Add(new Dialog_Confirm("EdB.ModOrder.CoreModOrderWarning".Translate(), delegate {
					this.Close(true);
					Event.current.Use();
				}, false));
			}
			else {
				this.Close(true);
				Event.current.Use();
			}
		}

		// Activate the core mod when no other mods are active.
		protected void ActivateCoreMod()
		{
			// Iterate the available mods to find and select the Core mod.
			for (int i = 0; i < availableMods.Count; i++) {
				if (availableMods[i].Name == LoadedMod.CoreModFolderName) {
					selectedIndex = i;
					selectedFromAvailable = true;
					selectedFromActive = false;
				}
			}
			// Once the core mod has been selected, activate it.
			ActivateSelectedMod();
		}

		// Open a dialog to display the information about the mod that is normally
		// displayed in the main mods config window in vanilla.
		protected void ShowInfoForSelectedMod()
		{
			// Figure out which mod is the selected mod
			InstalledMod mod = null;
			if (selectedFromActive) {
				mod = activeMods[selectedIndex];
			}
			else if (selectedFromAvailable) {
				mod = availableMods[selectedIndex];
			}
			else {
				return;
			}

			// Open the dialog
			Find.WindowStack.Add(new Dialog_ModInfo(mod));
		}

		// Handle the reloading of mods when the Mods screen is closed.
		public override void PostClose()
		{
			base.PostClose();

			// Only reloads mods if the user actually made changes to the mod configuration.
			if (HasConfigurationChanged) {

				// Deactivate all of the mods...
				foreach (InstalledMod mod in InstalledModLister.AllInstalledMods) {
					mod.Active = false;
				}
				// ...and then activate them in the selected order
				foreach (InstalledMod mod in activeMods) {
					mod.Active = true;
				}

				ModsConfig.Save();
				LongEventHandler.QueueLongEvent(delegate {
					PlayDataLoader.ClearAllPlayData();
					PlayDataLoader.LoadAllPlayData(false);
				}, "LoadingLongEvent", true, null);
			}
			else {
				Log.Message("Mod selection did not change.  Skipping mod reload.");
			}

			// TODO: Alpha 12
			//Find.WindowStack.Add(new Page_MainMenu());
		}

		// Check to see if the mod configuration has changed.
		protected bool HasConfigurationChanged {
			get {
				if (previousActiveMods.Count != activeMods.Count) {
					return true;
				}
				int count = activeMods.Count;
				for (int i = 0; i < count; i++) {
					if (previousActiveMods[i] != activeMods[i]) {
						return true;
					}
				}
				return false;
			}
		}

		// Draw an image button.  Similar to Verse.Widgets.ImageButton(), but accepts an additional
		// mouse-over highlight color argument.
		public static bool ImageButton(Rect butRect, Texture2D tex, Color baseColor, Color mouseOverColor)
		{
			if (butRect.Contains(Event.current.mousePosition)) {
				GUI.color = mouseOverColor;
			}
			GUI.DrawTexture(butRect, tex);
			GUI.color = baseColor;
			return Widgets.InvisibleButton(butRect);
		}

		// Draw a button with a graphic icon in the middle.  Similar to Verse.Widgets.TextButton(),
		// but draws an image instead of text in the button.
		public static bool IconButton(Rect rect, Texture texture, Color baseColor, Color highlightColor, bool enabled)
		{
			if (texture == null) {
				return false;
			}

			// Use the inactive color if the button is inactive.
			if (!enabled) {
				GUI.color = InactiveButtonColor;
			}
			else {
				GUI.color = Color.white;
			}

			// Draw the button background.
			Texture2D atlas = ButtonBGAtlas;
			if (rect.Contains(Event.current.mousePosition)) {
				atlas = ButtonBGAtlasMouseover;
				if (Input.GetMouseButton(0)) {
					atlas = ButtonBGAtlasClick;
				}
			}
			Widgets.DrawAtlas(rect, atlas);

			// Set up the icon image position.  It's centered in the button rectangle.
			Rect iconRect = new Rect(rect.x + (rect.width / 2) - (texture.width / 2), rect.y + (rect.height / 2) - (texture.height / 2), texture.width, texture.height);

			// Draw the icon image.
			if (!enabled) {
				GUI.color = InactiveButtonColor;
			}
			else {
				GUI.color = baseColor;
			}
			if (enabled && rect.Contains(Event.current.mousePosition)) {
				GUI.color = highlightColor;
			}
			GUI.DrawTexture(iconRect, texture);
			GUI.color = Color.white;

			// If the button is inactive, always return false.  Otherwise return true
			// if the user clicked on the button.
			if (!enabled) {
				return false;
			}
			return Widgets.InvisibleButton(rect);
		}
	}
}

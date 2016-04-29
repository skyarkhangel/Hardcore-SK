
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace EdB.ModOrder
{
	/*
	 *  This initializer class gets loaded by the fake building definition in the 
	 *  mod definition file: Resources/Defs/ThingDefs/EdModOrder.xml.  It's defined
	 *  as an ITab because RimWorld will create a single instance of each tab that's
	 *  defined in the mod XML.  It will only do this once when a mod is loaded, so
	 *  it makes it an ideal place to do initialization for your mod.
	 */
	public class ModInitializer : ITab
    {
		public ModInitializer() : base()
        {
			// Because mods are loaded in another thread, you need to be careful about initializing
			// or loading certain Unity objects.  Using this method makes the action run asynchronously,
			// but in the main thread.
			LongEventHandler.ExecuteWhenFinished(() => {
				Log.Message("Initialized the " + ModController.ModName + " mod");

				// Creates a new Unity Engine GameObject (http://docs.unity3d.com/ScriptReference/GameObject.html)
				// You can name this however you want as long as it doesn't conflict with an
				// existing game object, so don't choose a name that's too generic.  I'm
				// using a different game object for each of my mods, but you could also
				// try to use the same game object for all of your mods.  In that case,
				// you would look for the existing game object before creating a new one.
				// Cleaning up when the mod is unloaded would also be different in that case.
				GameObject gameObject = new GameObject(ModController.GameObjectName);

				// RimWorld has two Unity game "levels"--which don't correspond to what you'd
				// normally think of when you think of game levels.  The menus before you enter
				// gameplay are one "level" and the gameplay itself is the other.  Normally,
				// when a new level gets loaded, all game objects from the previous level get
				// destroyed.  We don't want that to happen with our game object, so we mark
				// it accordingly.
				MonoBehaviour.DontDestroyOnLoad(gameObject);

				// The ModController is a MonoBehavior (http://docs.unity3d.com/ScriptReference/MonoBehaviour.html)
				// You can attach event-driven behaviors to it.  For example, if you define
				// an Update() method in it, it will run that method every frame.  If you
				// define an OnLevelLoaded() method in it, it will run that method whenever
				// RimWorld switches between the main menus and gameplay.  We take advantage
				// of this to control our mod's behavior.  Each of my mods has a single
				// controller that is attached to its GameObject.
				gameObject.AddComponent<ModController>();
			});
        }

		// This method is declared as virtual in the parent ITab class, so we need to
		// define it, but it doesn't do anything.
		protected override void FillTab() {
			return;
		}
    }
}

using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Gastronomy.Timetable
{
	/// <summary>
	/// Wrapper for Pawn_TimetableTracker
	/// </summary>
	public class TimetableBool : IExposable
	{
		public List<bool> times;
		private Dictionary<bool, Texture2D> colorTextureInt;

		public bool CurrentAssignment(Map map) => times[GenLocalDate.HourOfDay(map)];

		public TimetableBool()
		{
			times = new List<bool>();
			for (int i = 0; i < 24; i++)
			{
				bool item = i > 5 && i <= 21;
				times.Add(item);
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref times, "times");
		}

		public bool GetAssignment(int hour)
		{
			return times[hour];
		}

		public void SetAssignment(int hour, bool ta)
		{
			times[hour] = ta;
		}

		public Texture2D GetTexture(bool assignment)
		{
			if (colorTextureInt == null)
			{
				colorTextureInt = new Dictionary<bool, Texture2D>
				{
					{true, SolidColorMaterials.NewSolidColorTexture(TimeAssignmentDefOf.Work.color)}, 
					{false, SolidColorMaterials.NewSolidColorTexture(TimeAssignmentDefOf.Sleep.color)}
				};
			}

			if (colorTextureInt.TryGetValue(assignment, out var texture)) return texture;
			throw new Exception("Missing texture!");
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace ZhentarFix
{
	class PawnRotatorFix : PawnRotator
	{
		public PawnRotatorFix(Pawn pawn) : base(pawn)
		{
		}


		private static readonly Func<PawnRotator, Pawn> pawnGet = Utils.GetFieldAccessor<PawnRotator, Pawn>("pawn");

		private static readonly FieldInfo PawnFuncInfo = typeof(PawnRotator).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

		private Pawn pawn { get { return pawnGet(this); } set { PawnFuncInfo.SetValue(this, value); } }

		private static Action<PawnRotator, IntVec3> FaceAdjacentCell = Utils.GetMethodInvoker<PawnRotator, IntVec3>("FaceAdjacentCell");

		[DetourMember]
		public new void PawnRotatorTick()
		{
			if (pawn.Destroyed)
			{
				return;
			}
			if (pawn.pather.Moving)
			{
				if (pawn.pather.curPath == null || pawn.pather.curPath.NodesLeftCount < 1)
				{
					return;
				}
				FaceAdjacentCell(this, pawn.pather.nextCell);
			}
			else
			{
				Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
				if (stance_Busy != null && stance_Busy.focusTarg.IsValid)
				{
					if (stance_Busy.focusTarg.HasThing)
					{
						Face(stance_Busy.focusTarg.Thing.DrawPos);
					}
					else
					{
						FaceCell(stance_Busy.focusTarg.Cell);
					}
					return;
				}
				if (pawn.jobs.curJob != null)
				{
					LocalTargetInfo target = pawn.CurJob.GetTarget(pawn.jobs.curDriver.rotateToFace);
					if (target.HasThing)
					{
						bool flag = false;
						IntVec3 c = default(IntVec3);
						CellRect cellRect = target.Thing.OccupiedRect();
						for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
						{
							for (int j = cellRect.minX; j <= cellRect.maxX; j++)
							{
								if (pawn.Position == new IntVec3(j, 0, i))
								{
									Face(target.Thing.DrawPos);
									return;
								}
							}
						}
						for (int k = cellRect.minZ; k <= cellRect.maxZ; k++)
						{
							for (int l = cellRect.minX; l <= cellRect.maxX; l++)
							{
								IntVec3 intVec = new IntVec3(l, 0, k);
								if (intVec.AdjacentToCardinal(pawn.Position))
								{
									FaceAdjacentCell(this, intVec);
									return;
								}
								if (intVec.AdjacentTo8Way(pawn.Position))
								{
									flag = true;
									c = intVec;
								}
							}
						}
						if (flag)
						{
							if (DebugViewSettings.drawPawnRotatorTarget)
							{
								pawn.Map.debugDrawer.FlashCell(pawn.Position, 0.6f, "jbthing");
								GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), c.ToVector3Shifted());
							}
							FaceAdjacentCell(this, c);
							return;
						}
					}
					else if (pawn.Position.AdjacentTo8Way(target.Cell))
					{
						if (DebugViewSettings.drawPawnRotatorTarget)
						{
							pawn.Map.debugDrawer.FlashCell(pawn.Position, 0.2f, "jbloc");
							GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), target.Cell.ToVector3Shifted());
						}
						FaceAdjacentCell(this, target.Cell);
						return;
					}
				}
			}
		}
	}
}

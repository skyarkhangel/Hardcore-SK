using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace XenoPrecept
{
	public class Thought_IdeoLeaderXenoResentment : Thought_Situational
	{
		// Token: 0x17000F5A RID: 3930
		// (get) Token: 0x060057FE RID: 22526 RVA: 0x001DF75F File Offset: 0x001DD95F
		public Pawn Leader
		{
			get
			{
				return this.pawn.Faction.leader;
			}
		}

		// Token: 0x17000F5B RID: 3931
		// (get) Token: 0x060057FF RID: 22527 RVA: 0x001DF771 File Offset: 0x001DD971
		public override string LabelCap
		{
			get
			{
				return "IdeoLeaderDifferentSpeciesThoughtLabel".Translate(this.Leader.def.label);
			}
		}

		// Token: 0x17000F5C RID: 3932
		// (get) Token: 0x06005800 RID: 22528 RVA: 0x001DF797 File Offset: 0x001DD997
		public override string Description
		{
			get
			{
				return base.CurStage.description.Formatted(this.Leader.def.label);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_Recruitable : PawnColumnWorker_Icon
    {
        protected override string GetIconTip(Pawn pawn)
        {
            if (pawn?.guest?.Recruitable == false)
            {
                return "Unrecruitable".Translate().AsTipTitle().CapitalizeFirst() + "\n\n" + "UnrecruitableDesc".Translate(pawn.Named("PAWN")).Resolve();
            }
            return null;
        }

        public override int Compare(Pawn a, Pawn b)
        {
            //either do complicated stuff with negation or just swap a and b.
            return b.guest?.Recruitable.CompareTo(a.guest?.Recruitable) ?? base.Compare(b, a);
        }

        protected override Texture2D GetIconFor(Pawn pawn)
        {
            return (pawn.guest?.Recruitable ?? true) ? null : StaticConstructorOnGameStart.UnwaveringlyLoyal;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.AI;
namespace MAD
{

    public class Util 
    {

        #region color related
        public static Color IntToColour(ColorInt col)
        {
            float r = (float)col.r / 255f;
            float g = (float)col.g / 255f;
            float b = (float)col.b / 255f;
            float a = (float)1f;

            return new Color(r, g, b, a);
        }
        #endregion

        #region mapmesh related
        public static void updateMap(IntVec3 pos)
        {
            Find.MapDrawer.MapMeshDirty(pos, MapMeshFlag.Things);
            Find.MapDrawer.MapMeshDirty(pos, MapMeshFlag.GroundGlow);
            Find.GlowGrid.MarkGlowGridDirty(pos);
        }
        #endregion

        #region Glower related
        //provides a new Compglower
        public static CompGlower newCompGlower(ThingWithComps parent, ColorInt glowColour, float glowRadius)
        {
            CompGlower Comp_NewGlower = new CompGlower();
            Comp_NewGlower.parent = parent;


            CompProperties CompProp_New = new CompProperties();
            CompProp_New.compClass = typeof(CompGlower);
            CompProp_New.glowColor = glowColour;
            CompProp_New.glowRadius = glowRadius;

            Comp_NewGlower.Initialize(CompProp_New);

            return Comp_NewGlower;
        }


        //moved it to util for easier implementation with MAD
        public static void DestroyNCreateGlower(ThingWithComps parent, ColorInt glowColour, float glowRadius)
        {
            CompGlower oldGlower = null;
            CompPowerTrader pwrTrader = null;

            List<ThingComp> list = parent.GetComps();

            foreach (ThingComp comp in list)
            {
                if (typeof(CompGlower) == comp.GetType())
                {
                    if (oldGlower == null)
                    {
                        oldGlower = (CompGlower)comp;
                    }

                }
                if (typeof(CompPowerTrader) == comp.GetType())
                {
                    pwrTrader = (CompPowerTrader)comp;
                }
            }
            if (oldGlower != null)
            {

                Boolean isLitoldGlower = oldGlower.Lit;
                oldGlower.Lit = false;

                CompGlower newGlower = Util.newCompGlower(parent, glowColour, glowRadius);
                list.Remove(oldGlower);
                list.Add(newGlower);

                parent.SetComps(list);

                newGlower.Lit = false;
                updateMap(parent.Position);

                if (pwrTrader != null)
                {
                    if (isLitoldGlower && pwrTrader.PowerOn)
                    {
                        newGlower.Lit = true;
                    }
                }
            }

        }
        #endregion


    }

}

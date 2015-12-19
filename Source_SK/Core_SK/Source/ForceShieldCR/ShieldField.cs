using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Reflection;
using Enhanced_Development.ShieldUtils;
using Combat_Realism;

namespace Combat_Realism_Compatibility.Shields
{
    class ShieldField : Enhanced_Development.Shields.ShieldField
    {

        public ShieldField(Building_Shield shieldBuilding, IntVec3 pos, int shieldMaxShieldStrength, int shieldInitialShieldStrength, int shieldShieldRadius, int shieldRechargeTickDelay, int shieldRecoverWarmup, bool shieldBlockIndirect, bool shieldBlockDirect, bool shieldFireSupression, bool shieldInterceptDropPod, bool shieldStructuralIntegrityMode, float colourRed, float colourGreen, float colourBlue)
            : base(shieldBuilding, pos, shieldMaxShieldStrength, shieldInitialShieldStrength, shieldShieldRadius, shieldRechargeTickDelay, shieldRecoverWarmup, shieldBlockIndirect, shieldBlockDirect, shieldFireSupression, shieldInterceptDropPod, shieldStructuralIntegrityMode, colourRed, colourGreen, colourBlue)
        {

        }

        /// <summary>
        /// Finds all projectiles at the position and destroys them
        /// </summary>
        /// <param name="square">The current square to protect</param>
        /// <param name="flag_direct">Block direct Fire</param>
        /// <param name="flag_indirect">Block indirect Fire</param>
        public override void ProtectSquare(IntVec3 square, bool flag_direct, bool flag_indirect, bool IFFcheck)
        {

            //Log.Message("Combat_Realism_Compatibility   ProtectSquare()");

            //Ignore squares outside the map
            if (!square.InBounds())
            {
                return;
            }

            if ((!shieldBlockIndirect || !flag_indirect) && (!shieldBlockDirect || !flag_direct))
            {
                return;
            }

            List<Thing> things = Find.ThingGrid.ThingsListAt(square);
            List<Thing> thingsToDestroy = new List<Thing>();

            for (int i = 0, l = things.Count(); i < l; i++)
            {

                if (things[i] != null && things[i] is Projectile)
                {
                    //Assign to variable
                     Projectile _pr = (Projectile)things[i];
                    if (!_pr.Destroyed && ((shieldBlockIndirect && flag_indirect && _pr.def.projectile.flyOverhead) || (shieldBlockDirect && flag_direct && !_pr.def.projectile.flyOverhead)))
                    {
                        bool wantToIntercept = true;

                        //Check IFF
                        if (IFFcheck == true)
                        {
                            //Log.Message("IFFcheck == true");
                            Thing launcher = ReflectionHelper.GetInstanceField(typeof(Projectile), _pr, "launcher") as Thing;

                            if (launcher != null)
                            {
                                if (launcher.Faction != null)
                                {
                                    if (launcher.Faction.def != null)
                                    {
                                        //Log.Message("launcher != null");
                                        if (launcher.Faction.def == FactionDefOf.Colony)
                                        {
                                            wantToIntercept = false;
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                            }
                        }

                        //Check OverShoot
                        if (_pr.def.projectile.flyOverhead)
                        {
                            if (this.WillTargetLandInRange(_pr))
                            {
                                //Log.Message("Fly Over");
                            }
                            else
                            {
                                wantToIntercept = false;
                                //Log.Message("In Range");
                            }
                        }


                        if (wantToIntercept)
                        {

                            //Detect proper collision using angles
                            Quaternion targetAngle = _pr.ExactRotation;

                            Vector3 projectilePosition2D = _pr.ExactPosition;
                            projectilePosition2D.y = 0;

                            Vector3 shieldPosition2D = Vectors.IntVecToVec(position);
                            shieldPosition2D.y = 0;

                            Quaternion shieldProjAng = Quaternion.LookRotation(projectilePosition2D - shieldPosition2D);


                            if ((Quaternion.Angle(targetAngle, shieldProjAng) > 90) || IFFcheck)
                            {

                                //On hit effects
                                MoteThrower.ThrowLightningGlow(_pr.ExactPosition, 0.5f);
                                //On hit sound
                                HitSoundDef.PlayOneShot(_pr.Position);
                                //Damage the shield
                                ProcessDamage(_pr.def.projectile.damageAmountBase);
                                //add projectile to the list of things to be destroyed
                                thingsToDestroy.Add(_pr);
                                if (!isOnline())
                                {
                                    //Stop if the shield was drained
                                    break;
                                }
                            }

                        }

                    }
                }


                if (things[i] != null && things[i] is ProjectileCR)
                {
                    //Assign to variable
                    ProjectileCR _pr = (ProjectileCR)things[i];
                    if (!_pr.Destroyed && ((shieldBlockIndirect && flag_indirect && _pr.def.projectile.flyOverhead) || (shieldBlockDirect && flag_direct && !_pr.def.projectile.flyOverhead)))
                    {
                        bool wantToIntercept = true;

                        //Check IFF
                        if (IFFcheck == true)
                        {
                            //Log.Message("IFFcheck == true");
                            Thing launcher = ReflectionHelper.GetInstanceField(typeof(ProjectileCR), _pr, "launcher") as Thing;

                            if (launcher != null)
                            {
                                if (launcher.Faction != null)
                                {
                                    if (launcher.Faction.def != null)
                                    {
                                        //Log.Message("launcher != null");
                                        if (launcher.Faction.def == FactionDefOf.Colony)
                                        {
                                            wantToIntercept = false;
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                            }
                        }

                        //Check OverShoot
                        if (_pr.def.projectile.flyOverhead)
                        {
                            if (this.WillTargetLandInRange(_pr))
                            {
                                //Log.Message("Fly Over");
                            }
                            else
                            {
                                wantToIntercept = false;
                                //Log.Message("In Range");
                            }
                        }


                        if (wantToIntercept)
                        {

                            //Detect proper collision using angles
                            Quaternion targetAngle = _pr.ExactRotation;

                            Vector3 projectilePosition2D = _pr.ExactPosition;
                            projectilePosition2D.y = 0;

                            Vector3 shieldPosition2D = Vectors.IntVecToVec(position);
                            shieldPosition2D.y = 0;

                            Quaternion shieldProjAng = Quaternion.LookRotation(projectilePosition2D - shieldPosition2D);


                            if ((Quaternion.Angle(targetAngle, shieldProjAng) > 90) || IFFcheck)
                            {

                                //On hit effects
                                MoteThrower.ThrowLightningGlow(_pr.ExactPosition, 0.5f);
                                //On hit sound
                                HitSoundDef.PlayOneShot(_pr.Position);
                                //Damage the shield
                                ProcessDamage(_pr.def.projectile.damageAmountBase);
                                //add projectile to the list of things to be destroyed
                                thingsToDestroy.Add(_pr);
                                if (!isOnline())
                                {
                                    //Stop if the shield was drained
                                    break;
                                }
                            }

                        }

                    }
                }




            }
            foreach (Thing currentThing in thingsToDestroy)
            {
                currentThing.Destroy();
            }

        }

        /// <summary>
        /// Checks if the projectile will land within the shield or pass over.
        /// </summary>
        /// <param name="projectile">The specific projectile that is being checked</param>
        /// <returns>True if the projectile will land close, false if it will be far away.</returns>
        public bool WillTargetLandInRange(ProjectileCR projectile)
        {
            Vector3 targetLocation = GetTargetLocationFromProjectile(projectile);

            if (Vector3.Distance(this.position.ToVector3(), targetLocation) > this.shieldShieldRadius)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public Vector3 GetTargetLocationFromProjectile(ProjectileCR projectile)
        {
            FieldInfo fieldInfo = projectile.GetType().GetField("destination", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            Vector3 reoveredVector = (Vector3)fieldInfo.GetValue(projectile);
            return reoveredVector;
        }
    }
}

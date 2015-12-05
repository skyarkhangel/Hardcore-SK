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

namespace Enhanced_Development.Shields
{
    public class ShieldField : IExposable
    {

        #region Variables
        private const int DAMAGE_TO_FIRE = 10;
        private const int DAMAGE_FROM_FIRE = 2;
        private const int FIRE_SUPRESSION_TICK_DELAY = 15;

        //Position of the shield, used to calculate projectile collisions
        public IntVec3 position;

        //Maximum shield strength
        public int shieldMaxShieldStrength;

        //Shield strength when going online
        private int shieldInitialShieldStrength;

        //Radius of the Shield
        public int shieldShieldRadius;

        //How many ticks between recharging the shield by 1 (lower number will recharge faster)
        public int shieldRechargeTickDelay;

        //How long to wait before starting to charge
        public int shieldRecoverWarmup;

        public bool shieldBlockIndirect;

        public bool shieldBlockDirect;

        public bool shieldFireSupression;

        public bool shieldInterceptDropPod;

        public bool shieldStructuralIntegrityMode;

        List<IntVec3> squares = new List<IntVec3>();

        public Building_Shield shieldBuilding;

        public float colourRed;
        public float colourGreen;
        public float colourBlue;


        //Current power
        private int shieldCurrentStrength_base = 0;
        //Tick index - this increases by 1 every tick
        private long tick = 0;
        //Whether the shield is online
        private bool online = false;
        //Warm-up ticks now
        private int warmupTicksCurrent = 0;

        //Ratio of lost power per damage
        private const float powerToDamage = 1f;

        //public List<string> SIFBuildings;

        Material currentMatrialColour;

        #endregion

        #region Accessors

        /// <summary>
        /// Getter for the power (shield health)
        /// </summary>
        public int shieldCurrentStrength
        {
            get
            {
                return this.shieldCurrentStrength_base;
            }
            set
            {
                if (value != this.shieldCurrentStrength_base)
                {
                    this.shieldCurrentStrength_base = Math.Max(0, value);
                    //Go offline of the power was lost
                    if (value <= 0)
                    {
                        online = false;
                    }
                }
            }
        }

        /// <summary>
        /// Get the Warmup Power
        /// </summary>
        public int warmupPower
        {
            get
            {
                if (this.status == enumShieldStatus.Loading)
                {
                    return warmupTicksCurrent;
                }
                else return 0;
            }
        }

        #endregion

        //Hit sound
        public static readonly SoundDef HitSoundDef = SoundDef.Named("Shields_HitShield");

        /// <summary>
        /// Getter for the shield status, used to chose correct power requirements
        /// </summary>
        public enumShieldStatus status
        {
            get
            {
                if (!this.enabled)
                {
                    return enumShieldStatus.Disabled;
                }
                else if (!online)
                {
                    return enumShieldStatus.Loading;
                }
                else if (online && shieldCurrentStrength < shieldMaxShieldStrength)
                {
                    return enumShieldStatus.Charging;
                }
                else
                {
                    return enumShieldStatus.Sustaining;
                }
            }
        }
        //When false, shield will be inactive
        private bool enabled_internal = true;

        //Getter and setter for enabled status
        public bool enabled
        {
            get
            {
                return enabled_internal;
            }
            set
            {
                if (value != enabled_internal)
                {
                    enabled_internal = value;
                    if (value)
                    {
                        //online = false;
                        warmupTicksCurrent = 0;
                    }
                    else
                    {
                        online = false;
                        shieldCurrentStrength = 0;
                    }
                }
            }
        }

        #region Setup

        //Constructor
        public ShieldField(Building_Shield shieldBuilding, IntVec3 pos, int shieldMaxShieldStrength, int shieldInitialShieldStrength, int shieldShieldRadius, int shieldRechargeTickDelay, int shieldRecoverWarmup, bool shieldBlockIndirect, bool shieldBlockDirect, bool shieldFireSupression, bool shieldInterceptDropPod, bool shieldStructuralIntegrityMode, float colourRed, float colourGreen, float colourBlue)
        {
            this.shieldBuilding = shieldBuilding;
            position = pos;
            //shieldCurrentStrength = 0;
            this.shieldMaxShieldStrength = shieldMaxShieldStrength;
            this.shieldInitialShieldStrength = shieldInitialShieldStrength;
            this.shieldShieldRadius = shieldShieldRadius;
            this.shieldRechargeTickDelay = shieldRechargeTickDelay;
            this.shieldRecoverWarmup = shieldRecoverWarmup;

            this.shieldBlockIndirect = shieldBlockIndirect;

            this.shieldBlockDirect = shieldBlockDirect;

            this.shieldFireSupression = shieldFireSupression;
            this.shieldInterceptDropPod = shieldInterceptDropPod;
            this.shieldStructuralIntegrityMode = shieldStructuralIntegrityMode;

            this.colourRed = colourRed;
            this.colourGreen = colourGreen;
            this.colourBlue = colourBlue;


        }

        //Blank constructor required by scribes
        public ShieldField()
        {

        }
        
        #endregion

        private void StartupField(int desiredPower)
        {
            //This code runs when warmup is finished
            online = true;
            warmupTicksCurrent = 0;
            shieldCurrentStrength = desiredPower;
        }

        //Tick - here the projectiles are being found
        public void ShieldTick(bool powerAvalable, bool flag_direct, bool flag_indirect, bool flag_fireSupression, bool flag_InterceptDropPod, bool shieldRepairMode)
        {
            //Sleep if disabled
            if (!this.enabled)
            {
                return;
            }
            tick++;
            //Find the projectiles around shield
            if (online && shieldCurrentStrength > 0)
            {
                if (this.shieldStructuralIntegrityMode)
                {
                    //List of squares around the shield centre
                    //IEnumerable<IntVec3> squares = GenRadial.RadialCellsAround(new IntVec3(0, 0, 0), shieldShieldRadius, false);
                    SetupCurrentSquares();

                    foreach (IntVec3 square in this.squares)
                    {
                        ProtectSquare(square, flag_direct, flag_indirect, true);
                        supressFire(flag_fireSupression, square);
                        repairSytem(square, shieldRepairMode);

                        //Stop if the power was drained
                        if (shieldCurrentStrength <= 0)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    //List of squares around the shield centre
                    IEnumerable<IntVec3> squares = GenRadial.RadialCellsAround(new IntVec3(0, 0, 0), shieldShieldRadius, false);

                    foreach (IntVec3 square in squares)
                    {
                        //Only use squares around, not inside
                        if (Vectors.VectorSize(square) >= (float)shieldShieldRadius - 1.5f)
                        {
                            ProtectSquare(square + position, flag_direct, flag_indirect, false);
                        }
                        //Stop if the power was drained
                        if (shieldCurrentStrength <= 0)
                        {
                            break;
                        }
                    }

                    supressFire(flag_fireSupression);
                    interceptPods(flag_InterceptDropPod);
                }
            }

            //Regenerate shield if necessary
            if (online && (tick % shieldRechargeTickDelay == 0) && shieldCurrentStrength < shieldMaxShieldStrength)
            {
                if (powerAvalable)
                {
                    shieldCurrentStrength++;
                }
                else
                {
                    shieldCurrentStrength--;
                }
            }

            //Try to get online - warmup
            if (!online && powerAvalable)
            {
                if (warmupTicksCurrent < shieldRecoverWarmup)
                {
                    warmupTicksCurrent++;
                    //Faster warmup when chat is on
                    if (DebugSettings.unlimitedPower)
                    {
                        warmupTicksCurrent += 5;
                    }
                }
                else
                {
                    //This code runs when warmup is finished
                    this.StartupField(this.shieldInitialShieldStrength);
                }
            }
        }

        /// <summary>
        /// Finds all projectiles at the position and destroys them
        /// </summary>
        /// <param name="square">The current square to protect</param>
        /// <param name="flag_direct">Block direct Fire</param>
        /// <param name="flag_indirect">Block indirect Fire</param>
        virtual public void ProtectSquare(IntVec3 square, bool flag_direct, bool flag_indirect, bool IFFcheck)
        {
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
                    Projectile pr = (Projectile)things[i];
                    if (!pr.Destroyed && ((shieldBlockIndirect && flag_indirect && pr.def.projectile.flyOverhead) || (shieldBlockDirect && flag_direct && !pr.def.projectile.flyOverhead)))
                    {
                        bool wantToIntercept = true;

                        //Check IFF
                        if (IFFcheck == true)
                        {
                            //Log.Message("IFFcheck == true");
                            Thing launcher = ReflectionHelper.GetInstanceField(typeof(Projectile), pr, "launcher") as Thing;

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
                        if (pr.def.projectile.flyOverhead)
                        {
                            if (this.WillTargetLandInRange(pr))
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
                            Quaternion targetAngle = pr.ExactRotation;

                            Vector3 projectilePosition2D = pr.ExactPosition;
                            projectilePosition2D.y = 0;

                            Vector3 shieldPosition2D = Vectors.IntVecToVec(position);
                            shieldPosition2D.y = 0;

                            Quaternion shieldProjAng = Quaternion.LookRotation(projectilePosition2D - shieldPosition2D);


                            if ((Quaternion.Angle(targetAngle, shieldProjAng) > 90) || IFFcheck)
                            {

                                //On hit effects
                                MoteThrower.ThrowLightningGlow(pr.ExactPosition, 0.5f);
                                //On hit sound
                                HitSoundDef.PlayOneShot(pr.Position);
                                //Damage the shield
                                ProcessDamage(pr.def.projectile.damageAmountBase);
                                //add projectile to the list of things to be destroyed
                                thingsToDestroy.Add(pr);
                                if (!isOnline())
                                {
                                    //Stop if the shield was drained
                                    break;
                                }
                            }

                        }
                        else
                        {
                            //Log.Message("Skip");
                        }

                    }
                }
            }
            foreach (Thing currentThing in thingsToDestroy)
            {
                currentThing.Destroy();
            }

        }

        private void interceptPods(bool flag_InterceptDropPod)
        {
            if (this.shieldInterceptDropPod && flag_InterceptDropPod)
            {
                IEnumerable<Thing> dropPods = Find.ListerThings.ThingsOfDef(ThingDefOf.DropPod);

                if (dropPods != null)
                {
                    //Test to protect the entire map from droppods.
                    //IEnumerable<Thing> closeFires = dropPods.Where<Thing>(t => t.Position.InHorDistOf(this.position, 9999999.0f));

                    IEnumerable<Thing> closeFires = dropPods.Where<Thing>(t => t.Position.InHorDistOf(this.position, this.shieldShieldRadius));

                    foreach (RimWorld.DropPod currentDropPod in closeFires.ToList())
                    {
                        //currentDropPod.Destroy();

                        currentDropPod.Destroy(DestroyMode.Vanish);
                        BodyPartDamageInfo bodyPartDamageInfo = new BodyPartDamageInfo(new BodyPartHeight?(), new BodyPartDepth?(BodyPartDepth.Outside));
                        new ExplosionInfo()
                        {
                            center = currentDropPod.Position,
                            radius = 1,
                            dinfo = new DamageInfo(DamageDefOf.Flame, 10, currentDropPod)

                        }.DoExplosion();
                    }
                }
            }
        }

        private void supressFire(bool flag_fireSupression)
        {
            if (this.shieldFireSupression && flag_fireSupression && (tick % FIRE_SUPRESSION_TICK_DELAY == 0))
            {
                IEnumerable<Thing> fires = Find.ListerThings.ThingsOfDef(ThingDefOf.Fire);

                if (fires != null)
                {
                    IEnumerable<Thing> closeFires = fires.Where<Thing>(t => t.Position.InHorDistOf(this.position, this.shieldShieldRadius));

                    if (closeFires != null)
                    {
                        //List<Thing> fireTo
                        foreach (Fire currentFire in closeFires.ToList())
                        {
                            if (this.shieldCurrentStrength > DAMAGE_FROM_FIRE)
                            {
                                //Damage the shield
                                ProcessDamage(DAMAGE_FROM_FIRE);

                                currentFire.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, DAMAGE_TO_FIRE, this.shieldBuilding));
                            }
                        }
                    }
                }


            }
        }

        private void supressFire(bool flag_fireSupression, IntVec3 position)
        {
            if (this.shieldFireSupression && flag_fireSupression && (tick % FIRE_SUPRESSION_TICK_DELAY == 0))
            {
                IEnumerable<Thing> things = Find.ThingGrid.ThingsAt(position);
                List<Thing> fires = new List<Thing>();

                foreach (Thing currentThing in things)
                {
                    if (currentThing is Fire)
                    {
                        fires.Add(currentThing);
                    }
                }

                foreach (Fire currentFire in fires.ToList())
                {
                    if (this.shieldCurrentStrength > DAMAGE_FROM_FIRE)
                    {
                        //Damage the shield
                        ProcessDamage(DAMAGE_FROM_FIRE);

                        currentFire.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, DAMAGE_TO_FIRE, this.shieldBuilding));
                    }
                }
            }
        }

        private void repairSytem(IntVec3 position, bool shieldRepairMode)
        {
            if (shieldRepairMode)
            {
                List<Thing> things = Find.ThingGrid.ThingsListAt(position);

                foreach (Thing thing in things)
                {
                    if (thing is Building)
                    {
                        if (thing.HitPoints < thing.MaxHitPoints)
                        {
                            if (this.shieldCurrentStrength > 1)
                            {
                                //Damage the shield
                                ProcessDamage(1);

                                //thing.Health += 1;
                                thing.HitPoints += 1;
                            }
                        }
                    }
                }
            }
        }

        private void SetupCurrentSquares()
        {
            this.squares.Clear();
            
            IEnumerable<IntVec3> possibleSquares = GenRadial.RadialCellsAround(this.position, shieldShieldRadius, true);

            if (this.shieldStructuralIntegrityMode)
            {
                foreach (IntVec3 square in possibleSquares)
                {
                    //Only use squares around, not inside
                    if (Vectors.VectorSize(square) >= (float)shieldShieldRadius - 1.5f)
                    {
                        List<Thing> things = Find.ThingGrid.ThingsListAt(square);

                        for (int i = 0, l = things.Count(); i < l; i++)
                        {
                            if (things[i] is Building)
                            {
                                Building building = (Building)things[i];

                                if (isBuildingValid(building))
                                {
                                    this.squares.Add(square);
                                    i = 99999;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                this.squares.AddRange(possibleSquares);
            }


        }

        public bool isBuildingValid(Thing currentBuilding)
        {
            if (this.shieldBuilding.SIFBuildings != null)
            {
                if (this.shieldBuilding.SIFBuildings.Contains(currentBuilding.def.defName))
                {
                    return true;
                }
            }
            else
            {
                Log.Error("ShieldField.validBuildings Not set up properly");
            }

            return false;
        }

        #region drawing

        //Draw the field on map
        public void DrawField(Vector3 center)
        {
            //SetupCurrentSquares();

            if (this.status == enumShieldStatus.Charging ||
               this.status == enumShieldStatus.Sustaining ||
               (this.status == enumShieldStatus.Loading && (shieldRecoverWarmup - warmupTicksCurrent) < 60)
              )
            {
                if (this.shieldStructuralIntegrityMode)
                {
                    foreach (IntVec3 square in this.squares)
                    {
                        DrawSubField(Vectors.IntVecToVec(square), 0.8f);
                    }
                }
                else
                {
                    DrawSubField(center, shieldShieldRadius);
                }
            }
        }

        public void DrawSubField(Vector3 position, float shieldShieldRadius)
        {
            position = position + (new Vector3(0.5f, 0f, 0.5f));

            Vector3 s = new Vector3(shieldShieldRadius, 1f, shieldShieldRadius);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(position, Quaternion.identity, s);

            if (currentMatrialColour == null)
            {
                //Log.Message("Creating currentMatrialColour");
                currentMatrialColour = SolidColorMaterials.NewSolidColorMaterial(new Color(colourRed, colourGreen, colourBlue, 0.15f), ShaderDatabase.MetaOverlay);
            }

            UnityEngine.Graphics.DrawMesh(Enhanced_Development.ShieldUtils.Graphics.CircleMesh, matrix, currentMatrialColour, 0);

        }

        #endregion

        /// <summary>
        /// Inspect string data
        /// </summary>
        /// <returns></returns>
        public string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            //stringBuilder.Append(base.GetInspectString());
            if (isOnline())
            {
                stringBuilder.AppendLine("Shield: " + this.shieldCurrentStrength + "/" + this.shieldMaxShieldStrength);
            }
            else if (this.enabled)
            {
                //stringBuilder.AppendLine("Initiating shield: " + ((warmupTicks * 100) / recoverWarmup) + "%");
                stringBuilder.AppendLine("Ready in " + Math.Round(GenDate.TicksToSeconds(shieldRecoverWarmup - warmupTicksCurrent)) + " seconds.");
            }
            else
            {
                stringBuilder.AppendLine("Shield disabled!");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns true if the shield is online
        /// </summary>
        /// <returns>True if this is onliner</returns>
        public bool isOnline()
        {
            return online && this.shieldCurrentStrength > 0;
        }

        /// <summary>
        /// Save / Load the Game
        /// </summary>
        public void ExposeData()
        {
            Scribe_Values.LookValue<int>(ref this.shieldMaxShieldStrength, "shieldMaxShieldStrength", 0, false);
            Scribe_Values.LookValue<int>(ref this.shieldInitialShieldStrength, "shieldInitialShieldStrength", 0, false);
            Scribe_Values.LookValue<int>(ref this.shieldShieldRadius, "shieldShieldRadius", 0, false);
            Scribe_Values.LookValue<int>(ref this.shieldRechargeTickDelay, "shieldRechargeTickDelay", 0, false);
            Scribe_Values.LookValue<int>(ref this.shieldRecoverWarmup, "shieldRecoverWarmup", 0, false);

            Scribe_Values.LookValue<bool>(ref this.shieldBlockIndirect, "shieldBlockIndirect", true, false);
            Scribe_Values.LookValue<bool>(ref this.shieldBlockDirect, "shieldBlockDirect", true, false);
            Scribe_Values.LookValue<bool>(ref this.shieldFireSupression, "shieldFireSupression", false, false);
            Scribe_Values.LookValue<bool>(ref this.shieldStructuralIntegrityMode, "shieldStructuralIntegrityMode", false, false);
            Scribe_Values.LookValue<bool>(ref this.shieldInterceptDropPod, "shieldInterceptDropPod", false, false);


            Scribe_Values.LookValue<int>(ref shieldCurrentStrength_base, "shieldCurrentStrength_base", 0, false);
            Scribe_Values.LookValue<long>(ref this.tick, "tick", 0, false);
            Scribe_Values.LookValue<bool>(ref this.online, "online", false, false);
            Scribe_Values.LookValue<int>(ref this.warmupTicksCurrent, "warmupTicksCurrent", 0, false);


            Scribe_Values.LookValue<float>(ref this.colourRed, "colourRed", 0, false);
            Scribe_Values.LookValue<float>(ref this.colourGreen, "colourGreen", 0, false);
            Scribe_Values.LookValue<float>(ref this.colourBlue, "colourBlue", 0, false);

            Scribe_References.LookReference<Enhanced_Development.Shields.Building_Shield>(ref this.shieldBuilding, "shieldBuilding");
        }

        /// <summary>
        /// Damage the Shield
        /// </summary>
        /// <param name="damage">The amount of damage to give</param>
        public void ProcessDamage(int damage)
        {
            //Prevent negative health
            if (this.shieldCurrentStrength <= 0)
                return;
            this.shieldCurrentStrength -= (int)(((float)damage) * powerToDamage);
        }

        /// <summary>
        /// Checks if the projectile will land within the shield or pass over.
        /// </summary>
        /// <param name="projectile">The specific projectile that is being checked</param>
        /// <returns>True if the projectile will land close, false if it will be far away.</returns>
        public bool WillTargetLandInRange(Projectile projectile)
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

        public Vector3 GetTargetLocationFromProjectile(Projectile projectile)
        {
            FieldInfo fieldInfo = projectile.GetType().GetField("destination", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            Vector3 reoveredVector = (Vector3)fieldInfo.GetValue(projectile);
            return reoveredVector;
        }
    }




}

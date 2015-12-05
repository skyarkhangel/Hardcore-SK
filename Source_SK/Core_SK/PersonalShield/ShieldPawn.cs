using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Core_SK.PersonalShields.Animal
{
    public enum ShieldStatePawn
    {
        Active = 0,
        Charging = 1,
        Inactive = 3
    }

    class ShieldPawn : Pawn
    {
        private int energy = 0;
        public int maxEnergy = 100;
        private ShieldStatePawn m_ShieldState = ShieldStatePawn.Inactive;

        private SoundDef SoundAbsorbDamage = SoundDef.Named("PersonalShieldAbsorbDamage");
        private SoundDef SoundBreak = SoundDef.Named("PersonalShieldBroken");

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.maxEnergy = 100;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue<int>(ref this.energy, "energy", 0, false);
            Scribe_Values.LookValue<int>(ref this.maxEnergy, "maxEnergy", 0, false);
            Scribe_Values.LookValue<ShieldStatePawn>(ref this.m_ShieldState, "shieldState", ShieldStatePawn.Inactive, false);
        }


        #region Gizmoes
        /*

        //public override void Tick()
        //{
        //  //  Log.Warning("SP Tick");
        //    base.Tick();
        //}

        public override IEnumerable<Gizmo> GetGizmos()
        {
            Log.Warning("Giz");
            IEnumerable<Gizmo> temp = base.GetGizmos().ToList();

            Gizmo_PersonalShieldStatus opt1 = new Gizmo_PersonalShieldStatus();

           // opt1.shield = this;

            temp.ToList().Add(opt1);

            return temp;

            //List<Gizmo> list = base.GetGizmos().ToList();

            //list.AddRange(this.apparel.GetGizmos());

            //return list;
        }

        internal class Gizmo_PersonalShieldStatus : Gizmo
        {
            //Links
            public Core_SK.PersonalShields.Apparel_PersonalNanoShield shield;

            //Constants
            private static readonly Texture2D FullTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
            private static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

            //Properties
            public override float Width { get { return 140; } }

            public override GizmoResult GizmoOnGUI(UnityEngine.Vector2 topLeft)
            {
                Rect overRect = new Rect(topLeft.x, topLeft.y, Width, Height);
                Widgets.DrawWindowBackground(overRect);

                Rect inRect = overRect.ContractedBy(6);

                //Item label
                {
                    Rect textRect = inRect;
                    textRect.height = overRect.height / 2;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(textRect, shield.LabelCap);
                }

                //Bar
                {
                    Rect barRect = inRect;
                    barRect.yMin = overRect.y + overRect.height / 2f;
                    //float ePct = shield.Energy / Mathf.Max(1f, shield.GetStatValue(StatDefOf.PersonalShieldEnergyMax));
                    float ePct = shield.Energy / Mathf.Max(1f, shield.maxEnergy);
                    Widgets.FillableBar(barRect, ePct, FullTex, EmptyTex, false);
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    //Widgets.Label(barRect, (shield.Energy * 100).ToString("F0") + " / " + (shield.GetStatValue(StatDefOf.PersonalShieldEnergyMax) * 100).ToString("F0"));
                    Widgets.Label(barRect, (shield.Energy).ToString("F0") + " / " + (shield.maxEnergy).ToString("F0"));
                    Text.Anchor = TextAnchor.UpperLeft;
                }

                return new GizmoResult(GizmoState.Clear);
            }
        }
        */
        #endregion

        public ShieldStatePawn ShieldState
        {
            get
            {
                //If the shield is Inactive dont change it.
                if (this.m_ShieldState != ShieldStatePawn.Inactive)
                {
                    if (this.energy >= this.maxEnergy)
                    {
                        this.m_ShieldState = ShieldStatePawn.Active;
                    }

                    if (this.energy <= 0)
                    {
                        this.m_ShieldState = ShieldStatePawn.Charging;
                    }
                }

                return this.m_ShieldState;
            }

            set
            {
                // if (value == ShieldStatePawn.Active)
                // {
                this.m_ShieldState = value;
                //Log.Warning("m_ShieldState set to" + m_ShieldState.ToString());
                //    this.energy = this.maxEnergy;
                // }
            }
        }

        public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;

            // if (this.ShieldState == ShieldState.Active && ((dinfo.Instigator != null && !dinfo.Instigator.Position.AdjacentTo8Way(this.wearer.Position)) || dinfo.Def.isExplosive))
            //if (this.energy > 0f)
            if (this.ShieldState == ShieldStatePawn.Active)
            {

                absorbed = true;

                if (dinfo.Def == DamageDefOf.HealGlobal)
                {
                    absorbed = false;
                }

                if (dinfo.Def == DamageDefOf.HealInjury)
                {
                    absorbed = false;
                }

                if (dinfo.Def == DamageDefOf.Repair)
                {
                    absorbed = false;
                }

                if (dinfo.Def == DamageDefOf.RestoreBodyPart)
                {
                    absorbed = false;
                }

                if (dinfo.Def == DamageDefOf.SurgicalCut)
                {
                    absorbed = false;
                }


                if (absorbed)
                {

                    this.AbsorbedDamage(dinfo);


                    /*if (dinfo.Def == DamageDefOf.EMP)
                    {
                        this.energy = -1f;
                    }*/


                }


            }


            //Log.Warning(absorbed.ToString());
        }

        private void AbsorbedDamage(DamageInfo dinfo)
        {

            this.energy -= dinfo.Amount;


            Verse.Sound.SoundStarter.PlayOneShot(SoundAbsorbDamage, this.Position);
            Vector3 impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = this.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + (float)dinfo.Amount / 10f);
            MoteThrower.ThrowStatic(loc, ThingDefOf.Mote_ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                MoteThrower.ThrowDustPuff(loc, Rand.Range(0.8f, 1.2f));
            }
            //this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
            //this.KeepDisplaying();

            if (this.energy < 0f)
            {
                this.Break();
            }
        }

        private void Break()
        {
            this.ShieldState = ShieldStatePawn.Charging;

            Verse.Sound.SoundStarter.PlayOneShot(SoundBreak, this.Position);
            MoteThrower.ThrowStatic(this.TrueCenter(), ThingDefOf.Mote_ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                UnityEngine.Vector3 loc = this.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
                MoteThrower.ThrowDustPuff(loc, Rand.Range(0.8f, 1.2f));
            }
            this.energy = 0;
        }

        public override string GetInspectString()
        {
            if (this.ShieldState == ShieldStatePawn.Active)
            {
                return "Shields: " + this.energy + " / " + this.maxEnergy + Environment.NewLine + base.GetInspectString();
            }
            else if (this.ShieldState == ShieldStatePawn.Charging)
            {
                return "Shields Charging: " + this.energy + " / " + this.maxEnergy + Environment.NewLine + base.GetInspectString();
            }

            return base.GetInspectString();
        }


        public bool isCharged()
        {
            if (this.energy >= this.maxEnergy)
            {
                return true;
            }
            return false;
        }

        public void recharge(int chargeAmmount)
        {
            this.energy += chargeAmmount;

            if (this.ShieldState == ShieldStatePawn.Inactive)
            {
                this.ShieldState = ShieldStatePawn.Active;
            }

            if (this.energy > this.maxEnergy)
            {
                this.energy = this.maxEnergy;
            }
        }
    }
}

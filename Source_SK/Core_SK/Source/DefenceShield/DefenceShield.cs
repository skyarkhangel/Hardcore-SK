using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
namespace SK_DefenceShield
{
    internal class Building_DefenceShield : Building
    {
        private const float MinDrawSize = 1.2f;
        private const float MaxDrawSize = 1.55f;
        private const float MaxDamagedJitterDist = 0.05f;
        private const int JitterDurationTicks = 8;
        private float energy;
        private int ticksToReset = -1;
        private int lastAbsorbDamageTick = -9999;
        private Vector3 impactAngleVect;
        private int lastKeepDisplayTick = -9999;
        private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);
        private static readonly Material ShieldRotator = MaterialPool.MatFrom("Things/Building/Security/OmniShield_top", ShaderDatabase.Transparent);
        private int StartingTicksToReset = 3200;
        private float EnergyOnReset = 0.2f;
        private float EnergyLossPerDamage = 0.005f;
        private static readonly SoundDef SoundAbsorbDamage = SoundDef.Named("PersonalShieldAbsorbDamage");
        private static readonly SoundDef SoundBreak = SoundDef.Named("PersonalShieldBroken");
        private static readonly SoundDef SoundReset = SoundDef.Named("PersonalShieldReset");
        protected CompPowerTrader powerComp;
        private List<IntVec3> squares = new List<IntVec3>();
        public float angle = 0;
        public void DrawTurret()
        {
            if (this.powerComp.PowerOn)
            {
                angle += energy / 1f;
                Vector3 s = new Vector3(1f, 1f, 1f);
                s.y = Altitudes.AltitudeFor(AltitudeLayer.VisEffects);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(this.DrawPos + Altitudes.AltIncVect, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, Building_DefenceShield.ShieldRotator, 0);
            }
            else
            {
                angle += energy / 1f;
                Matrix4x4 matrix = default(Matrix4x4);
                Vector3 s = new Vector3(1f, 1f, 1f);
                s.y = Altitudes.AltitudeFor(AltitudeLayer.MoteLow);
                matrix.SetTRS(this.DrawPos + Altitudes.AltIncVect + Altitudes.AltIncVect, Rotation.AsQuat, Vector3.one);
                Graphics.DrawMesh(MeshPool.plane10, matrix, Building_DefenceShield.ShieldRotator, 0);
            }
        }
        internal class Gizmo_PersonalShieldStatus : Gizmo
        {
            public Building_DefenceShield shield;
            private static readonly Texture2D FullTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
            private static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
            public override float Width
            {
                get
                {
                    return 140f;
                }
            }
            public override GizmoResult GizmoOnGUI(Vector2 topLeft)
            {
                Rect rect = new Rect(topLeft.x, topLeft.y, Width, 75f);
                Widgets.DrawWindowBackground(rect);
                Rect rect2 = GenUI.ContractedBy(rect, 6f);
                Rect rect3 = rect2;
                rect3.height = rect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect3, this.shield.LabelCap);
                Rect rect4 = rect2;
                rect4.yMin = rect.y + rect.height / 2f;
                float num = this.shield.Energy / Mathf.Max(1f, 2f);
                Widgets.FillableBar(rect4, num, Building_DefenceShield.Gizmo_PersonalShieldStatus.FullTex, Building_DefenceShield.Gizmo_PersonalShieldStatus.EmptyTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect4, (this.shield.Energy * 100f).ToString("F0") + " / " + (2f * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
                return new GizmoResult(0);
            }
        }
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.powerComp = base.GetComp<CompPowerTrader>();
        }
        private float EnergyMax
        {
            get
            {
                return 2f;
            }
        }
        private float EnergyGainPerTick
        {
            get
            {
                return (0.12f / 60f);
            }
        }
        public float Energy
        {
            get
            {
                return this.energy;
            }
        }
        public ShieldState ShieldState
        {
            get
            {
                if (this.ticksToReset > 0)
                {
                    return ShieldState.Resetting;
                }
                return ShieldState.Active;
            }
        }
        private bool ShouldDisplay
        {
            get
            {
                return energy > 0;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<float>(ref this.energy, "energy", 0f, false);
            Scribe_Values.LookValue<int>(ref this.ticksToReset, "ticksToReset", -1, false);
            Scribe_Values.LookValue<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
        }

        public override void Tick()
        {
            base.Tick();
            if ((this.powerComp == null || !this.powerComp.PowerOn) && this.energy > 0)
            {
                if (Find.TickManager.TicksGame % 60 == 0)
                {
                    energy -= 0.01f;
                }
            }
            if ((this.powerComp != null && this.powerComp.PowerOn) || this.energy > 0)
            {
                if (this.ShieldState == ShieldState.Resetting)
                {
                    this.ticksToReset--;
                    if (this.ticksToReset <= 0)
                    {
                        this.Reset();
                    }
                }
                else
                {
                    if (this.ShieldState == ShieldState.Active)
                    {
                        float num = Mathf.Lerp(1.2f, 5f, this.energy);
                        Vector3 vector = this.DrawPos;
                        vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                        int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                        if (num2 < 8)
                        {
                            float num3 = (float)(8 - num2) / 8f * 0.05f;
                            vector += this.impactAngleVect * num3;
                            num -= num3;
                        }
                        IEnumerable<IntVec3> enumerable = GenRadial.RadialCellsAround(new IntVec3(0, 0, 0), num, false);
                        foreach (IntVec3 current2 in enumerable)
                        {
                            if (Vectors.VectorSize(current2) >= (double)(num - 1.5f))
                            {
                                this.ProtectSquare(current2 + this.Position);
                            }
                            if (this.ShieldState != ShieldState.Active)
                            {
                                break;
                            }
                        }
                        if (this.powerComp != null && this.powerComp.PowerOn)
                        {
                            this.energy += this.EnergyGainPerTick;
                            if (this.energy > this.EnergyMax)
                            {
                                this.energy = this.EnergyMax;
                            }
                        }
                    }
                }
            }
        }
        private void ProtectSquare(IntVec3 square)
        {
            if (!square.InBounds())
            {
                return;
            }
            List<Thing> list = Find.ThingGrid.ThingsListAt(square);
            List<Thing> list2 = new List<Thing>();
            int i = 0;
            int num = list.Count<Thing>();
            while (i < num)
            {
                if (list[i] != null && list[i] is Projectile)
                {
                    Projectile projectile = (Projectile)list[i];
                    if (!projectile.Destroyed)
                    {
                        bool flag = true;
                        if (flag)
                        {
                            Quaternion exactRotation = projectile.ExactRotation;
                            Vector3 exactPosition = projectile.ExactPosition;
                            exactPosition.y = 0f;
                            Vector3 b = Vectors.IntVecToVec(this.Position);
                            b.y = 0f;
                            Quaternion b2 = Quaternion.LookRotation(exactPosition - b);
                            if (Quaternion.Angle(exactRotation, b2) > 90f)
                            {
                                MoteThrower.ThrowLightningGlow(projectile.ExactPosition, 0.5f);
                                Building_DefenceShield.SoundAbsorbDamage.PlayOneShot(projectile.Position);
                                int damageAmountBase = projectile.def.projectile.damageAmountBase;
                                BodyPartDamageInfo value = new BodyPartDamageInfo(null, null);
                                DamageInfo dinfo = new DamageInfo(projectile.def.projectile.damageDef, damageAmountBase, projectile, projectile.ExactRotation.eulerAngles.y, new BodyPartDamageInfo?(value), null);
                                this.AbsorbedDamage(dinfo);
                                list2.Add(projectile);
                                if (!this.ShouldDisplay)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                i++;
            }
            foreach (Thing current in list2)
            {
                current.Destroy(DestroyMode.Vanish);
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var c in base.GetGizmos())
                yield return c;

            yield return new Building_DefenceShield.Gizmo_PersonalShieldStatus
            {
                shield = this
            };
            yield break;
        }
        public void KeepDisplaying()
        {
            this.lastKeepDisplayTick = Find.TickManager.TicksGame;
        }
        private void AbsorbedDamage(DamageInfo dinfo)
        {
            this.energy -= (float)dinfo.Amount * this.EnergyLossPerDamage;
            if (this.energy < 0f)
            {
                this.Break();
            }
            //Building_OmniShield.SoundAbsorbDamage.PlayOneShot(this.Position);
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = this.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + (float)dinfo.Amount / 10f);
            //MoteThrower.ThrowStatic(loc, ThingDefOf.Mote_ExplosionFlash, num);
            int num2 = (int)num;
            this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
            this.KeepDisplaying();
        }
        private void Break()
        {
            Building_DefenceShield.SoundBreak.PlayOneShot(this.Position);
            MoteThrower.ThrowStatic(this.TrueCenter(), ThingDefOf.Mote_ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                Vector3 loc = this.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
                MoteThrower.ThrowDustPuff(loc, Rand.Range(0.8f, 1.2f));
            }
            this.energy = 0f;
            this.ticksToReset = this.StartingTicksToReset;
        }
        private void Reset()
        {
            Building_DefenceShield.SoundReset.PlayOneShot(this.Position);
            MoteThrower.ThrowLightningGlow(this.TrueCenter(), 3f);
            this.ticksToReset = -1;
            this.energy = this.EnergyOnReset;
        }
        public override void Draw()
        {
            base.Draw();
            if (this.ShieldState == ShieldState.Active && this.ShouldDisplay)
            {
                float num = Mathf.Lerp(1.2f, 9.5f, this.energy);
                Vector3 vector = this.DrawPos;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 9.5f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, Building_DefenceShield.BubbleMat, 0);
            }
            DrawTurret();
        }
    }
}

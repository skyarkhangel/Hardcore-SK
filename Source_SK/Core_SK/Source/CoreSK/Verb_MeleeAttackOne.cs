using System;
using Verse;

namespace RimWorld
{
    public class Verb_MeleeAttackOne : Verb_MeleeAttack
    {
        protected override bool TryCastShot()
        {
            bool flag = base.TryCastShot();
            if (flag)
            {
                this.SelfConsume();
            }
            return true;
        }

        private void SelfConsume()
        {
            if (this.ownerEquipment != null && !this.ownerEquipment.Destroyed)
            {
                this.ownerEquipment.Destroy(DestroyMode.Vanish);
            }
        }
    }
}

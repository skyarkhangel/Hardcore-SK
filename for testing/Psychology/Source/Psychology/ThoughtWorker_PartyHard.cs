using System;
using System.Reflection;
using RimWorld;
using Verse;

namespace Psychology
{
    public class ThoughtWorker_PartyHard : ThoughtWorker
    {
        internal static FieldInfo _setStartPartyASAP;

        private void SetStartPartyASAP(VoluntarilyJoinableLordsStarter _this, bool start)
        {
            if (_setStartPartyASAP == null)
            {
                _setStartPartyASAP = typeof(VoluntarilyJoinableLordsStarter).GetField("setStartPartyASAP", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_setStartPartyASAP == null)
                {
                    Log.ErrorOnce("Unable to reflect VoluntarilyJoinableLordsStarter.setStartPartyASAP!", 305432421);
                }
            }
            _setStartPartyASAP.SetValue(_this, start);
        }

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return ThoughtState.Inactive;
            if (!p.RaceProps.Humanlike)
                return ThoughtState.Inactive;
            if (!p.story.traits.HasTrait(TraitDefOfPsychology.PartyAnimal))
                return ThoughtState.Inactive;
            var LordsStarter = Find.VoluntarilyJoinableLordsStarter;
            if (LordsStarter == null)
                return ThoughtState.Inactive;
            if (Find.TickManager.TicksGame % 2500 == 0)
            {
                if (Rand.MTBEventOccurs(4f, 60000f, 5000f))
                {
                    SetStartPartyASAP(LordsStarter, true);
                }
            }
            return ThoughtState.Inactive;
        }
    }
}

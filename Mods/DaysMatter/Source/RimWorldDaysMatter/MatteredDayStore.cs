using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorldDaysMatter
{
    /*internal class MatteredDayStore : WorldComponent//UtilityWorldObject
    {
        private List<MatteredDay> _matteredDays = new List<MatteredDay>();
        private int _settlement;
        private int _birthdays;
        private int _loversAnniversaries;
        private int _marriageAnniversaries;
        private int _deathDays;


        public List<MatteredDay> MatteredDays { get { return _matteredDays; } set { _matteredDays = value; } }
        public Duration Birthdays { get { return (Duration)_birthdays; } set { _birthdays = (int)value; } }
        public Duration LoversAnniversaries { get { return (Duration)_loversAnniversaries; } set { _loversAnniversaries = (int)value; } }
        public Duration MarriageAnniversaries { get { return (Duration)_marriageAnniversaries; } set { _marriageAnniversaries = (int)value; } }
        public Duration DeathDays { get { return (Duration)_deathDays; } set { _deathDays = (int)value; } }
        public Duration Settlement { get { return (Duration)_settlement; } set { _settlement = (int)value; } }

        public MatteredDayStore(World world) : base(world)
        { }

        public override void ExposeData()
        {
            //base.ExposeData();
            Scribe_Collections.Look(ref _matteredDays, false, "customDays", LookMode.Deep, new object[0]);
            Scribe_Values.Look(ref _birthdays, "birthdays", 0, true);
            Scribe_Values.Look(ref _loversAnniversaries, "loversAnniversaries", 0, true);
            Scribe_Values.Look(ref _marriageAnniversaries, "marriageAnniversaries", 0, true);
            Scribe_Values.Look(ref _deathDays, "deathDays", 0, true);
            Scribe_Values.Look(ref _settlement, "settlement", 0, true);
        }
    }*/
}
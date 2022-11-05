using RimWorld;
using Verse;

namespace RimWorldDaysMatter
{
    public class MatteredDay : IExposable
    {
        private int _quadrum;
        private int _day;
        private string _name;
        private int _duration;

        public Quadrum Quadrum { get { return (Quadrum)_quadrum; } set { _quadrum = (int)value; } }
        public int DayOfQuadrum { get { return _day; } set { _day = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public Duration Duration { get { return (Duration)_duration; } set { _duration = (int)value; } }

        public MatteredDay(string name, Quadrum quadrum, int day, Duration duration)
        {
            Name = name;
            Quadrum = quadrum;
            DayOfQuadrum = day;
            Duration = duration;
        }

        public MatteredDay()
        {
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _quadrum, "RWDM_quadrum", (int)Quadrum.Aprimay, true);
            Scribe_Values.Look(ref _day, "RWDM_day", 1, true);
            Scribe_Values.Look(ref _name, "RWDM_name", "New Day", true);
            Scribe_Values.Look(ref _duration, "RWDM_duration", (int)Duration.None, true);
        }
    }
}
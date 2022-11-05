using Verse;

namespace RimWorldDaysMatter
{
    public static class DurationUtil
    {
        public static string Label(this Duration dura)
        {
            string label;
            switch (dura)
            {
                case Duration.None:
                    label = "DM.Tab.Duration.None".Translate();
                    break;
                case Duration.AllDay:
                    label = "DM.Tab.Duration.AllDay".Translate();
                    break;
                case Duration.Morning:
                    label = "DM.Tab.Duration.Morning".Translate();
                    break;
                case Duration.Noon:
                    label = "DM.Tab.Duration.Noon".Translate();
                    break;
                case Duration.Afternoon:
                    label = "DM.Tab.Duration.Afternoon".Translate();
                    break;
                case Duration.Evening:
                    label = "DM.Tab.Duration.Evening".Translate();
                    break;
                case Duration.Night:
                    label = "DM.Tab.Duration.Night".Translate();
                    break;
                default:
                    label = "N/A";
                    break;
            }
            return label;
        }

        public static int Start(this Duration dura)
        {
            int hour;
            switch (dura)
            {
                case Duration.None:
                    hour = -1;
                    break;
                case Duration.AllDay:
                    hour = 8;
                    break;
                case Duration.Morning:
                    hour = 8;
                    break;
                case Duration.Noon:
                    hour = 11;
                    break;
                case Duration.Afternoon:
                    hour = 14;
                    break;
                case Duration.Evening:
                    hour = 17;
                    break;
                case Duration.Night:
                    hour = 20;
                    break;
                default:
                    hour = -1;
                    break;
            }
            return hour;
        }
    }
}
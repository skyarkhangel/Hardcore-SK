using RimWorld;
using Verse;

namespace RimWorldDaysMatter
{
    internal class DialogRename : Dialog_Rename
    {
        private readonly MatteredDay _matteredDay;
        private readonly string _oldName;

        public DialogRename(MatteredDay matteredDay)
        {
            _matteredDay = matteredDay;
            curName = matteredDay.Name;
            _oldName = curName;
        }

        protected override void SetName(string name)
        {
            _matteredDay.Name = name;
            Messages.Message("renamed " + _oldName + " to " + name, MessageTypeDefOf.SilentInput);
        }
    }
}

//using RimWorld;

//namespace Analyzer.Performance
//{
//    internal class AlertInfo
//    {
//        public bool changed = true;
//        public bool dirty = true;
//        public AlertReport report;

//        public bool Dirty()
//        {
//            if (dirty)
//            {
//                dirty = false;
//                changed = true;
//                return true;
//            }

//            return false;
//        }

//        public void Update(AlertReport report)
//        {
//            this.report = report;
//            changed = false;
//        }
//    }
//}
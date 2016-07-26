using RimWorld;
using UnityEngine;
using Verse;

namespace Fluffy
{
    public enum FilterType
    {
        True,
        False,
        None
    }

    public interface IFilter
    {
        string label { get; }

        FilterType state { get; set; }

        bool filter(Pawn p);

        void bump();

        Texture2D[] textures { get; }
    }

    public class Filter
    {
        public virtual FilterType state { get; set; }

        public virtual bool filter(Pawn p)
        {
            return true;
        }

        public virtual void bump()
        {
            int next = (int)state + 1;
            if (next > 2)
            {
                state = FilterType.True;
            }
            else if (next == 1)
            {
                state = FilterType.False;
            }
            else
            {
                state = FilterType.None;
            }
            Widgets_Filter.Filter = true;
            Widgets_Filter.FilterPossible = true;
        }
    }

    public class Filter_Gender : Filter, IFilter
    {
        public string label => "gender";

        public override FilterType state { get; set; } = FilterType.None;

        public override bool filter(Pawn p)
        {
            if (state == FilterType.None) return true;
            if (state == FilterType.False && p.gender == Gender.Male) return true;
            if (state == FilterType.True && p.gender == Gender.Female) return true;
            return false;
        }

        public Texture2D[] textures
        {
            get
            {
                return new[]
                {
                    ContentFinder<Texture2D>.Get("UI/Gender/female"),
                    ContentFinder<Texture2D>.Get("UI/Gender/male"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/all_large")
                };
            }
        }
    }

    public class Filter_Reproductive : Filter, IFilter
    {
        public string label => "reproductive";

        public override FilterType state { get; set; } = FilterType.None;

        public override bool filter(Pawn p)
        {
            bool repro = p.ageTracker.CurLifeStage.reproductive;
            if (state == FilterType.None) return true;
            if (state == FilterType.True && repro) return true;
            if (state == FilterType.False && !repro) return true;
            return false;
        }

        public Texture2D[] textures
        {
            get
            {
                return new[]
                {
                    ContentFinder<Texture2D>.Get("UI/FilterStates/reproductive_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/not_reproductive_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/all_large")
                };
            }
        }
    }

    public class Filter_Training : Filter, IFilter
    {
        public string label => "training";


        public override FilterType state { get; set; } = FilterType.None;

        public override bool filter(Pawn p)
        {
            bool tamed = p.training.IsCompleted(TrainableDefOf.Obedience);
            if (state == FilterType.None) return true;
            if (state == FilterType.True && tamed) return true;
            if (state == FilterType.False && !tamed) return true;
            return false;
        }

        public Texture2D[] textures
        {
            get
            {
                return new[]
                {
                    ContentFinder<Texture2D>.Get("UI/FilterStates/obedience_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/not_obedience_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/all_large")
                };
            }
        }
    }


    public class Filter_Milkable : Filter, IFilter
    {
        public string label => "milkable";

        public override FilterType state { get; set; } = FilterType.None;

        public override bool filter(Pawn p)
        {
            bool milkable = p.ageTracker.CurLifeStage.milkable && p.GetComp<CompMilkable>() != null && p.gender == Gender.Female;
            if (state == FilterType.None) return true;
            if (state == FilterType.True && milkable) return true;
            if (state == FilterType.False && !milkable) return true;
            return false;
        }

        public Texture2D[] textures
        {
            get
            {
                return new[]
                {
                    ContentFinder<Texture2D>.Get("UI/FilterStates/milkable_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/not_milkable_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/all_large")
                };
            }
        }
    }

    public class Filter_Shearable : Filter, IFilter
    {
        public string label => "shearable";

        public override FilterType state { get; set; } = FilterType.None;

        public override bool filter(Pawn p)
        {
            bool shearable = p.ageTracker.CurLifeStage.shearable && p.GetComp<CompShearable>() != null;
            if (state == FilterType.None) return true;
            if (state == FilterType.True && shearable) return true;
            if (state == FilterType.False && !shearable) return true;
            return false;
        }
        
        public Texture2D[] textures
        {
            get
            {
                return new[]
                {
                    ContentFinder<Texture2D>.Get("UI/FilterStates/shearable_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/not_shearable_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/all_large")
                };
            }
        }
    }

    public class Filter_Pregnant : Filter, IFilter
    {
        public string label => "pregnant";

        public override FilterType state { get; set; } = FilterType.None;

        public override bool filter(Pawn p)
        {
            Hediff diff = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant);
            bool pregnant = diff != null && diff.Visible;
            if (state == FilterType.None) return true;
            if (state == FilterType.True && pregnant) return true;
            if (state == FilterType.False && !pregnant) return true;
            return false;
        }

        public Texture2D[] textures
        {
            get
            {
                return new[]
                {
                    ContentFinder<Texture2D>.Get("UI/FilterStates/pregnant_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/not_pregnant_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/all_large")
                };
            }
        }
    }

    public class Filter_Old : Filter, IFilter
    {
        public string label => "old";

        public override FilterType state { get; set; } = FilterType.None;

        public override bool filter(Pawn p)
        {
            bool old = p.ageTracker.AgeBiologicalYearsFloat > p.RaceProps.lifeExpectancy * .9;
            if (state == FilterType.None) return true;
            if (state == FilterType.True && old) return true;
            if (state == FilterType.False && !old) return true;
            return false;
        }

        public Texture2D[] textures
        {
            get
            {
                return new[]
                {
                    ContentFinder<Texture2D>.Get("UI/FilterStates/old_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/not_old_large"),
                    ContentFinder<Texture2D>.Get("UI/FilterStates/all_large")
                };
            }
        }
    }
}
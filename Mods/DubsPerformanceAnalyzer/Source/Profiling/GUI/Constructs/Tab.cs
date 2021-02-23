using System;
using System.Collections.Generic;

namespace Analyzer.Profiling
{
    public class Tab
    {
        public Action onClick;
        public Func<bool> onSelect;
        public bool Selected => onSelect?.Invoke() ?? false;

        public Category category;
        public Func<string> label;
        public Func<string> tip;
        
        public string Label => label();
        public string Tip => tip();
        public bool collapsed = false;

        public Dictionary<Entry, Type> entries = new Dictionary<Entry, Type>();

        public Tab(Func<string> label, Action onClick, Func<bool> onSelect, Category category, Func<string> tip)
        {
            this.label = label;
            this.onClick = onClick;
            this.onSelect = onSelect;
            this.category = category;
            this.tip = tip;
        }
    }
}
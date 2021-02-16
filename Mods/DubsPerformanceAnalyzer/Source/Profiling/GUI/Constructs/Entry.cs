using Analyzer;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{
    [AttributeUsage(AttributeTargets.Field)]
    public class Setting : Attribute
    {
        public string name;
        public string tip;
        public Setting(string name, string tip = null)
        {
            this.name = name;
            this.tip = tip;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Entry : Attribute
    {
        // static list for runtime generated
        public static List<Entry> entries = new List<Entry>();

        public string name;
        public string tip;
        public Dictionary<FieldInfo, Setting> Settings = new Dictionary<FieldInfo, Setting>();

        public Category category;
        public Type type;

        public MethodInfo onMouseOver;
        public MethodInfo onSelect;
        public MethodInfo onClick;
        public MethodInfo checkBox;

        public bool isActive = false;
        public bool isLoading = false;
        public bool isPatched = false;
        public bool isClosable = false;

        public void SetActive(bool value)
        {
            if (type != null) // Active must be static here.
                AccessTools.Field(type, "Active")?.SetValue(null, value);

            isActive = value;
        }

        public static Entry Create(string name, Category category, Type type, bool closeable, bool dynGen = false)
        {
            Entry entry = entries.FirstOrDefault(
                x => x.name == name
                && x.category == category
                && x.type == type);

            if (entry != null) return entry;

            entry = new Entry(name, category, dynGen);
            // get rid of the untranslated garbage
            if (dynGen)
            {
                entry.name = name;
                entry.tip = name;
            }
            entry.type = type;
            entry.isClosable = closeable;
            entries.Add(entry);

            return entry;
        }

        public Entry(string name, Category category, bool dyGen = false)
        {
            this.category = category;
            if (dyGen) return;
            this.name = name?.TranslateSimple();
            this.tip = (name + ".tooltip")?.TranslateSimple();
        }

        public Profiler Start(string key, MethodBase info)
        {
            return ProfileController.Start(key, null, null, null, null, info);
        }

        public void PatchMethods()
        {
            if (!isPatched && !isLoading)
            {
                isLoading = true;
                Analyzer.PatchEntry(this);
            }
        }
    }
}
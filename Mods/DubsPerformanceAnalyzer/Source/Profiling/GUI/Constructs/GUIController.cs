using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Analyzer.Profiling
{
    public enum Category { Settings, Tick, Update, GUI, Modder }

    public static class GUIController
    {
        private static Tab currentTab;
        private static Entry currentEntry;
        private static Profiler currentProfiler;
        private static Category currentCategory = Category.Settings;

        private static Dictionary<Category, Tab> tabs;
        public static Dictionary<string, Type> types = new Dictionary<string, Type>();

        public static Profiler CurrentProfiler { get { return currentProfiler; } set { currentProfiler = value; } }
        public static Tab GetCurrentTab => currentTab;
        public static Category CurrentCategory => currentCategory;
        public static Entry CurrentEntry => currentEntry;

        public static IEnumerable<Tab> Tabs => tabs.Values;
        public static Tab Tab(Category cat) => tabs[cat];
        public static Entry EntryByName(string name) => Tabs.Where(t => t.entries.Keys.Any(e => e.name == name)).First().entries.First(e => e.Key.name == name).Key;

        public static void InitialiseTabs()
        {
            tabs = new Dictionary<Category, Tab>();

            addTab(() => ResourceCache.Strings.tab_setting, () => ResourceCache.Strings.tab_setting_desc, Category.Settings);
            addTab(() => ResourceCache.Strings.tab_tick, () => ResourceCache.Strings.tab_tick_desc, Category.Tick);
            addTab(() => ResourceCache.Strings.tab_update, () => ResourceCache.Strings.tab_update_desc, Category.Update);
            addTab(() => ResourceCache.Strings.tab_gui, () => ResourceCache.Strings.tab_gui_desc, Category.GUI);
            addTab(() => ResourceCache.Strings.tab_modder, () => ResourceCache.Strings.tab_modder_desc, Category.Modder);

            void addTab(Func<string> name, Func<string> desc, Category cat)
            {
                tabs.Add(cat, new Tab(name, 
                    () =>
                    {
                        currentCategory = cat;
                        if (currentEntry != null)
                        {
                            currentEntry.SetActive(false);
                            currentEntry = null;
                            ResetProfilers();
                        }
                    }, 
                    () => currentCategory == cat, cat, desc));
            }
        }

        public static void ClearEntries()
        {
            var entriesToRemove = new List<string>();
            foreach (var entry in tabs.Values.SelectMany(tab => tab.entries.Keys))
            {
                if (entry.isClosable) entriesToRemove.Add(entry.name);
                entry.isPatched = false;
            }

            foreach(var entry in entriesToRemove)
                RemoveEntry(entry);
        }

        public static void ResetToSettings()
        {
            if (currentEntry != null)
            {
                currentEntry.SetActive(false);
                ResetProfilers();
            }

            currentTab = Tab(Category.Settings);
            currentCategory = Category.Settings;
        }

        public static void ResetProfilers()
        {
            ProfileController.Profiles.Clear();
            Analyzer.RefreshLogCount();
            currentProfiler = null;
        }

        public static void SwapToEntry(string entryName)
        {
            if (currentEntry != null)
            {
                currentEntry.SetActive(false);
                ResetProfilers();
            }

            currentEntry = EntryByName(entryName);

            if (!currentEntry.isPatched)
            {
                currentEntry.PatchMethods();
            }

            currentEntry.SetActive(!Analyzer.CurrentlyPaused);
            currentCategory = currentEntry.category;
            currentTab = Tab(currentCategory);
        }

        public static void AddEntry(string name, Category category)
        {
            Type myType = null;
            if (types.ContainsKey(name))
            {
                myType = types[name];
            }
            else
            {
                myType = DynamicTypeBuilder.CreateType(name, null);
                types.Add(name, myType);
            }

#if DEBUG
            ThreadSafeLogger.Message($"Adding entry {name} into the category {category}");
#endif
            var entry = Entry.Create(name, category, myType, true, true);

            if (Tab(category).entries.ContainsKey(entry))
            {
                ThreadSafeLogger.Error($"Attempting to re-add entry {name} into the category {category}");
            }
            else
            {
                Tab(category).entries.Add(entry, myType);
            }
        }

        public static void RemoveEntry(string name)
        {
            var entry = EntryByName(name);
            entry.isPatched = false;
            entry.SetActive(false);

            Tab(entry.category).entries.Remove(entry);

#if DEBUG
            ThreadSafeLogger.Message($"Removing entry {name} from the category {entry.category.ToString()}");
#endif
        }
    }
}

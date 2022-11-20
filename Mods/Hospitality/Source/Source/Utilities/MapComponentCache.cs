using System;
using JetBrains.Annotations;
using Verse;

namespace Hospitality
{
    public static class MapComponentCache
    {
        [NotNull]private static Hospitality_MapComponent[] cachedComponents = new Hospitality_MapComponent[12];

        [CanBeNull]
        public static Hospitality_MapComponent GetMapComponent([CanBeNull]this Map map)
        {
            return map == null ? null : cachedComponents[map.Index];
        }

        [CanBeNull]
        public static Hospitality_MapComponent GetMapComponent([CanBeNull]this Thing thing)
        {
            if (thing == null || thing.mapIndexOrState < 0) return null;
            return cachedComponents[thing.mapIndexOrState];
        }

        public static void Register([NotNull]Hospitality_MapComponent component)
        {
            if (cachedComponents.Length < Find.Maps.Count)
            {
                Array.Resize(ref cachedComponents, Find.Maps.Count + 6); // This does Array.Copy for us.
            }   

            cachedComponents[component.map.Index] = component;
        }
    }
}

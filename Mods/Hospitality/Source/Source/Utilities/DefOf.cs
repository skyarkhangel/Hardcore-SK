using System;
using Verse;
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace Hospitality
{
    [RimWorld.DefOf]
    public static class DefOf
    {
        // This exists is to address this issue: https://github.com/OrionFive/Hospitality/issues/668
        // The mod doing this is "Dormitories".
        [Obsolete("Added back in because someone is referencing this from their mod. To whatever mod referencing this, you're **doing it wrong**. Please look into the MayRequire attribute to find out how to do def references correctly.")]
        public static RoomRoleDef GuestRoom;
    }
}
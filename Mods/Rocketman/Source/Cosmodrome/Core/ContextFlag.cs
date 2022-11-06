using System;
namespace RocketMan
{
    public enum ContextFlag
    {
        /// <summary>
        /// Indicate that the current context is unknown. (outside both ticking and UI)
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Indicate that the current context is within the TickManager.DoSingleTick
        /// </summary>
        Ticking = 1,

        /// <summary>
        /// Indicate that the current context is UI and outside the TickManager.DoSingleTick
        /// </summary>
        Updating = 2,
    }
}

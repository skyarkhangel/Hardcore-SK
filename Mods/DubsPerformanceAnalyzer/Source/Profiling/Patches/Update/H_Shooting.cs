using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.update.shooting", Category.Update)]
    public static class H_Shooting
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            foreach (var m in Utility.GetTypeMethods(typeof(Verb_LaunchProjectile)))
                yield return m;

            foreach (var m in Utility.GetTypeMethods(typeof(Projectile)))
                yield return m;

            foreach (var m in Utility.GetTypeMethods(typeof(CompProjectileInterceptor)))
                yield return m;

            foreach (var m in Utility.GetTypeMethods(typeof(Building_Turret)))
                yield return m;
        }
    }
}

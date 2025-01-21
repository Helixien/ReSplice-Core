using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace ReSpliceCore
{
    [HarmonyPatch]
    public static class Building_GeneExtractor_SelectionWeight_Patch
    {
        public static MethodBase TargetMethod()
        {
            var nestedClasses = typeof(Building_GeneExtractor).GetNestedTypes(AccessTools.all);
            foreach (var nested in nestedClasses)
            {
                var methods = nested.GetMethods(AccessTools.all);
                foreach (var method in methods)
                {
                    if (method.Name.Contains("SelectionWeight"))
                    {
                        return method;
                    }
                }
            }
            return null;
        }

        public static void Postfix(ref float __result, Gene g)
        {
            if (g.def.IsDarkArchite())
            {
                __result = 0f;
            }
        }
    }
}

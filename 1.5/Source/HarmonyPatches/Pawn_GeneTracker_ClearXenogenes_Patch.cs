using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ReSpliceCore
{
    [HarmonyPatch(typeof(Pawn_GeneTracker), "ClearXenogenes")]
    public static class Pawn_GeneTracker_ClearXenogenes_Patch
    {
        public static void Prefix(Pawn_GeneTracker __instance, out List<Gene> __state)
        {
            __state = __instance.xenogenes.Where(x => x.def.IsDarkArchite()).ToList();
            foreach (var gene in __state)
            {
                __instance.xenogenes.Remove(gene);
            }
        }

        public static void Postfix(Pawn_GeneTracker __instance, List<Gene> __state)
        {
            foreach (var gene in __state)
            {
                __instance.xenogenes.Add(gene);
            }
        }
    }
}

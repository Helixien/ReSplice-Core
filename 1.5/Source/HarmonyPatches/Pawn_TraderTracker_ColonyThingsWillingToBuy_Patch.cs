using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ReSpliceCore
{
    [HarmonyPatch(typeof(Pawn_TraderTracker), "ColonyThingsWillingToBuy")]
    public static class Pawn_TraderTracker_ColonyThingsWillingToBuy_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> result, Pawn_TraderTracker __instance)
        {
            foreach (var thing in result)
            {
                yield return thing;
            }
            foreach (var genebank in RS_Utils.allGeneBanks.Where(x => x != ThingDefOf.GeneBank))
            {
                foreach (var geneVault in __instance.pawn.Map.listerBuildings.AllBuildingsColonistOfDef(genebank))
                {
                    if (__instance.ReachableForTrade(geneVault))
                    {
                        var comp = geneVault.TryGetComp<CompGenepackContainer>();
                        foreach (var genepack in comp.ContainedGenepacks)
                        {
                            if (result.Contains(genepack) is false)
                            {
                                yield return genepack;
                            }
                        }
                    }
                }
            }
        }
    }
}

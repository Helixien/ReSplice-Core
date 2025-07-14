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
            HashSet<Thing> yieldedThings = new HashSet<Thing>();
            foreach (var thing in result)
            {
                yieldedThings.Add(thing);
                yield return thing;
            }
            foreach (var genebank in RS_Utils.allGeneBanks.Where(x => x != ThingDefOf.GeneBank).ToList())
            {
                foreach (var geneVault in __instance.pawn.Map.listerBuildings.AllBuildingsColonistOfDef(genebank).ToList())
                {
                    if (__instance.ReachableForTrade(geneVault))
                    {
                        var comp = geneVault.TryGetComp<CompGenepackContainer>();
                        foreach (var genepack in comp.ContainedGenepacks.ToList())
                        {
                            if (yieldedThings.Contains(genepack) is false)
                            {
                                yieldedThings.Add(genepack);
                                yield return genepack;
                            }
                        }
                    }
                }
            }
        }
    }
}

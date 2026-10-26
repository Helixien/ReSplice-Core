using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ReSpliceCore
{
    [HarmonyPatch(typeof(TradeUtility), "AllLaunchableThingsForTrade")]
    public static class TradeUtility_AllLaunchableThingsForTrade_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> result, Map map, ITrader trader = null)
        {
            foreach (Thing thing in result)
            {
                yield return thing;
            }
            HashSet<Thing> banks = [];
            foreach (Building_OrbitalTradeBeacon item in Building_OrbitalTradeBeacon.AllPowered(map))
            {
                foreach (IntVec3 tradeableCell in item.TradeableCells)
                {
                    List<Thing> thingList = tradeableCell.GetThingList(map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        Thing t = thingList[i];
                        if (t.def != ThingDefOf.GeneBank && RS_Utils.allGeneBanks.Contains(t.def))
                        {
                            banks.Add(t);
                        }
                    }
                }
            }
            foreach (var bank in banks)
            {
                var comp = bank.TryGetComp<CompGenepackContainer>();
                foreach (var genepack in comp.ContainedGenepacks.ToList())
                {
                    yield return genepack;
                }
            }
        }
    }
}

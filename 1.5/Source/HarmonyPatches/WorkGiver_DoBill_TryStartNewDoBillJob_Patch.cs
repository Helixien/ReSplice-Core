using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReSpliceCore
{
    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryStartNewDoBillJob")]
    public static class WorkGiver_DoBill_TryStartNewDoBillJob_Patch
    {
        public static void Postfix(Job __result, Bill bill)
        {
            if (__result?.def == JobDefOf.DoBill)
            {
                if (bill is Bill_InfuseDarkArchite infuse)
                {
                    __result.targetQueueB.Add(infuse.spike);
                    __result.countQueue.Add(1);
                }
            }
        }
    }
}

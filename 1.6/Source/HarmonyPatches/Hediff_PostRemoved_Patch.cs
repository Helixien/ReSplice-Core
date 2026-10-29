using HarmonyLib;
using Verse;

namespace ReSpliceCore
{
    [HarmonyPatch(typeof(Hediff), "PostRemoved")]
    public static class Hediff_PostRemoved_Patch
    {
        public static void Postfix(Hediff __instance)
        {
            var extension = __instance.def.GetModExtension<GeneExtension>();
            if (extension is null)
            {
                return;
            }
            if (__instance.pawn.RaceProps.Humanlike && __instance.pawn.health.hediffSet.hediffs.Any((Hediff a) => a.def == __instance.def) is false)
            {
                extension.TryRemoveXenogene(__instance.pawn);
                extension.TryRemoveEndogene(__instance.pawn);
            }
        }
    }
}

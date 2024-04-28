using HarmonyLib;
using Verse;

namespace ReSpliceCore
{
    [HarmonyPatch(typeof(HediffSet), "AddDirect")]
    public static class HediffSet_AddDirect_Patch
    {
        public static void Postfix(HediffSet __instance, Pawn ___pawn, Hediff hediff)
        {
            if (__instance.pawn.RaceProps.Humanlike && __instance.hediffs.Any((Hediff a) => a.def == hediff.def))
            {
                var extension = hediff.def.GetModExtension<GeneExtension>();
                if (extension != null)
                {
                    extension.TryAddXenogene(___pawn);
                    extension.TryAddEndogene(___pawn);
                }
            }
        }
    }
}

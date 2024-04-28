using HarmonyLib;
using RimWorld;
using Verse;

namespace ReSpliceCore
{
    [HarmonyPatch(typeof(Pawn_AbilityTracker), "RemoveAbility")]
    public static class Pawn_AbilityTracker_RemoveAbility_Patch
    {
        public static void Postfix(Pawn_AbilityTracker __instance, AbilityDef def)
        {
            var pawn = __instance.pawn;
            if (pawn.RaceProps.Humanlike && pawn.abilities.abilities.Any((Ability a) => a.def == def) is false)
            {
                var extension = def.GetModExtension<GeneExtension>();
                if (extension != null)
                {
                    extension.TryRemoveXenogene(__instance.pawn);
                    extension.TryRemoveEndogene(__instance.pawn);
                }
            }
        }
    }
}

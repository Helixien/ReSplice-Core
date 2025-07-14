using HarmonyLib;
using RimWorld;
using Verse;

namespace ReSpliceCore
{
    [HarmonyPatch(typeof(Pawn_AbilityTracker), "GainAbility")]
    public static class Pawn_AbilityTracker_GainAbility_Patch
    {
        public static void Postfix(Pawn_AbilityTracker __instance, AbilityDef def)
        {
            if (__instance.pawn.RaceProps.Humanlike && __instance.abilities.Any((Ability a) => a.def == def))
            {
                var extension = def.GetModExtension<GeneExtension>();
                if (extension != null)
                {
                    extension.TryAddXenogene(__instance.pawn);
                    extension.TryAddEndogene(__instance.pawn);
                }
            }
        }
    }
}

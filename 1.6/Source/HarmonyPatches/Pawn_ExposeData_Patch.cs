using HarmonyLib;
using Verse;

namespace ReSpliceCore
{
    [HarmonyPatch(typeof(Pawn), "ExposeData")]
    public static class Pawn_ExposeData_Patch
    {
        public static void Postfix(Pawn __instance)
        {
            if (Scribe.mode == LoadSaveMode.PostLoadInit && __instance.RaceProps.Humanlike)
            {
                foreach (var ability in __instance.abilities.abilities)
                {
                    var extension = ability.def.GetModExtension<GeneExtension>();
                    if (extension != null)
                    {
                        extension.TryAddXenogene(__instance);
                        extension.TryAddEndogene(__instance);
                    }
                }

                foreach (var hediff in __instance.health.hediffSet.hediffs)
                {
                    var extension = hediff.def.GetModExtension<GeneExtension>();
                    if (extension != null)
                    {
                        extension.TryAddXenogene(__instance);
                        extension.TryAddEndogene(__instance);
                    }
                }
            }
        }
    }
}

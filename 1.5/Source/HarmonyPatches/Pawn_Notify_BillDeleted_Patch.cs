using HarmonyLib;
using RimWorld;
using Verse;

namespace ReSpliceCore
{
    [HarmonyPatch(typeof(Pawn), "Notify_BillDeleted")]
    public static class Pawn_Notify_BillDeleted_Patch
    {
        public static void Postfix(Bill bill)
        {
            if (bill is Bill_InfuseDarkArchite infuse)
            {
                infuse.spike?.Notify_BillRemoved();
            }
        }
    }
}

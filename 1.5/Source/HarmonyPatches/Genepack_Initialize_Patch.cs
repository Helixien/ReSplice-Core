using HarmonyLib;
using RimWorld;
using Verse;

namespace ReSpliceCore
{
    [HarmonyPatch(typeof(Genepack), "Initialize")]
    public static class Genepack_Initialize_Patch
    {
        private const string TexPath = "Things/Item/Special/Genepack/GenepackCapsule_DarkArchite";

        public static void Postfix(Genepack __instance)
        {
            __instance.TryAssignDarkArchiteGraphic();
        }

        public static void TryAssignDarkArchiteGraphic(this Genepack genepack)
        {
            if (ModsConfig.AnomalyActive)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    bool hasDarkArchiteGenes = genepack.GeneSet.genes.Any(x => x.IsDarkArchite());
                    bool hasDarkArchiteGraphic = genepack.graphicInt?.path == TexPath;
                    if (hasDarkArchiteGraphic is false && hasDarkArchiteGenes)
                    {
                        var copy = new GraphicData();
                        copy.CopyFrom(genepack.def.graphicData);
                        copy.texPath = TexPath;
                        copy.graphicClass = typeof(Graphic_Single);
                        genepack.graphicInt = copy.GraphicColoredFor(genepack);
                        genepack.Map?.mapDrawer.MapMeshDirty(genepack.Position, MapMeshFlagDefOf.Things);
                    }
                    else if (hasDarkArchiteGraphic && hasDarkArchiteGenes is false)
                    {
                        genepack.graphicInt = null;
                        genepack.Map?.mapDrawer.MapMeshDirty(genepack.Position, MapMeshFlagDefOf.Things);
                    }
                });
            }
        }
    }


    [HarmonyPatch(typeof(Genepack), "ExposeData")]
    public static class Genepack_ExposeData_Patch
    {
        public static void Postfix(Genepack __instance)
        {
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                __instance.TryAssignDarkArchiteGraphic();
            }
        }
    }

    [HarmonyPatch(typeof(Genepack), "TickRare")]
    public static class Genepack_TickRare_Patch
    {
        public static void Postfix(Genepack __instance)
        {
            __instance.TryAssignDarkArchiteGraphic();
        }
    }

    [HarmonyPatch(typeof(Genepack), "PostMake")]
    public static class Genepack_PostMake_Patch
    {
        public static void Postfix(Genepack __instance)
        {
            __instance.TryAssignDarkArchiteGraphic();
        }
    }
}

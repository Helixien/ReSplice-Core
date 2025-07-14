using RimWorld;
using Verse;

namespace ReSpliceCore
{
    [DefOf]
    public static class RS_DefOf
    {
        public static SoundDef RS_GeneCentrifuge_Ambience, RS_XenoGermDuplicator_Ambience;
        public static JobDef RS_InsertThingIntoProcessor;
        public static ThingDef RS_GeneCentrifuge, RS_XenoGermDuplicator;

        // Anomaly content
        [MayRequireAnomaly] public static ThingDef RS_DarkArchiteSpike;
        [MayRequireAnomaly] public static GeneCategoryDef RS_DarkArchite;
        [MayRequireAnomaly] public static HediffDef RS_InfusionComa;
        [MayRequireAnomaly] public static RecipeDef RS_InfuseDarkArchite;

    }
}

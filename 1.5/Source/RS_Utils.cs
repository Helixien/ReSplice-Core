using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ReSpliceCore
{
    [StaticConstructorOnStartup]
    public static class RS_Utils
    {
        public static List<ThingDef> allGenepacks = new List<ThingDef>();
        public static List<ThingDef> allGeneBanks = new List<ThingDef>();
        static RS_Utils()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.thingClass != null && typeof(Genepack).IsAssignableFrom(def.thingClass))
                {
                    allGenepacks.Add(def);
                }
                if (def.HasComp(typeof(CompGenepackContainer)))
                {
                    allGeneBanks.Add(def);
                }
            }
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}

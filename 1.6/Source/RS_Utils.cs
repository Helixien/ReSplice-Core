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

        public static IEnumerable<GeneDef> AllDarkArchiteGenes()
        {
            return DefDatabase<GeneDef>.AllDefsListForReading.Where(IsDarkArchite);
        }

        public static bool IsDarkArchite(this GeneDef x)
        {
            return x != null && x.displayCategory == RS_DefOf.RS_DarkArchite;
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static List<Genepack> GetAllGenepacks(Map map, int minGeneCount = 0)
        {
            var allPacks = new List<Genepack>();
            foreach (var def in allGenepacks)
            {
                allPacks.AddRange(map.listerThings.ThingsOfDef(def).Cast<Genepack>()
                    .Where(x => x.GeneSet.GenesListForReading.Count >= minGeneCount));
            }
            foreach (var def in allGeneBanks)
            {
                var banks = map.listerThings.ThingsOfDef(def);
                foreach (var bank in banks)
                {
                    var comp = bank.TryGetComp<CompGenepackContainer>();
                    if (comp != null)
                    {
                        allPacks.AddRange(comp.ContainedGenepacks.Where(x => x.GeneSet.GenesListForReading.Count >= minGeneCount));
                    }
                }
            }
            return allPacks.Distinct().ToList();
        }
    }
}

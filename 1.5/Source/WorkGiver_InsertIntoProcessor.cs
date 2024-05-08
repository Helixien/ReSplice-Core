using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ReSpliceCore
{
    public class WorkGiver_InsertXenogerm : WorkGiver_InsertIntoProcessor
    {
        public override IEnumerable<ThingDef> TargetThings => Gen.YieldSingle(ThingDefOf.Xenogerm);
        public override ThingDef ProcessorDef => RS_DefOf.RS_XenoGermDuplicator;
    }

    public class WorkGiver_InsertGenepack : WorkGiver_InsertIntoProcessor
    {
        public override IEnumerable<ThingDef> TargetThings => RS_Utils.allGenepacks;
        public override ThingDef ProcessorDef => RS_DefOf.RS_GeneCentrifuge;
    }

    [HotSwappable]
    public abstract class WorkGiver_InsertIntoProcessor : WorkGiver_Scanner
    {
        public abstract IEnumerable<ThingDef> TargetThings { get; }
        public abstract ThingDef ProcessorDef { get; }
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var things = new List<Thing>();
            var genebanks = RS_Utils.allGeneBanks.SelectMany(x => pawn.Map.listerThings.ThingsOfDef(x)).Where(x => pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly)).ToList();
            foreach (var thing in TargetThings)
            {
                things.AddRange(pawn.Map.listerThings.ThingsOfDef(thing)
                .Where(x => pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly)));
            }
            foreach (var thing in genebanks)
            {
                if (pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Deadly))
                {
                    things.Add(thing);
                }
            }
            return things;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return GetProcessors(pawn, t, out _).Any();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var processor = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, 
                GetProcessors(pawn, t, out var toBeCarried),
                PathEndMode.Touch, TraverseParms.For(pawn));
            var job = JobMaker.MakeJob(RS_DefOf.RS_InsertThingIntoProcessor, t, processor, toBeCarried);
            job.count = 1;
            return job;
        }

        private IEnumerable<Building_Processor> GetProcessors(Pawn hauler, Thing targetThing, out Thing toBeCarried)
        {
            toBeCarried = targetThing;
            var comp = targetThing.TryGetComp<CompGenepackContainer>();
            if (comp != null)
            {
                toBeCarried = GetGene(hauler, comp);
            }
            var temp = toBeCarried;
            var storages = hauler.Map.listerThings.ThingsOfDef(ProcessorDef).Cast<Building_Processor>()
            .Where(x => x.Accepts(temp) && hauler.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly)).ToList();
            return storages;
        }

        public Thing GetGene(Pawn hauler, CompGenepackContainer comp)
        {
            foreach (var gene in comp.ContainedGenepacks)
            {
                var storages = hauler.Map.listerThings.ThingsOfDef(ProcessorDef).Cast<Building_Processor>()
                    .Where(x => x.Accepts(gene) && hauler.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly)).ToList();
                if (storages.Any())
                {
                    return gene;
                }
            }
            return null;
        }
    }
}

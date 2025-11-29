using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ReSpliceCore
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class Building_GeneCentrifuge : Building_Processor
    {
        public static Texture2D InsertGenePack = ContentFinder<Texture2D>.Get("UI/Gizmos/InsertGenePack");
        public static Texture2D CombineIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/MergeGenePack");

        public Genepack genepackToStore;
        public Genepack firstGenepackToCombine;
        public Genepack secondGenepackToCombine;

        public GeneDef geneToSeparate;
        
        public bool IsSeparating => genepackToStore != null && innerContainer.Contains(genepackToStore);
        
        public bool IsCombining => firstGenepackToCombine != null && secondGenepackToCombine != null && innerContainer.Contains(firstGenepackToCombine) && innerContainer.Contains(secondGenepackToCombine);

        public override SoundDef SustainerDef => RS_DefOf.RS_GeneCentrifuge_Ambience;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (Faction == Faction.OfPlayer)
            {
                var separateGene = new Command_Action
                {
                    defaultLabel = "RS.SeparateGene".Translate(),
                    defaultDesc = "RS.SeparateGeneDesc".Translate(),
                    icon = InsertGenePack,
                    action = delegate
                    {
                        var allGenePacks = RS_Utils.GetAllGenepacks(this.Map, 1);
                        var floatList = new List<FloatMenuOption>();
                        foreach (var genepack in allGenePacks)
                        {
                            floatList.Add(new FloatMenuOption(genepack.LabelCap, delegate
                            {
                                if (genepack.GeneSet.GenesListForReading.Count <= 2)
                                    SelectGenepackToProcess(genepack, genepack.GeneSet.GenesListForReading.FirstOrDefault());
                                else
                                    Find.WindowStack.Add(new Window_SeparateGene(this, genepack));
                            }));
                        }
                        if (floatList.Any() is false)
                        {
                            floatList.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
                        }
                        Find.WindowStack.Add(new FloatMenu(floatList));
                    }
                };
                if (IsSeparating)
                {
                    separateGene.Disable("RS.CentrigureWorking".Translate());
                }
                if (Powered is false)
                {
                    separateGene.Disable("NoPower".Translate());
                }
                yield return separateGene;

                if (IsSeparating)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "RS.CancelGeneSeparating".Translate(),
                        defaultDesc = "RS.CancelGeneSeparatingDesc".Translate(),
                        activateSound = SoundDefOf.Tick_Tiny,
                        icon = CancelIcon,
                        action = delegate ()
                        {
                            JobCleanup();
                            if (IsSeparating)
                            {
                                this.EjectContents();
                            }
                        }
                    };
                }

                if (Prefs.DevMode && IsSeparating)
                {
                    Command_Action command_Action = new Command_Action
                    {
                        defaultLabel = "Debug: Finish separation",
                        action = delegate
                        {
                            ticksDone = ExtractionDuration(genepackToStore);
                            FinishJob();
                        }
                    };
                    yield return command_Action;
                }

                var combineGene = new Command_Action
                {
                    defaultLabel = "RS.CombineGenepacks".Translate(),
                    defaultDesc = "RS.CombineGenepacksDesc".Translate(),
                    icon = CombineIcon,
                    action = delegate
                    {
                        Find.WindowStack.Add(new Window_CombineGene(this));
                    }
                };

                if (IsSeparating)
                {
                    combineGene.Disable("RS.CentrigureWorking".Translate());
                }
                if (Powered is false)
                {
                    combineGene.Disable("NoPower".Translate());
                }
                yield return combineGene;

                if (firstGenepackToCombine != null || secondGenepackToCombine != null || IsCombining)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "RS.CancelGeneCombining".Translate(),
                        defaultDesc = "RS.CancelGeneCombiningDesc".Translate(),
                        activateSound = SoundDefOf.Tick_Tiny,
                        icon = CancelIcon,
                        action = delegate ()
                        {
                            JobCleanup();
                            if (IsCombining)
                            {
                                EjectContents();
                            }
                        }
                    };
                }

                if (Prefs.DevMode && IsCombining)
                {
                    Command_Action command_Action = new Command_Action
                    {
                        defaultLabel = "Debug: Finish combining",
                        action = delegate
                        {
                            ticksDone = CombinationDuration();
                            FinishJob();
                        }
                    };
                    yield return command_Action;
                }
            }
        }

        public override bool Accepts(Thing thing)
        {
            return base.Accepts(thing) && (genepackToStore == thing || firstGenepackToCombine == thing || secondGenepackToCombine == thing);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref genepackToStore, "xenogermToDuplicate");
            Scribe_References.Look(ref firstGenepackToCombine, "firstGenepackToCombine");
            Scribe_References.Look(ref secondGenepackToCombine, "secondGenepackToCombine");
            Scribe_Defs.Look(ref geneToSeparate, "geneToSeparate");
        }
        public override string GetInspectString()
        {
            var sb = new StringBuilder();
            if (IsCombining)
            {
                var progress = ticksDone / (float)CombinationDuration();
                sb.AppendLine("RS.CombiningProgress".Translate(progress.ToStringPercent()));
            }
            else if (IsSeparating)
            {
                var progress = ticksDone / (float)ExtractionDuration(genepackToStore);
                sb.AppendLine("RS.SeparatingProgress".Translate(progress.ToStringPercent()));
                sb.AppendLine("RS.ContainsGenepack".Translate(genepackToStore.Label));
            }
            sb.Append(base.GetInspectString());
            return sb.ToString();
        }

        public override void Tick()
        {
            base.Tick();
            if (Powered && IsSeparating)
            {
                var durationTicks = ExtractionDuration(genepackToStore);
                DoWork(durationTicks);
            }
            else if (Powered && IsCombining)
            {
                var durationTicks = CombinationDuration();
                DoWork(durationTicks);
            }
        }

        protected override void FinishJob()
        {
            if (IsSeparating)
            {
                if (geneToSeparate is null)
                {
                    GenPlace.TryPlaceThing(genepackToStore, Position, Map, ThingPlaceMode.Near, extraValidator: (IntVec3 x) => x.GetFirstThing<Building_GeneCentrifuge>(this.Map) is null);
                    Log.Error(this + " finished separating gene, but there was no specified gene to separate. Perhaps it was removed from the game? Spawning the full genepack instead.");
                    return;
                }
                SeparateGenepack(genepackToStore);
            }
            else if (IsCombining)
            {
                FinishCombination();
            }
            JobCleanup();
        }

        private void SeparateGenepack(Genepack storedGenepack)
        {
            innerContainer.Remove(storedGenepack);
            var newGenepack = (Genepack)ThingMaker.MakeThing(storedGenepack.def);
            storedGenepack.GeneSet.genes.Remove(geneToSeparate);
            storedGenepack.GeneSet.DirtyCache();
            newGenepack.Initialize(new List<GeneDef>
                    {
                        geneToSeparate
                    });
            if (Rand.Chance(0.1f))
            {
                if (Rand.Bool)
                {
                    GenPlace.TryPlaceThing(storedGenepack, Position, Map, ThingPlaceMode.Near,
                        extraValidator: (IntVec3 x) => x.GetFirstThing<Building_GeneCentrifuge>(this.Map) is null);
                    Messages.Message("RS.FinishedSeparatingDestroyedMessage".Translate(newGenepack.LabelCap), new List<Thing>
                            {
                                storedGenepack
                            }, MessageTypeDefOf.CautionInput);
                    newGenepack.Destroy();
                }
                else
                {
                    GenPlace.TryPlaceThing(newGenepack, Position, Map, ThingPlaceMode.Near,
                        extraValidator: (IntVec3 x) => x.GetFirstThing<Building_GeneCentrifuge>(this.Map) is null);
                    Messages.Message("RS.FinishedSeparatingDestroyedMessage".Translate(storedGenepack.LabelCap), new List<Thing>
                            {
                                newGenepack
                            }, MessageTypeDefOf.CautionInput);
                    storedGenepack.Destroy();
                }
            }
            else
            {
                GenPlace.TryPlaceThing(newGenepack, Position, Map, ThingPlaceMode.Near,
                    extraValidator: (IntVec3 x) => x.GetFirstThing<Building_GeneCentrifuge>(this.Map) is null);
                GenPlace.TryPlaceThing(storedGenepack, Position, Map, ThingPlaceMode.Near,
                    extraValidator: (IntVec3 x) => x.GetFirstThing<Building_GeneCentrifuge>(this.Map) is null);
                Messages.Message("RS.FinishedSeparatingMessage".Translate(), new List<Thing>
                        {
                            storedGenepack, newGenepack
                        }, MessageTypeDefOf.CautionInput);
            }
        }

        protected override void JobCleanup()
        {
            base.JobCleanup();
            geneToSeparate = null;
            genepackToStore = null;
            firstGenepackToCombine = null;
            secondGenepackToCombine = null;
        }

        public int ExtractionDuration(Genepack genepack)
        {
            if (genepack.GeneSet.ArchitesTotal > 0)
            {
                return 360000;
            }
            return 120000;
        }

        public int CombinationDuration(List<Genepack> genepacks = null)
        {
            var duration = 0;
            var packs = genepacks ?? innerContainer.OfType<Genepack>().ToList();
            foreach (var pack in packs)
            {
                foreach (var gene in pack.GeneSet.GenesListForReading)
                {
                    duration += gene.biostatArc > 0 ? 360000 : 120000;
                }
            }
            return duration;
        }

        public override void StartJob()
        {
            genepackToStore = null;
        }

        public void SelectGenepackToProcess(Genepack genepack, GeneDef chosenGene)
        {
            if (chosenGene != null && genepack.GeneSet.GenesListForReading.Contains(chosenGene))
            {
                genepackToStore = genepack;
                geneToSeparate = chosenGene;
            }
            else
            {
                Log.Error($"Attempting to extract a {chosenGene} gene from a genepack which doesn't contain it.");
            }
        }

        public void SelectGenepacksToCombine(Genepack first, Genepack second)
        {
            firstGenepackToCombine = first;
            secondGenepackToCombine = second;
        }

        public void FinishCombination()
        {
            var packsInMachine = innerContainer.ToList();
            var firstPack = packsInMachine[0] as Genepack;
            var secondPack = packsInMachine[1] as Genepack;

            var newGeneSet = new GeneSet();
            foreach (var gene in firstPack.GeneSet.GenesListForReading)
            {
                newGeneSet.AddGene(gene);
            }
            foreach (var gene in secondPack.GeneSet.GenesListForReading)
            {
                newGeneSet.AddGene(gene);
            }

            var newGenepack = (Genepack)ThingMaker.MakeThing(firstPack.def);
            newGenepack.Initialize(newGeneSet.GenesListForReading);
            GenPlace.TryPlaceThing(newGenepack, Position, Map, ThingPlaceMode.Near);

            EjectContents();
            firstPack.Destroy();
            secondPack.Destroy();
        }
    }
}

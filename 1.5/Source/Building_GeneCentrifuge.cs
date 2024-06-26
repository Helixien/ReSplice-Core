﻿using RimWorld;
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

        public Genepack genepackToStore;

        public GeneDef geneToSeparate;
        public Genepack StoredGenepack => this.innerContainer.OfType<Genepack>().FirstOrDefault();

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
                        var allGenePacks = RS_Utils.allGenepacks.SelectMany(def => this.Map.listerThings.ThingsOfDef(def)
                            .Cast<Genepack>().Where(x => x.GeneSet.GenesListForReading.Count > 1)).ToList();
                        foreach (var bank in RS_Utils.allGeneBanks.SelectMany(def => this.Map.listerThings.ThingsOfDef(def)))
                        {
                            var comp = bank.TryGetComp<CompGenepackContainer>();
                            var genes = comp.ContainedGenepacks.Where(x => x.GeneSet.GenesListForReading.Count > 1).ToList();
                            if (genes.Any())
                            {
                                allGenePacks.AddRange(genes);
                            }
                        }

                        var floatList = new List<FloatMenuOption>();
                        foreach (var genepack in allGenePacks)
                        {
                            floatList.Add(new FloatMenuOption(genepack.LabelCap, delegate
                            {
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
                if (StoredGenepack != null)
                {
                    separateGene.Disable("RS.CentrigureWorking".Translate());
                }
                if (Powered is false)
                {
                    separateGene.Disable("NoPower".Translate());
                }
                yield return separateGene;

                if (StoredGenepack != null || genepackToStore != null)
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
                            if (StoredGenepack != null)
                            {
                                this.EjectContents();
                            }
                        }
                    };
                }

                if (Prefs.DevMode && StoredGenepack != null)
                {
                    Command_Action command_Action = new Command_Action
                    {
                        defaultLabel = "Debug: Finish separation",
                        action = delegate
                        {
                            ticksDone = ExtractionDuration(StoredGenepack);
                            FinishJob();
                        }
                    };
                    yield return command_Action;
                }
            }
        }

        public override bool Accepts(Thing thing)
        {
            return base.Accepts(thing) && genepackToStore == thing;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref genepackToStore, "xenogermToDuplicate");
            Scribe_Defs.Look(ref geneToSeparate, "geneToSeparate");
        }
        public override string GetInspectString()
        {
            var sb = new StringBuilder();
            if (StoredGenepack != null)
            {
                var progress = ticksDone / (float)ExtractionDuration(StoredGenepack);
                sb.AppendLine("RS.SeparatingProgress".Translate(progress.ToStringPercent()));
                sb.AppendLine("RS.ContainsGenepack".Translate(StoredGenepack.Label));
            }
            sb.Append(base.GetInspectString());
            return sb.ToString();
        }

        public override void Tick()
        {
            base.Tick();
            if (Powered && StoredGenepack != null)
            {
                var durationTicks = ExtractionDuration(StoredGenepack);
                DoWork(durationTicks);
            }
        }

        protected override void FinishJob()
        {
            var storedGenepack = StoredGenepack;
            var newGenepack = (Genepack)ThingMaker.MakeThing(storedGenepack.def);
            storedGenepack.GeneSet.genes.Remove(geneToSeparate);
            storedGenepack.GeneSet.DirtyCache();
            newGenepack.Initialize(new List<GeneDef>
                    {
                        geneToSeparate
                    });
            var removed = innerContainer.Remove(storedGenepack);
            if (Rand.Chance(0.1f))
            {
                if (Rand.Bool)
                {
                    GenPlace.TryPlaceThing(storedGenepack, Position, Map, ThingPlaceMode.Near,
                        nearPlaceValidator: (IntVec3 x) => x.GetFirstThing<Building_GeneCentrifuge>(this.Map) is null);
                    Messages.Message("RS.FinishedSeparatingDestroyedMessage".Translate(newGenepack.LabelCap), new List<Thing>
                            {
                                storedGenepack
                            }, MessageTypeDefOf.CautionInput);
                    newGenepack.Destroy();
                }
                else
                {
                    GenPlace.TryPlaceThing(newGenepack, Position, Map, ThingPlaceMode.Near,
                        nearPlaceValidator: (IntVec3 x) => x.GetFirstThing<Building_GeneCentrifuge>(this.Map) is null);
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
                    nearPlaceValidator: (IntVec3 x) => x.GetFirstThing<Building_GeneCentrifuge>(this.Map) is null);
                GenPlace.TryPlaceThing(storedGenepack, Position, Map, ThingPlaceMode.Near,
                    nearPlaceValidator: (IntVec3 x) => x.GetFirstThing<Building_GeneCentrifuge>(this.Map) is null);
                Messages.Message("RS.FinishedSeparatingMessage".Translate(), new List<Thing>
                        {
                            storedGenepack, newGenepack
                        }, MessageTypeDefOf.CautionInput);
            }
            JobCleanup();
        }
        protected override void JobCleanup()
        {
            base.JobCleanup();
            geneToSeparate = null;
            genepackToStore = null;
        }

        public int ExtractionDuration(Genepack genepack)
        {
            if (genepack.GeneSet.ArchitesTotal > 0)
            {
                return 360000;
            }
            return 120000;
        }

        public override void StartJob()
        {
            genepackToStore = null;
        }
    }
}

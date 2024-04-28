using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ReSpliceCore
{
    [StaticConstructorOnStartup]
    public class DarkArchiteSpike : GeneSetHolderBase
    {
        private Pawn targetPawn;

        private static readonly CachedTexture InfuseTex = new CachedTexture("UI/Gizmos/InfuseGenes");

        private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

        public override string LabelNoCount
        {
            get
            {
                return base.LabelNoCount + " (" + geneSet.GenesListForReading.Select(x => x.label).ToStringSafeEnumerable() + ")";
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            geneSet = GenerateGeneSet();
        }

        public static GeneSet GenerateGeneSet(int? seed = null)
        {
            GeneSet geneSet = new GeneSet();
            if (seed.HasValue)
            {
                Rand.PushState(seed.Value);
            }
            var gene = DefDatabase<GeneDef>.AllDefsListForReading.Where(x => x.displayCategory == RS_DefOf.RS_DarkArchite).RandomElement();
            geneSet.AddGene(gene);
            geneSet.GenerateName();
            if (seed.HasValue)
            {
                Rand.PopState();
            }
            if (geneSet.Empty)
            {
                Log.Error("Generated gene pack with no genes.");
            }
            geneSet.SortGenes();
            return geneSet;
        }

        public void SetTargetPawn(Pawn newTarget)
        {
            int trueMax = RS_DefOf.RS_InfusionComa.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks.TrueMax;
            TaggedString text = "RS.InfuseDarkArchiteWarningDesc".Translate(newTarget.Named("PAWN"), trueMax.ToStringTicksToPeriod().Named("COMADURATION"));
            int num = GeneUtility.MetabolismAfterImplanting(newTarget, geneSet);
            text += "\n\n" + "RS.InfuseDarkArchiteWarningNewMetabolism".Translate(newTarget.Named("PAWN"), num.Named("MET"), GeneTuning.MetabolismToFoodConsumptionFactorCurve.Evaluate(num).ToStringPercent().Named("CONSUMPTION"));
            text += "\n\n" + "WouldYouLikeToContinue".Translate();
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate
            {
                Bill bill = targetPawn?.BillStack?.Bills.OfType<Bill_InfuseDarkArchite>()?.FirstOrDefault(x => x.spike == this);
                if (bill != null)
                {
                    targetPawn.BillStack.Delete(bill);
                }
                CreateSurgeryBill(newTarget, RS_DefOf.RS_InfuseDarkArchite, null);
                targetPawn = newTarget;
                SendLetter(newTarget);
            }, destructive: true));
        }

        public Bill_InfuseDarkArchite CreateSurgeryBill(Pawn medPawn, RecipeDef recipe, BodyPartRecord part, List<Thing> uniqueIngredients = null, bool sendMessages = true)
        {
            Bill_InfuseDarkArchite bill_Medical = new Bill_InfuseDarkArchite(this, recipe, uniqueIngredients);
            medPawn.BillStack.AddBill(bill_Medical);
            bill_Medical.Part = part;
            if (recipe.conceptLearned != null)
            {
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
            }
            if (sendMessages)
            {
                Map mapHeld = medPawn.MapHeld;
                if (!(from p in mapHeld.mapPawns.PawnsInFaction(Faction.OfPlayer)
                      where p.IsFreeColonist || p.IsColonyMechPlayerControlled
                      select p).Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
                {
                    Bill.CreateNoPawnsWithSkillDialog(recipe);
                }
                if (!medPawn.InBed() && medPawn.RaceProps.IsFlesh)
                {
                    if (!mapHeld.listerBuildings.allBuildingsColonist.Any((Building x) => x is Building_Bed && RestUtility.CanUseBedEver(medPawn, x.def) && ((Building_Bed)x).Medical))
                    {
                        Messages.Message("MessageNoMedicalBeds".Translate(), medPawn, MessageTypeDefOf.CautionInput, historical: false);
                    }
                }
                if (medPawn.Faction != null && !medPawn.Faction.Hidden && !medPawn.Faction.HostileTo(Faction.OfPlayer) && recipe.Worker.IsViolationOnPawn(medPawn, part, Faction.OfPlayer))
                {
                    Messages.Message("MessageMedicalOperationWillAngerFaction".Translate(medPawn.HomeFaction), medPawn, MessageTypeDefOf.CautionInput, historical: false);
                }
                recipe.Worker.CheckForWarnings(medPawn);
            }
            return bill_Medical;
        }

        private void SendLetter(Pawn targetPawn)
        {
            string arg = string.Empty;
            if (!targetPawn.InBed() && !targetPawn.Map.listerBuildings.allBuildingsColonist.Any((Building x) => x is Building_Bed && RestUtility.CanUseBedEver(targetPawn, x.def) && ((Building_Bed)x).Medical))
            {
                arg = "XenogermOrderedImplantedBedNeeded".Translate(targetPawn.Named("PAWN"));
            }
            Find.LetterStack.ReceiveLetter("RS.LetterLabelDarkArchiteOrderedInfused".Translate(), "RS.LetterDarkArchiteOrderedInfused".Translate(targetPawn.Named("PAWN"), arg.Named("BEDINFO")), LetterDefOf.NeutralEvent);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption;
            }
            if (!ModsConfig.BiotechActive || selPawn.genes == null)
            {
                yield break;
            }
            int num = GeneUtility.MetabolismAfterImplanting(selPawn, geneSet);
            if (num < GeneTuning.BiostatRange.TrueMin)
            {
                yield return new FloatMenuOption("CannotGenericWorkCustom".Translate((string)("RS.OrderInfusionIntoPawn".Translate(selPawn.Named("PAWN")).Resolve().UncapitalizeFirst() + ": " + "ResultingMetTooLow".Translate() + " (") + num + ")"), null);
                yield break;
            }
            if (PawnIdeoDisallowsInfuseing(selPawn))
            {
                yield return new FloatMenuOption("CannotGenericWorkCustom".Translate("RS.OrderInfusionIntoPawn".Translate(selPawn.Named("PAWN")).Resolve().UncapitalizeFirst() + ": " + "IdeoligionForbids".Translate()), null);
                yield break;
            }
            yield return new FloatMenuOption("RS.OrderInfusionIntoPawn".Translate(selPawn.Named("PAWN")), delegate
            {
                SetTargetPawn(selPawn);
            });
        }

        public bool PawnIdeoDisallowsInfuseing(Pawn selPawn)
        {
            if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive)
            {
                return false;
            }
            if (!IdeoUtility.DoerWillingToDo(HistoryEventDefOf.PropagateBloodfeederGene, selPawn) && base.GeneSet.GenesListForReading.Any((GeneDef x) => x == GeneDefOf.Bloodfeeder))
            {
                return true;
            }
            if (!IdeoUtility.DoerWillingToDo(HistoryEventDefOf.BecomeNonPreferredXenotype, selPawn))
            {
                return true;
            }
            return false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (geneSet == null)
            {
                yield break;
            }
            if (targetPawn == null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "RS.InfuseDarkArchite".Translate() + "...",
                    defaultDesc = "RS.InfuseDarkArchiteDesc".Translate(),
                    icon = InfuseTex.Texture,
                    action = delegate
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        foreach (Pawn item in base.Map.mapPawns.AllPawnsSpawned)
                        {
                            Pawn pawn = item;
                            if (!pawn.IsQuestLodger() && pawn.genes != null && (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony || (pawn.IsColonyMutant && pawn.IsGhoul)))
                            {
                                int num = GeneUtility.MetabolismAfterImplanting(pawn, geneSet);
                                if (num < GeneTuning.BiostatRange.TrueMin)
                                {
                                    list.Add(new FloatMenuOption((string)(pawn.LabelShortCap + ": " + "ResultingMetTooLow".Translate() + " (") + num + ")", null, pawn, Color.white));
                                }
                                else if (PawnIdeoDisallowsInfuseing(pawn))
                                {
                                    list.Add(new FloatMenuOption(pawn.LabelShortCap + ": " + "IdeoligionForbids".Translate(), null, pawn, Color.white));
                                }
                                else
                                {
                                    list.Add(new FloatMenuOption(pawn.LabelShortCap + ", " + pawn.genes.XenotypeLabelCap, delegate
                                    {
                                        SetTargetPawn(pawn);
                                    }, pawn, Color.white));
                                }
                            }
                        }
                        if (!list.Any())
                        {
                            list.Add(new FloatMenuOption("NoInfuseablePawns".Translate(), null));
                        }
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                };
                yield break;
            }
            yield return new Command_Action
            {
                defaultLabel = "RS.CancelInfusion".Translate(),
                defaultDesc = "RS.CancelInfusingDesc".Translate(targetPawn.Named("PAWN")),
                icon = CancelIcon,
                action = delegate
                {
                    Bill bill = targetPawn?.BillStack?.Bills.OfType<Bill_InfuseDarkArchite>()?.FirstOrDefault(x => x.spike == this);
                    if (bill != null)
                    {
                        targetPawn.BillStack.Delete(bill);
                    }
                }
            };
        }

        public void Notify_BillRemoved()
        {
            targetPawn = null;
        }

        public bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!target.IsValid || target.Pawn == null)
            {
                return false;
            }
            if (target.Pawn.IsQuestLodger())
            {
                if (showMessages)
                {
                    Messages.Message("RS.MessageCannotInfuseInTempFactionMembers".Translate(), target.Pawn, MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            if (!target.Pawn.IsColonist && !target.Pawn.IsPrisonerOfColony && !target.Pawn.IsSlaveOfColony)
            {
                if (showMessages)
                {
                    Messages.Message("MessageCanOnlyTargetColonistsPrisonersAndSlaves".Translate(), target.Pawn, MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            return true;
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if (targetPawn != null && targetPawn.Map == base.Map)
            {
                GenDraw.DrawLineBetween(this.TrueCenter(), targetPawn.TrueCenter());
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref targetPawn, "targetPawn");
        }
    }
}

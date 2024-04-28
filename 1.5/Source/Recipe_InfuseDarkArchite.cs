using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ReSpliceCore
{
    public class Recipe_InfuseDarkArchite : Recipe_Surgery
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return false;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (!CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
            {
                if (bill is Bill_InfuseDarkArchite infuse)
                {
                    foreach (GeneDef item in infuse.spike.GeneSet.GenesListForReading)
                    {
                        pawn.genes.AddGene(item, xenogene: true);
                    }
                }
                if (IsViolationOnPawn(pawn, part, Faction.OfPlayer))
                {
                    ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
                }
                if (ModsConfig.IdeologyActive)
                {
                    Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.InstalledProsthetic,
                        billDoer.Named(HistoryEventArgsNames.Doer)));
                }
            }
        }
    }
}

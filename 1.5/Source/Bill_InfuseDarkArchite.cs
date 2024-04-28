using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ReSpliceCore
{
    public class Bill_InfuseDarkArchite : Bill_Medical
    {
        public DarkArchiteSpike spike;

        public Bill_InfuseDarkArchite()
        {
        }

        public Bill_InfuseDarkArchite(DarkArchiteSpike spike, RecipeDef recipe, List<Thing> uniqueIngredients)
            : base(recipe, uniqueIngredients)
        {
            this.spike = spike;
        }

        public override Bill Clone()
        {
            var bill = base.Clone() as Bill_InfuseDarkArchite;
            bill.spike = this.spike;
            return bill;
        }

        public Effecter effecter;
        public override void Notify_PawnDidWork(Pawn p)
        {
            base.Notify_PawnDidWork(p);
            var patient = this.GiverPawn;
            if (effecter is null)
            {
                effecter = EffecterDefOf.MonolithStage1.SpawnAttached(patient, patient.Map);
            }
            effecter.EffectTick(patient, patient);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref spike, "spike");
        }
    }
}

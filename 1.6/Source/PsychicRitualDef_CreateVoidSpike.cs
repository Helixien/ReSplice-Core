using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace ReSpliceCore
{
    public class PsychicRitualDef_CreateVoidSpike : PsychicRitualDef_InvocationCircle
    {
        public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            List<PsychicRitualToil> list = base.CreateToils(psychicRitual, parent);
            list.Add(new PsychicRitualToil_CreateVoidSpike(TargetRole));
            return list;
        }

        public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
        {
            return outcomeDescription.Formatted(qualityRange.Average.ToStringPercent());
        }

        public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
        {
            using (new ProfilerBlock("PsychicRitualDef.BlockingIssues"))
            {
                var pawn = assignments.FirstAssignedPawn(TargetRole);
                if (pawn?.genes != null)
                {
                    if (pawn.genes.GenesListForReading.Any(x => x.def.IsDarkArchite()) is false)
                    {
                        yield return "RS.TargetMustHaveAtLeastOneDarkGene".Translate();
                    }
                }
            }
        }
    }
}

using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ReSpliceCore
{
    public class PsychicRitualToil_CreateVoidSpike : PsychicRitualToil
    {
        private PsychicRitualRoleDef targetRole;

        protected PsychicRitualToil_CreateVoidSpike()
        {
        }

        public PsychicRitualToil_CreateVoidSpike(PsychicRitualRoleDef targetRole)
        {
            this.targetRole = targetRole;
        }

        public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            base.Start(psychicRitual, parent);
            Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(targetRole);
            psychicRitual.ReleaseAllPawnsAndBuildings();
            if (pawn != null)
            {
                pawn.health.AddHediff(RS_DefOf.RS_DarkArchiteExtraction);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref targetRole, "targetRole");
        }
    }
}
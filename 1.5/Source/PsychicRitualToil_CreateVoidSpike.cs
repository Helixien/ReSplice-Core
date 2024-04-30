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
                var positionHeld = pawn.PositionHeld;
                var mapHeld = pawn.MapHeld;
                Explode(pawn, positionHeld, mapHeld);
                if (Rand.Chance(psychicRitual.PowerPercent))
                {
                    var darkGene = pawn.genes.GenesListForReading.Where(x => x.def.displayCategory == RS_DefOf.RS_DarkArchite).RandomElement();
                    var voidSpike = ThingMaker.MakeThing(RS_DefOf.RS_DarkArchiteSpike) as DarkArchiteSpike;
                    voidSpike.geneSet = new GeneSet();
                    voidSpike.geneSet.AddGene(darkGene.def);
                    voidSpike.geneSet.GenerateName();
                    voidSpike.geneSet.SortGenes();
                    GenSpawn.Spawn(voidSpike, positionHeld, mapHeld);
                }
            }
        }


        private static readonly IntRange MeatPieces = new IntRange(3, 4);

        private static readonly IntRange BloodFilth = new IntRange(3, 4);

        public void Explode(Pawn pawn, IntVec3 positionHeld, Map mapHeld)
        {
            int num = Mathf.Max(GenMath.RoundRandom(pawn.GetStatValue(StatDefOf.MeatAmount)), 3);
            pawn.Kill(new DamageInfo(DamageDefOf.Psychic, 99999f, 0f, -1f));
            if (pawn.Corpse != null)
            {
                pawn.Corpse.Destroy();
            }

            EffecterDefOf.MeatExplosion.Spawn(positionHeld, mapHeld).Cleanup();
            FleshbeastUtility.MeatSplatter(BloodFilth.RandomInRange, positionHeld, mapHeld, FleshbeastUtility.MeatExplosionSize.Large);
            int stackLimit = ThingDefOf.Meat_Twisted.stackLimit;
            int randomInRange = MeatPieces.RandomInRange;
            for (int i = 0; i < randomInRange; i++)
            {
                if (CellFinder.TryRandomClosewalkCellNear(positionHeld, mapHeld, 1, out var result))
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Meat_Twisted);
                    int maxInclusive = Mathf.Min(stackLimit, num - (randomInRange - i - 1));
                    thing.stackCount = Rand.RangeInclusive(1, maxInclusive);
                    num -= thing.stackCount;
                    GenSpawn.Spawn(thing, result, mapHeld);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref targetRole, "targetRole");
        }
    }
}
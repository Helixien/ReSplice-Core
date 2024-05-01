using RimWorld;
using System.Linq;
using Verse;

namespace ReSpliceCore
{
    public abstract class Gene_Tentacles : Gene
    {
        public abstract HediffDef TentacleDef { get; }
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn.health.hediffSet.GetFirstHediffOfDef(TentacleDef) is null)
            {
                var countToAdd = Rand.RangeInclusive(1, 2);
                var bodyParts = pawn.RaceProps.body.GetPartsWithDef(TentacleDef.defaultInstallPart).InRandomOrder().ToList();
                if (bodyParts.Count >= countToAdd)
                {
                    for (var i = 0; i < countToAdd; i++)
                    {
                        var part = bodyParts[i];
                        if (pawn.health.hediffSet.PartIsMissing(part) is false
                            && pawn.health.hediffSet.HasDirectlyAddedPartFor(part) is false)
                        {
                            pawn.health.AddHediff(TentacleDef, part);
                        }
                    }
                }
            }
        }
    }

    public class Gene_FleshTentacles : Gene_Tentacles
    {
        public override HediffDef TentacleDef => HediffDefOf.Tentacle;
    }

    public class Gene_FleshWhips : Gene_Tentacles
    {
        public override HediffDef TentacleDef => HediffDefOf.FleshWhip;
    }
}

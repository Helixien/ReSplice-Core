using Verse;

namespace ReSpliceCore
{
    public class GeneExtension : DefModExtension
    {
        private GeneDef linkedXenogene;
        private GeneDef linkedEndogene;

        public void TryAddXenogene(Pawn pawn)
        {
            if (linkedXenogene != null)
            {
                if (pawn.genes.HasActiveGene(linkedXenogene) is false)
                {
                    pawn.genes.AddGene(linkedXenogene, true);
                }
            }
        }
        public void TryAddEndogene(Pawn pawn)
        {
            if (linkedEndogene != null)
            {
                if (pawn.genes.HasActiveGene(linkedEndogene) is false)
                {
                    pawn.genes.AddGene(linkedEndogene, false);
                }
            }
        }

        public void TryRemoveXenogene(Pawn pawn)
        {
            if (linkedXenogene != null)
            {
                TryRemoveGene(pawn, linkedXenogene);
            }
        }

        public void TryRemoveEndogene(Pawn pawn)
        {
            if (linkedEndogene != null)
            {
                TryRemoveGene(pawn, linkedEndogene);
            }
        }

        private void TryRemoveGene(Pawn pawn, GeneDef geneDef)
        {
            var gene = pawn.genes.GetGene(geneDef);
            if (gene != null)
            {
                pawn.genes.RemoveGene(gene);
            }
        }
    }
}

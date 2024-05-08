using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ReSpliceCore
{
    public class JobDriver_InsertIntoProcessor : JobDriver_HaulToContainer
    {
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
            this.FailOn(delegate
            {
                ThingOwner thingOwner = Container.TryGetInnerInteractableThingOwner();
                if (thingOwner != null && !thingOwner.CanAcceptAnyOf(ThingToCarry))
                {
                    return true;
                }
                IHaulDestination haulDestination = Container as IHaulDestination;
                return (haulDestination != null && !haulDestination.Accepts(ThingToCarry)) ? true : false;
            });
            Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            Toil startCarryingThing = Toils_Haul.StartCarryThing(TargetIndex.C, putRemainderInQueue: false, 
                subtractNumTakenFromJobCount: true, canTakeFromInventory: true);
            Toil carryToContainer = Toils_Haul.CarryHauledThingToContainer();
            yield return getToHaulTarget;
            yield return startCarryingThing;
            yield return carryToContainer;
            Toil toil = Toils_General.Wait(Duration, TargetIndex.B);
            toil.WithProgressBarToilDelay(TargetIndex.B);
            EffecterDef workEffecter = WorkEffecter;
            if (workEffecter != null)
            {
                toil.WithEffect(workEffecter, TargetIndex.B);
            }
            SoundDef workSustainer = WorkSustainer;
            if (workSustainer != null)
            {
                toil.PlaySustainerOrSound(workSustainer);
            }
            Thing destThing = job.GetTarget(TargetIndex.B).Thing;
            yield return toil;
            yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.C, delegate
            {
                var processor = Container as Building_Processor;
                processor.StartJob();
            });
        }
    }
}

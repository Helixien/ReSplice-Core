using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ReSpliceCore;

[StaticConstructorOnStartup]
public class CompArchiteRefuelable : CompRefuelable
{
    public static readonly Texture2D EjectContents = ContentFinder<Texture2D>.Get("UI/Designators/EjectFuel");

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
        {
            if (gizmo is Gizmo_SetFuelLevel)
                yield return new Gizmo_SetFuelLevel_Rounded(this);
            else
                yield return gizmo;
        }

        // CompRefuelable supports an eject gizmo/designator, but its label is always "eject fuel".
        // No need for cancel gizmo, as the designator will automatically handle it. In fact, getting rid of it adds extra complexity.
        if (Fuel > 0 && parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.EjectFuel) == null)
        {
            var ejectGizmo = new Command_Action
            {
                defaultLabel = "RS.EjectArchites".Translate(),
                defaultDesc = "RS.EjectArchitesDesc".Translate(),
                icon = EjectContents,
                Order = -19.9f, // Designator.CreateReverseDesignationGizmo uses -20, but use slightly less to be ahead of deconstruct gizmo
                action = () => parent.Map.designationManager.AddDesignation(new Designation(parent, DesignationDefOf.EjectFuel)),
                activateSound = SoundDefOf.Tick_Tiny,
            };

            if (explosiveComp is { wickStarted: true })
                ejectGizmo.Disable("AboutToExplode".Translate());

            yield return ejectGizmo;
        }
    }

    public override string CompInspectStringExtra()
    {
        if (!Props.targetFuelLevelConfigurable)
            return base.CompInspectStringExtra();

        var text = $"{Props.FuelLabel}: {fuel.ToStringDecimalIfSmall()} / {Props.fuelCapacity.ToStringDecimalIfSmall()}";
        if (Props.consumeFuelOnlyWhenUsed is false && HasFuel)
        {
            var numTicks = (int)(fuel / Props.fuelConsumptionRate * 60000f);
            text += $" ({numTicks.ToStringTicksToPeriod()})";
        }
        if (HasFuel is false && !Props.outOfFuelMessage.NullOrEmpty())
        {
            string value = parent.def.building?.IsTurret is true ? ("CannotShoot".Translate() + ": " + Props.outOfFuelMessage).Resolve() : Props.outOfFuelMessage;
            text += $"\n{value} ({GetFuelCountToFullyRefuel()}x {Props.fuelFilter.AnyAllowedDef.label})";
        }
        return $"{text}\n" + "RS.TargetArchiteCapsuleLevel".Translate(TargetFuelLevel.ToStringDecimalIfSmall());
    }
}
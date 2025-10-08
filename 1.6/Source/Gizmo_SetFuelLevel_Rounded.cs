using RimWorld;
using UnityEngine;

namespace ReSpliceCore;

public class Gizmo_SetFuelLevel_Rounded(CompRefuelable refuelable) : Gizmo_SetFuelLevel(refuelable)
{
    public override float Target
    {
        get => base.Target;
        set => refuelable.TargetFuelLevel = Mathf.Clamp(Mathf.RoundToInt(value * refuelable.Props.fuelCapacity), 0, refuelable.Props.fuelCapacity);
    }
}
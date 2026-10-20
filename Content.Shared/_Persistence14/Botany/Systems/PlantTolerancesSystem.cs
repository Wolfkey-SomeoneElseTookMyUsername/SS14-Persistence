using JetBrains.Annotations;

namespace Content.Shared._Persistence14.Botany.Systems;

using Content.Shared.Botany.Components;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class PlantTolerancesSystem : EntitySystem
{
    [Dependency] private EntityQuery<PlantAtmosphericComponent> _atmosQuery = default!;
    [Dependency] private EntityQuery<PlantChemicalsComponent> _chemQuery = default!;
    [Dependency] private EntityQuery<PlantConsumeExudeGasComponent> _gasQuery = default!;

    [SubscribeLocalEvent]
    private void OnPlantAtmosphericInit(Entity<PlantAtmosphericComponent> ent, ref ComponentInit _)
    {
        UpdateModifiers(ent);
    }

    [PublicAPI]
    public bool TryUpdateModifiers(EntityUid ent)
    {
        if (!_atmosQuery.TryComp(ent, out var atmos))
            return false;

        UpdateModifiers((ent, atmos));
        return true;
    }

    [PublicAPI]
    public void UpdateModifiers(Entity<PlantAtmosphericComponent> ent)
    {
        ent.Comp.ModifiedHeatToleranceDamage = ent.Comp.HeatToleranceDamage;
        ent.Comp.ModifiedHighHeatTolerance = ent.Comp.HighHeatTolerance;
        ent.Comp.ModifiedLowHeatTolerance = ent.Comp.LowHeatTolerance;
        ent.Comp.ModifiedPressureToleranceDamage = ent.Comp.PressureToleranceDamage;
        ent.Comp.ModifiedHighPressureTolerance = ent.Comp.HighPressureTolerance;
        ent.Comp.ModifiedLowPressureTolerance = ent.Comp.LowPressureTolerance;
    }
}

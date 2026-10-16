using System.Diagnostics.CodeAnalysis;
using Content.Shared._Persistence14.Botany.Components;
using Content.Shared.Botany.Components;
using Content.Shared.Botany.Systems;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Botany.Systems;

public sealed partial class PlantRequirementsSystem : EntitySystem
{
    [Dependency] private PlantSystem _plant = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    [Dependency] private EntityQuery<PlantBaseRequirementsComponent> _requirementsQuery = default!;
    [Dependency] private EntityQuery<PlantChemicalsComponent> _chemQuery = default!;

    /// <summary>
    /// Returns the total requirements for all nutrients in the plant.
    /// </summary>
    [PublicAPI]
    public Dictionary<ProtoId<PlantNutrientPrototype>, NutrientRequirement> GetAllRequirements(
        Entity<PlantComponent?> ent)
    {
        var baseComp = _requirementsQuery.CompOrNull(ent.Owner);
        var chemicalsComp = _chemQuery.CompOrNull(ent.Owner);

        return GetAllRequirements((ent.Owner, baseComp, chemicalsComp));
    }

    [PublicAPI]
    public Dictionary<ProtoId<PlantNutrientPrototype>, NutrientRequirement> GetAllRequirements(
        Entity<PlantBaseRequirementsComponent?, PlantChemicalsComponent?> ent)
    {
        Dictionary<ProtoId<PlantNutrientPrototype>, NutrientRequirement> totalRequirement = new();

        if (Resolve(ent.Owner, ref ent.Comp1, false))
        {
            foreach (var (nutrient, baseRequirement) in ent.Comp1.BaseRequirements)
            {
                NutrientRequirement requirement = new()
                {
                    Requirement = baseRequirement,
                };
                totalRequirement.Add(nutrient, requirement);
            }
        }

        if (!Resolve(ent.Owner, ref ent.Comp2, false))
            return totalRequirement;

        foreach (var chemQuantity in ent.Comp2.Chemicals.Values)
        {
            var quantityProto = _prototypeManager.Index(chemQuantity.Id);
            foreach (var (nutrient, requirement) in quantityProto.Requirements)
            {
                if (!totalRequirement.TryAdd(nutrient, requirement))
                {
                    var newRequirement = totalRequirement[nutrient];
                    newRequirement.Requirement += requirement.Requirement;
                    newRequirement.BonusRequirement += requirement.BonusRequirement;
                    newRequirement.BonusAmount += requirement.BonusAmount;
                    totalRequirement[nutrient] = newRequirement;
                }
            }
        }

        return totalRequirement;
    }

    /// <summary>
    /// Returns true if all the non-bonus requirements of the plant are fulfilled.
    /// </summary>
    [PublicAPI]
    public bool AllRequirementsFulfilled(Entity<PlantComponent?> ent)
    {
        if (!_plant.TryGetTray(ent, out var tray))
            return false;

        return AllRequirementsFulfilled(ent, tray);
    }

    /// <summary>
    /// Returns true if all the non-bonus requirements of the plant are fulfilled by the nutrients in the tray.
    /// </summary>
    [PublicAPI]
    public bool AllRequirementsFulfilled(Entity<PlantComponent?> ent, Entity<PlantTrayComponent> tray)
    {

        var requirements = GetAllRequirements(ent);
        var nutrients = tray.Comp.Nutrients;

        return AllRequirementsFulfilled(requirements, nutrients);
    }

    /// <summary>
    /// Returns true if all the non-bonus requirements are fulfilled by the nutrients.
    /// </summary>
    [PublicAPI]
    public bool AllRequirementsFulfilled(Dictionary<ProtoId<PlantNutrientPrototype>, NutrientRequirement> requirements,
        Dictionary<ProtoId<PlantNutrientPrototype>, FixedPoint2> nutrients)
    {
        foreach (var (nutrientId, requirement) in requirements)
        {
            if (requirement.Requirement == 0)
                continue;
            if (!nutrients.TryGetValue(nutrientId, out var nutrientAmount))
                return false;
            if (nutrientAmount < requirement.Requirement)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Attempts to return a whether each of the plant's requirements' mandatory and bonus requirements are fulfilled. Should only fail if a plant is not in a tray for some reason.
    /// </summary>
    [PublicAPI]
    public bool TryGetFulfilledRequirements(Entity<PlantComponent?> ent, [NotNullWhen(true)] out Dictionary<ProtoId<PlantNutrientPrototype>, (bool, bool)>? info)
    {
        if (!_plant.TryGetTray(ent, out var tray))
        {
            info = null;
            return false;
        }

        info = GetFulfilledRequirements(ent, tray);
        return true;
    }

    /// <summary>
    /// Returns a whether each of the plant's requirements' mandatory and bonus requirements are fulfilled by the tray's nutrients.
    /// </summary>
    [PublicAPI]
    public Dictionary<ProtoId<PlantNutrientPrototype>, (bool, bool)> GetFulfilledRequirements(Entity<PlantComponent?> ent, Entity<PlantTrayComponent> tray)
    {
        var requirements = GetAllRequirements(ent);
        var nutrients = tray.Comp.Nutrients;

        return GetFulfilledRequirements(requirements, nutrients);
    }

    /// <summary>
    /// Returns a whether each of the requirements' mandatory and bonus requirements are fulfilled.
    /// </summary>
    [PublicAPI]
    public Dictionary<ProtoId<PlantNutrientPrototype>, (bool, bool)> GetFulfilledRequirements(Dictionary<ProtoId<PlantNutrientPrototype>, NutrientRequirement> requirements,
        Dictionary<ProtoId<PlantNutrientPrototype>, FixedPoint2> nutrients)
    {
        Dictionary<ProtoId<PlantNutrientPrototype>, (bool, bool)> info = new();

        foreach (var (nutrientId, requirement) in requirements)
        {
            var totalBonus = requirement.Requirement + requirement.BonusRequirement;

            if (!nutrients.TryGetValue(nutrientId, out var nutrientAmount))
                info[nutrientId] = (requirement.Requirement == 0, totalBonus == 0);
            else
                info[nutrientId] = (requirement.Requirement <= nutrientAmount, totalBonus <= nutrientAmount);
        }

        return info;
    }

}

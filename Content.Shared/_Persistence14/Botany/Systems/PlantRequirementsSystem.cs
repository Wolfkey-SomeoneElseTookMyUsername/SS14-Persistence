using Content.Shared._Persistence14.Botany.Components;
using Content.Shared.Botany.Components;
using Content.Shared.Botany.Systems;
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
    /// Returns the total requirements for all nutrients.
    /// </summary>
    [PublicAPI]
    public Dictionary<ProtoId<PlantNutrientPrototype>, NutrientRequirement> GetAllRequirements(Entity<PlantComponent?> ent)
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
}

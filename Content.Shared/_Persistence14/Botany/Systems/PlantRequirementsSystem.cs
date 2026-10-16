using Content.Shared._Persistence14.Botany.Components;
using Content.Shared.Botany.Components;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Botany.Systems;

public sealed partial class PlantRequirementsSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private EntityQuery<PlantBaseRequirementsComponent> _requirementsQuery = default!;
    [Dependency] private EntityQuery<PlantChemicalsComponent> _chemQuery = default!;

    /// <summary>
    /// Returns the total requirements for all nutrients.
    /// </summary>
    [PublicAPI]
    public Dictionary<ProtoId<PlantNutrientPrototype>, NutrientRequirement> GetAllRequirements(Entity<PlantComponent?> ent) // The plant component is never used, but i don't know of a better way to do this.
    {
        Dictionary<ProtoId<PlantNutrientPrototype>, NutrientRequirement> totalRequirement = new();

        if (_requirementsQuery.TryComp(ent, out var baseRequirementComp))
        {
            foreach (var (nutrient, baseRequirement) in baseRequirementComp.BaseRequirements)
            {
                NutrientRequirement requirement = new()
                {
                    Requirement = baseRequirement,
                };
                totalRequirement.Add(nutrient, requirement);
            }
        }

        if (!_chemQuery.TryComp(ent, out var chemicalsComp))
            return totalRequirement;

        foreach (var chemQuantity in chemicalsComp.Chemicals.Values)
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

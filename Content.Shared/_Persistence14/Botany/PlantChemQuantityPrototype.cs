using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.FixedPoint;

namespace Content.Shared._Persistence14.Botany;

/// <summary>
/// Prototype used to track the amount of a chemical in a plant, and the requirements and tolerance modifiers of the plant.
/// This is a prototype instead of a struct to allow plants to be changed without leaving behind legacy plants with the old values.
/// </summary>
[Prototype]
[DataDefinition]
public sealed partial class PlantChemQuantityPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField] public FixedPoint2 BaseAmount;
    [DataField] public Dictionary<ProtoId<PlantNutrientPrototype>, NutrientRequirement> Requirements;
}

[Serializable, NetSerializable]
public struct NutrientRequirement
{
    /// <summary>
    /// The nutrient required by this nutrient requirement
    /// </summary>
    [DataField("Nutrient")] public ProtoId<PlantNutrientPrototype> Nutrient;

    /// <summary>
    /// Minimum amount of nutrient required for a plant with this requirement to grow.
    /// </summary>
    [DataField("Requirement")] public FixedPoint2 Requirement;

    /// <summary>
    /// Amount of nutrient above the minimum requirement needed to get all bonus chemicals.
    /// </summary>
    [DataField("BonusRequirement")] public FixedPoint2 BonusRequirement;

    /// <summary>
    /// Amount of extra chemicals added to the plant's produce when the BonusRequirement is fulfilled.
    /// </summary>
    [DataField("BonusAmount")] public FixedPoint2 BonusAmount;
}

[Serializable, NetSerializable]
public struct ToleranceModifier
{
    /// <summary>
    /// Is added to the minimum pressure or heat tolerance of the plant.
    /// </summary>
    [DataField("Low")] public float LowToleranceModifier;

    /// <summary>
    /// Is added to the maximum pressure or heat tolerance of the plant.
    /// </summary>
    [DataField("High")] public float HighToleranceModifier;

    /// <summary>
    /// The amount of damage the plant takes from improper pressure or heat is multiplied by this amount
    /// </summary>
    [DataField("Damage")] public float DamageMultiplier; // Multiplied instead of added to prevent plants that heal from improper conditions.
}

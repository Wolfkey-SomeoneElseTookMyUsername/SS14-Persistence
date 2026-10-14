plant-component-has-requirements-message = This plant requires the following nutrients:

plant-component-nutrient-requirement-message = [color={$color}]{CAPITALIZE($name)}[/color]: { $fulfilled ->
        [true] [color=green]{$requiredNutrients}[/color]
       *[false] [color=red]{$requiredNutrients}[/color]
    }

plant-component-nutrient-bonus-message = -{ $fulfilled ->
        [true] [color=green]{$bonusNutrients}[/color]
       *[false] {$bonusNutrients}
    }

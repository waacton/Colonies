﻿namespace Wacton.Colonies.Domain.Intentions
{
    using System;
    using System.Collections.Generic;

    using Wacton.Colonies.Domain.Measures;
    using Wacton.Colonies.Domain.Organisms;

    public class NourishLogic : InteractionIntentionLogic
    {
        public override Inventory AssociatedIntenvory => Inventory.Nutrient;
        public override Dictionary<EnvironmentMeasure, double> EnvironmentBias => new Dictionary<EnvironmentMeasure, double>
            {
                { EnvironmentMeasure.Sound, 25 },
                { EnvironmentMeasure.Pheromone, -25 },
                { EnvironmentMeasure.Damp, -10 },
                { EnvironmentMeasure.Heat, -10 },
                { EnvironmentMeasure.Disease, -50 }
            };

        public override bool CanPerformInteraction(IOrganismState organismState)
        {
            return OrganismHasNutrients(organismState);
        }

        public override IntentionAdjustments EffectsOfInteraction(IOrganismState organismState, IOrganismState otherOrganismState)
        {
            if (!this.CanPerformInteraction(organismState))
            {
                return new IntentionAdjustments();
            }

            var organismAdjustments = new Dictionary<OrganismMeasure, double>();
            var environmentAdjustments = new Dictionary<EnvironmentMeasure, double>();

            var availableNutrient = organismState.GetLevel(OrganismMeasure.Inventory);
            var desiredNutrient = 1 - otherOrganismState.GetLevel(OrganismMeasure.Health);
            var nutrientTaken = Math.Min(desiredNutrient, availableNutrient);

            organismAdjustments.Add(OrganismMeasure.Inventory, -nutrientTaken);

            return new IntentionAdjustments(organismAdjustments, environmentAdjustments);
        }

        private static bool OrganismHasNutrients(IOrganismState organismState)
        {
            return organismState.CurrentInventory.Equals(Inventory.Nutrient) && organismState.GetLevel(OrganismMeasure.Inventory) > 0.0;
        }
    }
}

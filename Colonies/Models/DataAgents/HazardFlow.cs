﻿namespace Wacton.Colonies.Models.DataAgents
{
    using System.Linq;

    using Wacton.Colonies.DataTypes;
    using Wacton.Colonies.DataTypes.Enums;
    using Wacton.Colonies.Extensions;
    using Wacton.Colonies.Logic;
    using Wacton.Colonies.Models.Interfaces;

    public class HazardFlow
    {
        private readonly EcosystemData ecosystemData;
        private readonly EcosystemRates ecosystemRates;
        private readonly Distributor distributor;
        private readonly IWeather weather;

        public HazardFlow(EcosystemData ecosystemData, EcosystemRates ecosystemRates, Distributor distributor, IWeather weather)
        {
            this.ecosystemData = ecosystemData;
            this.ecosystemRates = ecosystemRates;
            this.distributor = distributor;
            this.weather = weather;
        }

        public void Advance()
        {
            foreach (var environmentMeasureHazardRate in this.ecosystemRates.HazardRates)
            {
                var environmentMeasure = environmentMeasureHazardRate.Key;
                var hazardRate = environmentMeasureHazardRate.Value;

                var weatherBiasedSpreadRate = hazardRate.SpreadRate;
                var weatherBiasedRemoveRate = hazardRate.RemoveRate;
                var weatherBiasedAddRate = hazardRate.AddRate;

                var weatherTrigger = environmentMeasure.WeatherTrigger;
                if (weatherTrigger != WeatherType.None)
                {
                    var weatherLevel = this.weather.GetLevel(weatherTrigger);
                    weatherBiasedSpreadRate *= weatherLevel;
                    weatherBiasedRemoveRate *= (1 - weatherLevel);
                    weatherBiasedAddRate *= weatherLevel;
                }

                this.RandomlySpreadHazards(environmentMeasure, weatherBiasedSpreadRate);
                this.RandomlyRemoveHazards(environmentMeasure, weatherBiasedRemoveRate);
                this.RandomlyInsertHazards(environmentMeasure, weatherBiasedAddRate);
            }
        }

        private void RandomlyInsertHazards(EnvironmentMeasure environmentMeasure, double addChance)
        {
            if (!DecisionLogic.IsSuccessful(addChance))
            {
                return;
            }

            var hazardCoordinates = this.distributor.HazardSources(environmentMeasure).ToList();
            var nonHazardCoordinates = this.ecosystemData.AllCoordinates().Except(hazardCoordinates);
            var chosenNonHazardCoordinate = DecisionLogic.MakeDecision(nonHazardCoordinates);
            if (!this.ecosystemData.HasLevel(chosenNonHazardCoordinate, EnvironmentMeasure.Obstruction))
            {
                this.distributor.Insert(environmentMeasure, chosenNonHazardCoordinate);
            }
        }

        private void RandomlySpreadHazards(EnvironmentMeasure environmentMeasure, double spreadChance)
        {
            var hazardCoordinates = this.distributor.HazardSources(environmentMeasure).ToList();
            foreach (var hazardCoordinate in hazardCoordinates)
            {
                if (!DecisionLogic.IsSuccessful(spreadChance))
                {
                    continue;
                }

                var neighbouringCoordinates = this.ecosystemData.GetNeighbours(hazardCoordinate, 1, false, false).ToList();
                var validNeighbouringCoordinates = neighbouringCoordinates.Where(neighbourCoordinate =>
                    neighbourCoordinate != null
                    && !this.ecosystemData.HasLevel(neighbourCoordinate, EnvironmentMeasure.Obstruction)
                    && this.ecosystemData.GetLevel(neighbourCoordinate, environmentMeasure) < 1).ToList();

                if (validNeighbouringCoordinates.Count == 0)
                {
                    continue;
                }

                var chosenCoordinate = DecisionLogic.MakeDecision(validNeighbouringCoordinates);
                this.distributor.Insert(environmentMeasure, chosenCoordinate);
            }
        }

        private void RandomlyRemoveHazards(EnvironmentMeasure environmentMeasure, double removeChance)
        {
            var hazardCoordinates = this.distributor.HazardSources(environmentMeasure).ToList();
            foreach (var hazardCoordinate in hazardCoordinates)
            {
                if (!DecisionLogic.IsSuccessful(removeChance))
                {
                    continue;
                }

                this.distributor.Remove(environmentMeasure, hazardCoordinate);
            }
        }
    }
}
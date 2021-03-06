﻿namespace Wacton.Colonies.Domain.Settings
{
    using System.Collections.Generic;

    using Wacton.Colonies.Domain.Measures;

    public class EcosystemSettings : IEcosystemSettings
    {
        public Dictionary<IMeasure, double> IncreasingRates { get; }
        public Dictionary<IMeasure, double> DecreasingRates { get; }
        public Dictionary<EnvironmentMeasure, HazardRate> HazardRates { get; }

        public EcosystemSettings()
        {
            this.IncreasingRates = new Dictionary<IMeasure, double>
                {
                    { EnvironmentMeasure.Pheromone, 1 / 10.0 },
                    { EnvironmentMeasure.Nutrient, 1 / 500.0 },
                    { EnvironmentMeasure.Mineral, 1 / 100.0 }
                };

            this.DecreasingRates = new Dictionary<IMeasure, double>
                {
                    { OrganismMeasure.Health, 1 / 750.0 },
                    { EnvironmentMeasure.Pheromone, 1 / 300.0 },
                    { EnvironmentMeasure.Obstruction, 1 / 5.0 }
                };

            this.HazardRates = new Dictionary<EnvironmentMeasure, HazardRate>
                {
                    { EnvironmentMeasure.Damp, new HazardRate(1 / 2000.0, 1 / 500.0, 1 / 1000.0) },
                    { EnvironmentMeasure.Heat, new HazardRate(1 / 2000.0, 1 / 500.0, 1 / 1000.0) },
                    { EnvironmentMeasure.Disease, new HazardRate(0.0, 1 / 50.0, 1 / 50.0) }
                };
        }
    }
}

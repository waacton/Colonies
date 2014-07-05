﻿namespace Wacton.Colonies.Models.Interfaces
{
    using System.Collections.Generic;

    using Wacton.Colonies.DataTypes;
    using Wacton.Colonies.DataTypes.Enums;

    public interface IEcosystem : IBiased<OrganismMeasure>
    {
        int Width { get; }

        int Height { get; }

        double HealthDeteriorationRate { get; set; }

        double PheromoneDepositRate { get; set; }

        double PheromoneFadeRate { get; set; }

        double NutrientGrowthRate { get; set; }

        double MineralFormRate { get; set; }

        double ObstructionDemolishRate { get; set; }

        IWeather Weather { get; }

        HazardRate GetHazardRate(EnvironmentMeasure environmentMeasure);

        void SetHazardRate(EnvironmentMeasure environmentMeasure, HazardRate hazardChance);

        UpdateSummary Update();

        Dictionary<IOrganism, Coordinate> OrganismCoordinates();

        IEnumerable<Coordinate> PerformEnvironmentInteractions();

        IEnumerable<Coordinate> PerformMovementsActions();

        IEnumerable<Coordinate> PerformOrganismInteractions();

        IEnumerable<Coordinate> PerformEcosystemModifiers();
    }
}
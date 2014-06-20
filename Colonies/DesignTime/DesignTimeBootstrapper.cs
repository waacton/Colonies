﻿namespace Wacton.Colonies.DesignTime
{
    using System.Collections.Generic;
    using System.Windows.Media;

    using Wacton.Colonies.DataTypes;
    using Wacton.Colonies.DataTypes.Enums;
    using Wacton.Colonies.Models;
    using Wacton.Colonies.ViewModels;

    public class DesignTimeBootstrapper : Bootstrapper
    {
        private const int EcosystemWidth = 10;
        private const int EcosystemHeight = 10;

        public MainViewModel MainViewModel { get; private set; }

        public override void Run()
        {
            this.MainViewModel = this.BuildMainDataContext(EcosystemWidth, EcosystemHeight);
            this.MainViewModel.Refresh();
        }

        protected override void InitialiseTerrain(Ecosystem ecosystem)
        {
            ecosystem.SetLevel(new Coordinate(1, 1), EnvironmentMeasure.Obstruction, 1.0);
            ecosystem.SetLevel(new Coordinate(1, 1), EnvironmentMeasure.Sound, 0.5);
        }

        protected override Dictionary<Organism, Coordinate> InitialOrganismCoordinates()
        {
            var organismLocations = new Dictionary<Organism, Coordinate>
                                        {
                                            { new Gatherer("DesignTimeOrganism-01", Colors.Silver), new Coordinate(0, 0) },
                                            { new Gatherer("DesignTimeOrganism-02", Colors.Silver), new Coordinate(EcosystemWidth - 1, EcosystemHeight - 1) }
                                        };

            return organismLocations;
        }
    }
}

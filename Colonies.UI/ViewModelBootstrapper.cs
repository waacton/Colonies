﻿namespace Wacton.Colonies.UI
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Practices.Prism.PubSubEvents;

    using Wacton.Colonies.Domain.Core;
    using Wacton.Colonies.Domain.Mains;
    using Wacton.Colonies.UI.Ecosystems;
    using Wacton.Colonies.UI.Environments;
    using Wacton.Colonies.UI.Habitats;
    using Wacton.Colonies.UI.Mains;
    using Wacton.Colonies.UI.Organisms;
    using Wacton.Colonies.UI.OrganismSynopses;
    using Wacton.Colonies.UI.Settings;

    public class ViewModelBootstrapper
    {
        public MainViewModel BuildViewModel(Main domainModel)
        {
            var ecosystemWidth = domainModel.Ecosystem.Width;
            var ecosystemHeight = domainModel.Ecosystem.Height;

            // the event aggregator might be used by view models to inform of changes
            var eventaggregator = new EventAggregator();

            var habitatViewModels = new List<List<HabitatViewModel>>();

            for (var x = 0; x < ecosystemWidth; x++)
            {
                habitatViewModels.Add(new List<HabitatViewModel>());

                for (var y = 0; y < ecosystemHeight; y++)
                {
                    var habitat = domainModel.Ecosystem.HabitatAt(new Coordinate(x, y));

                    var environmentViewModel = new EnvironmentViewModel(habitat.Environment, eventaggregator);
                    var habitatViewModel = new HabitatViewModel(habitat, environmentViewModel, eventaggregator);
                    habitatViewModels[x].Add(habitatViewModel);
                }
            }

            var settingsViewModel = new SettingsViewModel(domainModel.Ecosystem.EcosystemSettings, eventaggregator);

            var ecosystemViewModel = new EcosystemViewModel(domainModel.Ecosystem, habitatViewModels, eventaggregator);

            // hook organism model into the organism synopsis
            var organismSynopsis = domainModel.OrganismSynopsis;
            var organismViewModels = organismSynopsis.Organisms.Select(organism => new OrganismViewModel(organism, eventaggregator)).ToList();
            var organismSynopsisViewModel = new OrganismSynopsisViewModel(organismSynopsis, organismViewModels, eventaggregator);

            var mainViewModel = new MainViewModel(domainModel, settingsViewModel, ecosystemViewModel, organismSynopsisViewModel, eventaggregator);
            return mainViewModel;
        }
    }
}

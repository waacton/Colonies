namespace Wacton.Colonies.UI.OrganismSynopses
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;

    using Microsoft.Practices.Prism.PubSubEvents;

    using Wacton.Colonies.Domain.Organisms;
    using Wacton.Colonies.Domain.OrganismSynopses;
    using Wacton.Colonies.UI.Infrastructure;
    using Wacton.Colonies.UI.Organisms;

    public class OrganismSynopsisViewModel : ViewModelBase<IOrganismSynopsis>
    {
        // ObservableCollection seems to be required because this is bound to by an ItemSource
        private ObservableCollection<OrganismViewModel> organismViewModels;
        public ObservableCollection<OrganismViewModel> OrganismViewModels
        {
            get
            {
                return this.organismViewModels;
            }
            set
            {
                this.organismViewModels = value;
                this.OnPropertyChanged(nameof(this.OrganismViewModels));
            }
        }

        public OrganismSynopsisViewModel(IOrganismSynopsis domainModel, List<OrganismViewModel> organismViewModels, IEventAggregator eventAggregator)
            : base(domainModel, eventAggregator)
        {
            this.OrganismViewModels = new ObservableCollection<OrganismViewModel>(organismViewModels);
        }

        public void AddOrganism(IOrganism organism)
        {
            this.DomainModel.Organisms.Add(organism);
            var updateOrganismViewModelsAction = new Action(() => this.organismViewModels.Add(new OrganismViewModel(organism, this.EventAggregator)));
            Application.Current.Dispatcher.Invoke(updateOrganismViewModelsAction);
        }

        public override void Refresh()
        {
            // refresh all child view models (each organism)
            foreach (var organismViewModel in this.OrganismViewModels)
            {
                organismViewModel.Refresh();
            }
        }
    }
}

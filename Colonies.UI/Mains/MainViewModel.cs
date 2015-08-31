namespace Wacton.Colonies.UI.Mains
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media;

    using Microsoft.Practices.Prism.PubSubEvents;

    using Wacton.Colonies.Domain.Ecosystems.Phases;
    using Wacton.Colonies.Domain.Mains;
    using Wacton.Colonies.Domain.Measures;
    using Wacton.Colonies.Domain.Weathers;
    using Wacton.Colonies.UI.Ecosystems;
    using Wacton.Colonies.UI.Environments;
    using Wacton.Colonies.UI.Habitats;
    using Wacton.Colonies.UI.Infrastructure;
    using Wacton.Colonies.UI.OrganismSynopses;
    using Wacton.Colonies.UI.Properties;

    // TODO: stop breaking the law of demeter so badly :(
    public class MainViewModel : ViewModelBase<IMain>
    {
        // if the timer interval is too small, the model update won't have finished
        // so use a lock to ensure the model isn't updated while it's updating...
        // (volatile because, if interval update is too small, lock will be accessed by multiple threads simultaneously)
        private readonly Timer ecosystemTimer;
        private volatile object performPhaseLock = new object();

        public ICommand ToggleEcosystemCommand { get; set; }
        public ICommand IncreaseTurnIntervalCommand { get; set; }
        public ICommand DecreaseTurnIntervalCommand { get; set; }

        private EcosystemViewModel ecosystemViewModel;
        public EcosystemViewModel EcosystemViewModel
        {
            get
            {
                return this.ecosystemViewModel;
            }
            set
            {
                this.ecosystemViewModel = value;
                this.OnPropertyChanged("EcosystemViewModel");
            }
        }

        private OrganismSynopsisViewModel organismSynopsisViewModel;
        public OrganismSynopsisViewModel OrganismSynopsisViewModel
        {
            get
            {
                return this.organismSynopsisViewModel;
            }
            set
            {
                this.organismSynopsisViewModel = value;
                this.OnPropertyChanged("OrganismSynopsisViewModel");
            }
        }

        private bool isEcosystemActive;
        public bool IsEcosystemActive
        {
            get
            {
                return this.isEcosystemActive;
            }
            set
            {
                this.isEcosystemActive = value;
                this.OnPropertyChanged("IsEcosystemActive");

                // if the ecosystem turns on/off the timer needs to start/stop 
                this.ChangeEcosystemTimer();
            }
        }

        // TODO: should the slider go from "slow (1) -> fast (100)", and that value be converted in this view model to a ms value?
        // phase interval is in ms
        private int desiredPhaseInterval;
        public int DesiredPhaseInterval
        {
            get
            {
                return this.desiredPhaseInterval;
            }
            set
            {
                this.desiredPhaseInterval = value;
                this.OnPropertyChanged("DesiredPhaseInterval");
            }
        }

        private int previousDesiredPhaseInterval;

        public double HealthDeteriorationDemoninator
        {
            get
            {
                return 1 / this.DomainModel.Ecosystem.EcosystemRates.DecreasingRates[OrganismMeasure.Health];
            }
            set
            {
                this.DomainModel.Ecosystem.EcosystemRates.DecreasingRates[OrganismMeasure.Health] = 1 / value;
                this.OnPropertyChanged("HealthDeteriorationDemoninator");
            }
        }

        public double PheromoneDepositDemoninator
        {
            get
            {
                return 1 / this.DomainModel.Ecosystem.EcosystemRates.IncreasingRates[EnvironmentMeasure.Pheromone];
            }
            set
            {
                this.DomainModel.Ecosystem.EcosystemRates.IncreasingRates[EnvironmentMeasure.Pheromone] = 1 / value;
                this.OnPropertyChanged("PheromoneDepositDemoninator");
            }
        }

        public double PheromoneFadeDemoninator
        {
            get
            {
                return 1 / this.DomainModel.Ecosystem.EcosystemRates.DecreasingRates[EnvironmentMeasure.Pheromone];
            }
            set
            {
                this.DomainModel.Ecosystem.EcosystemRates.DecreasingRates[EnvironmentMeasure.Pheromone] = 1 / value;
                this.OnPropertyChanged("PheromoneFadeDemoninator");
            }
        }

        public double NutrientGrowthDemoninator
        {
            get
            {
                return 1 / this.DomainModel.Ecosystem.EcosystemRates.IncreasingRates[EnvironmentMeasure.Nutrient];
            }
            set
            {
                this.DomainModel.Ecosystem.EcosystemRates.IncreasingRates[EnvironmentMeasure.Nutrient] = 1 / value;
                this.OnPropertyChanged("NutrientGrowthRate");
            }
        }

        public double MineralFormDemoninator
        {
            get
            {
                return 1 / this.DomainModel.Ecosystem.EcosystemRates.IncreasingRates[EnvironmentMeasure.Nutrient];
            }
            set
            {
                this.DomainModel.Ecosystem.EcosystemRates.IncreasingRates[EnvironmentMeasure.Nutrient] = 1 / value;
                this.OnPropertyChanged("MineralFormRate");
            }
        }

        public double ObstructionDemolishDenominator
        {
            get
            {
                return 1 / this.DomainModel.Ecosystem.EcosystemRates.DecreasingRates[EnvironmentMeasure.Obstruction];
            }
            set
            {
                this.DomainModel.Ecosystem.EcosystemRates.DecreasingRates[EnvironmentMeasure.Obstruction] = 1 / value;
                this.OnPropertyChanged("ObstructionDemolishRate");
            }
        }

        // TODO: needs add, spread, remove for damp, heat, disease...
        public double DampSpreadDenominator
        {
            get
            {
                return 1 / this.DomainModel.Ecosystem.EcosystemRates.HazardRates[EnvironmentMeasure.Damp].SpreadRate;
            }
            set
            {
                var currentHazardRate = this.DomainModel.Ecosystem.EcosystemRates.HazardRates[EnvironmentMeasure.Damp];
                var updatedHazardRate = new HazardRate(currentHazardRate.AddRate, 1 / value, currentHazardRate.RemoveRate);
                this.DomainModel.Ecosystem.EcosystemRates.HazardRates[EnvironmentMeasure.Damp] = updatedHazardRate;
                this.OnPropertyChanged("DampSpreadDenominator");
            }
        }

        public static Color PheromoneColor { get { return EnvironmentViewModel.MeasureColors[EnvironmentMeasure.Pheromone]; } }
        public static Color NutrientColor { get { return EnvironmentViewModel.MeasureColors[EnvironmentMeasure.Nutrient]; } }
        public static Color MineralColor { get { return EnvironmentViewModel.MeasureColors[EnvironmentMeasure.Mineral]; } }
        public static Color ObstructionColor { get { return EnvironmentViewModel.MeasureColors[EnvironmentMeasure.Obstruction]; } }
        public static Color SoundColor { get { return EnvironmentViewModel.MeasureColors[EnvironmentMeasure.Sound]; } }
        public static Color DampColor { get { return EnvironmentViewModel.MeasureColors[EnvironmentMeasure.Damp]; } }
        public static Color HeatColor { get { return EnvironmentViewModel.MeasureColors[EnvironmentMeasure.Heat]; } }

        private string weatherDampLevel;
        public string WeatherDampLevel
        {
            get
            {
                return this.weatherDampLevel;
            }
            set
            {
                this.weatherDampLevel = value;
                this.OnPropertyChanged("WeatherDampLevel");
            }
        }

        private string weatherHeatLevel;
        public string WeatherHeatLevel
        {
            get
            {
                return this.weatherHeatLevel;
            }
            set
            {
                this.weatherHeatLevel = value;
                this.OnPropertyChanged("WeatherHeatLevel");
            }
        }

        private int roundCount;
        public int RoundCount
        {
            get
            {
                return this.roundCount;
            }
            private set
            {
                this.roundCount = value;
                this.OnPropertyChanged("RoundCount");
            }
        }

        private int phaseCount;
        public int PhaseCount
        {
            get
            {
                return this.phaseCount;
            }
            private set
            {
                this.phaseCount = value;
                this.OnPropertyChanged("PhaseCount");
            }
        }

        private int phaseDuration;
        public int PhaseDuration
        {
            get
            {
                return this.phaseDuration;
            }
            private set
            {
                this.phaseDuration = value;
                this.OnPropertyChanged("PhaseDuration");
            }
        }

        private int roundDuration;
        public int RoundDuration
        {
            get
            {
                return this.roundDuration;
            }
            private set
            {
                this.roundDuration = value;
                this.OnPropertyChanged("RoundDuration");
            }
        }

        private DateTime previousPhaseStartTime = DateTime.MinValue;
        private DateTime previousRoundStartTime = DateTime.MinValue;

        public MainViewModel(IMain domainModel, EcosystemViewModel ecosystemViewModel, OrganismSynopsisViewModel organismSynopsisViewModel, IEventAggregator eventAggregator)
            : base(domainModel, eventAggregator)
        {
            this.EcosystemViewModel = ecosystemViewModel;
            this.OrganismSynopsisViewModel = organismSynopsisViewModel;

            // initally set the ecosystem up to be not running
            this.RoundCount = 0;
            this.PhaseCount = 0;
            this.ecosystemTimer = new Timer(this.OnEcosystemTimerTick);
            this.IsEcosystemActive = false;
            var initialDesiredPhaseInterval = Settings.Default.PhaseIntervalInMs;
            this.DesiredPhaseInterval = initialDesiredPhaseInterval;
            this.previousDesiredPhaseInterval = initialDesiredPhaseInterval;
            this.PhaseDuration = 0;
            this.RoundDuration = 0;

            // hook up a toggle ecosystem command so a keyboard shortcut can be used to toggle the ecosystem on/off
            this.ToggleEcosystemCommand = new RelayCommand(this.ToggleEcosystem);
            this.IncreaseTurnIntervalCommand = new RelayCommand(this.IncreasePhaseInterval);
            this.DecreaseTurnIntervalCommand = new RelayCommand(this.DecreasePhaseInterval);
        }

        private void ToggleEcosystem(object obj)
        {
            this.IsEcosystemActive = !this.IsEcosystemActive;
        }

        // TODO: bind slider max and min to these values
        private void IncreasePhaseInterval(object obj)
        {
            this.DesiredPhaseInterval++;
            if (this.DesiredPhaseInterval > 2000)
            {
                this.DesiredPhaseInterval = 2000;
            }
        }

        private void DecreasePhaseInterval(object obj)
        {
            this.DesiredPhaseInterval--;
            if (this.DesiredPhaseInterval < 1)
            {
                this.DesiredPhaseInterval = 1;
            }
        }

        private void ChangeEcosystemTimer()
        {
            const int ImmediateStart = 0;
            const int PreventStart = Timeout.Infinite;

            this.ecosystemTimer.Change(this.IsEcosystemActive ? this.DesiredPhaseInterval : PreventStart, this.DesiredPhaseInterval);
            this.previousDesiredPhaseInterval = this.DesiredPhaseInterval;
        }

        private void OnEcosystemTimerTick(object state)
        {
            if (Monitor.TryEnter(this.performPhaseLock))
            {
                try
                {
                    var phaseSummary = this.DomainModel.PerformPhase();
                    this.UpdateViewModels(phaseSummary);
                    this.PhaseCount = phaseSummary.PhaseNumber;

                    // TODO: only do these after all phases have been performed?
                    var previousRoundCount = this.RoundCount;
                    this.RoundCount = this.PhaseCount / phaseSummary.PhasesPerRound;
                    this.WeatherDampLevel = string.Format("{0:0.0000}", this.DomainModel.Ecosystem.Weather.GetLevel(WeatherType.Damp));
                    this.WeatherHeatLevel = string.Format("{0:0.0000}", this.DomainModel.Ecosystem.Weather.GetLevel(WeatherType.Heat));

                    // if there's been a change in the phase interval while the previous phase was processed
                    // update the interval of the ecosystem timer
                    if (this.DesiredPhaseInterval != this.previousDesiredPhaseInterval)
                    {
                        this.ChangeEcosystemTimer();
                    }

                    this.CalculateDuration(previousRoundCount);
                }
                finally
                {
                    Monitor.Exit(this.performPhaseLock);
                }
            }
        }

        private void CalculateDuration(int previousRoundCount)
        {
            if (this.previousPhaseStartTime.Equals(DateTime.MinValue))
            {
                this.previousPhaseStartTime = DateTime.Now;
            }
            else
            {
                var phaseStartTime = DateTime.Now;
                this.PhaseDuration = (int)(phaseStartTime - this.previousPhaseStartTime).TotalMilliseconds;
                this.previousPhaseStartTime = phaseStartTime;
            }

            if (this.previousRoundStartTime.Equals(DateTime.MinValue))
            {
                this.previousRoundStartTime = DateTime.Now;
            }
            else
            {
                if (this.RoundCount > previousRoundCount)
                {
                    var roundStartTime = DateTime.Now;
                    this.RoundDuration = (int)(roundStartTime - this.previousRoundStartTime).TotalMilliseconds;
                    this.previousRoundStartTime = roundStartTime;
                }
            }
        }

        private void UpdateViewModels(PhaseSummary phaseSummary)
        {
            var updatedHabitatViewModels = new ConcurrentBag<HabitatViewModel>();

            // update properties of all modifications
            Parallel.ForEach(phaseSummary.EcosystemHistory.Modifications, modification =>
            {
                var x = modification.Coordinate.X;
                var y = modification.Coordinate.Y;

                var habitatViewModel = this.EcosystemViewModel.HabitatViewModels[x][y];
                updatedHabitatViewModels.Add(habitatViewModel);
            });

            // update properties of all organisms that have been added
            Parallel.ForEach(phaseSummary.EcosystemHistory.Additions, addition =>
            {
                var x = addition.Coordinate.X;
                var y = addition.Coordinate.Y;

                var habitatViewModel = this.EcosystemViewModel.HabitatViewModels[x][y];
                habitatViewModel.AssignOrganismModel(addition.Organism);
                updatedHabitatViewModels.Add(habitatViewModel);

                this.OrganismSynopsisViewModel.AddOrganism(addition.Organism);
            });

            // update properties of all organisms that have not moved
            Parallel.ForEach(phaseSummary.OrganismCoordinates, organismCoordinate =>
            {
                var organism = organismCoordinate.Key;

                if (phaseSummary.EcosystemHistory.Relocations.Any(relocation => relocation.Organism.Equals(organism)))
                {
                    return;
                }

                var x = organismCoordinate.Value.X;
                var y = organismCoordinate.Value.Y;

                var habitatViewModel = this.EcosystemViewModel.HabitatViewModels[x][y];
                updatedHabitatViewModels.Add(habitatViewModel);
            });

            // unassign moving organisms from their previous view models
            Parallel.ForEach(phaseSummary.EcosystemHistory.Relocations, relocation =>
            {
                var x = relocation.PreviousCoordinate.X;
                var y = relocation.PreviousCoordinate.Y;

                var habitatViewModel = this.EcosystemViewModel.HabitatViewModels[x][y];
                habitatViewModel.UnassignOrganismModel();
                updatedHabitatViewModels.Add(habitatViewModel);
            });

            // assign moving organisms to their current view models
            Parallel.ForEach(phaseSummary.EcosystemHistory.Relocations, relocation =>
            {
                var x = relocation.UpdatedCoordinate.X;
                var y = relocation.UpdatedCoordinate.Y;

                var habitatViewModel = this.EcosystemViewModel.HabitatViewModels[x][y];
                habitatViewModel.AssignOrganismModel(relocation.Organism);
                updatedHabitatViewModels.Add(habitatViewModel);
            });

            // refresh each distinct habitat view model that has been updated
            Parallel.ForEach(updatedHabitatViewModels.Distinct(), habitatViewModel => habitatViewModel.Refresh());

            this.EcosystemViewModel.RefreshWeatherColor();

            // refresh organism synopsis
            this.OrganismSynopsisViewModel.Refresh();
        }

        public override void Refresh()
        {
            // refresh all child view models (ecosystem and organism synopsis)
            this.EcosystemViewModel.Refresh();
            this.OrganismSynopsisViewModel.Refresh();
        }
    }
}

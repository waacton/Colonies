﻿namespace Wacton.Colonies.Domain.Organisms
{
    using System;
    using System.Windows.Media;

    using Wacton.Colonies.Domain.Intentions;
    using Wacton.Colonies.Domain.Measures;

    public interface IOrganism : IOrganismState, IBiased<EnvironmentMeasure>
    {
        Guid ColonyId { get; }

        string Name { get; }

        Color Color { get; }

        string Description { get; }

        double Age { get; }

        void IncrementAge(double increment);

        Intention DecideIntention(IMeasurable<EnvironmentMeasure> measurableEnvironment);

        void UpdateIntention(Intention intention);

        void OverloadPheromone();

        void OverloadSound();

        void ContractDisease();

        bool CanAct(IMeasurable<EnvironmentMeasure> measurableEnvironment);

        IntentionAdjustments ActionEffects(IMeasurable<EnvironmentMeasure> measurableEnvironment);

        bool CanInteract();

        IntentionAdjustments InteractionEffects(IOrganismState otherOrganismState);
    }
}
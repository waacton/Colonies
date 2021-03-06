﻿namespace Wacton.Colonies.Domain.Environments
{
    using System.Collections.Generic;

    using Wacton.Colonies.Domain.Measures;

    public interface IEnvironment : IMeasurable<EnvironmentMeasure>
    {
        bool IsHarmful { get; }

        IEnumerable<EnvironmentMeasure> HarmfulMeasures { get; }
    }
}
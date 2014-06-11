﻿namespace Wacton.Colonies.DataTypes.Enums
{
    using System.Collections.Generic;
    using System.Linq;

    using Wacton.Colonies.DataTypes.Interfaces;

    public class EnvironmentMeasure : Enumeration, IMeasure
    {
        public static readonly EnvironmentMeasure Pheromone = new EnvironmentMeasure(0, "Pheromone", WeatherType.None);
        public static readonly EnvironmentMeasure Nutrient = new EnvironmentMeasure(1, "Nutrient", WeatherType.None);
        public static readonly EnvironmentMeasure Mineral = new EnvironmentMeasure(2, "Mineral", WeatherType.None);
        public static readonly EnvironmentMeasure Damp = new EnvironmentMeasure(3, "Damp", WeatherType.Damp);
        public static readonly EnvironmentMeasure Heat = new EnvironmentMeasure(4, "Heat", WeatherType.Heat);
        public static readonly EnvironmentMeasure Poison = new EnvironmentMeasure(5, "Poison", WeatherType.None);
        public static readonly EnvironmentMeasure Obstruction = new EnvironmentMeasure(6, "Obstruction", WeatherType.None);
        public static readonly EnvironmentMeasure Sound = new EnvironmentMeasure(7, "Sound", WeatherType.None);

        public static IEnumerable<EnvironmentMeasure> HazardousMeasures()
        {
            return new List<EnvironmentMeasure> { Heat, Damp, Poison };
        }

        public static IEnumerable<EnvironmentMeasure> TransportableMeasures()
        {
            return new List<EnvironmentMeasure> { Nutrient, Mineral };
        }

        public bool IsHazardous
        {
            get
            {
                return HazardousMeasures().Contains(this);
            }
        }

        public bool IsTransportable
        {
            get
            {
                return TransportableMeasures().Contains(this);
            }
        }

        public WeatherType WeatherTrigger { get; private set; }

        private EnvironmentMeasure(int value, string friendlyString, WeatherType weatherTrigger)
            : base(value, friendlyString)
        {
            this.WeatherTrigger = weatherTrigger;
        }
    }
}

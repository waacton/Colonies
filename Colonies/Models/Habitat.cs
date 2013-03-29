﻿namespace Colonies.Models
{
    using System;

    public sealed class Habitat
    {
        public Environment Environment { get; set; }
        public Organism Organism { get; set; }

        public Habitat(Environment environment, Organism organism)
        {
            this.Environment = environment;
            this.Organism = organism;
        }

        public override String ToString()
        {
            return String.Format("{0}, {1}", this.Environment, this.Organism);
        }
    }
}
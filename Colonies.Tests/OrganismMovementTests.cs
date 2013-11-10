﻿namespace Wacton.Colonies.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;

    using NUnit.Framework;

    using Wacton.Colonies.Ancillary;
    using Wacton.Colonies.Logic;
    using Wacton.Colonies.Models;

    [TestFixture]
    public class OrganismMovementTests
    {
        private Habitat[,] habitats;
        private Dictionary<string, Organism> organismsById;
        private Dictionary<Habitat, Coordinate> habitatCoordinates;

        [SetUp]
        public void SetupTest()
        {
            const int width = 10;
            const int height = 1;
            this.habitats = GenerateBaseHabitats(width, height);

            var organismIdentifiers = new List<string> { "A", "B", "C", "D", "W", "X", "Y", "Z" };

            this.organismsById = new Dictionary<string, Organism>();
            foreach (var organismIdentifier in organismIdentifiers)
            {
                this.organismsById.Add(organismIdentifier, new Organism(organismIdentifier, Colors.Black));
            }

            this.habitatCoordinates = new Dictionary<Habitat, Coordinate>();
            for (var i = 0; i < this.habitats.GetLength(0); i++)
            {
                for (var j = 0; j < this.habitats.GetLength(1); j++)
                {
                    this.habitatCoordinates.Add(this.habitats[i, j], new Coordinate(i, j));
                }
            }
        }

        [Test]
        public void IndependentMovements()
        {
            /* take a grid and populate with organisms: |___|_A_|___|_B_|___|___|___|___|___|___|
             * make A choose rightmost stimulus & make B choose leftmost stimulus
             * no conflict, both organisms should move where they chose to go
             * result of test:                          |_A_|___|___|___|_B_|___|___|___|___|___| */

            var organismHabitats = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[1, 0] },
                                            { this.organismsById["B"], this.habitats[3, 0] }
                                        };

            var organismIntendedDestinations = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[0, 0] },
                                            { this.organismsById["B"], this.habitats[4, 0] }
                                        };

            var expectedOrganismDestinations = organismIntendedDestinations.ToDictionary(
                    intendedDestination => intendedDestination.Key, 
                    intendedDestination => intendedDestination.Value);

            var updateSummary = this.CreateAndUpdateEcosystem(organismHabitats, organismIntendedDestinations);

            var expectedCoordinates = expectedOrganismDestinations.Values.Select(expectedDestination => this.habitatCoordinates[expectedDestination]).ToList();
            var actualCoordinates = updateSummary.CurrentOrganismCoordinates.Values.ToList();
            Assert.AreEqual(actualCoordinates, expectedCoordinates);
        }

        [Test]
        public void IndividualConflictingMovements()
        {
            /* take a grid and populate with organisms: |___|_B_|___|_A_|___|___|___|___|___|___|
             * make A choose leftmost stimulus & make B choose rightmost stimulus
             * make the ecosystem choose the first organism when >1 organism want to move to the same space
             * (therefore, when A wants to move left and B wants to move right, A will win)
             * result of test:                          |___|_B_|_A_|___|___|___|___|___|___|___| */

            var organismHabitats = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[3, 0] },
                                            { this.organismsById["B"], this.habitats[1, 0] }
                                        };

            var organismIntendedDestinations = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[2, 0] },
                                            { this.organismsById["B"], this.habitats[2, 0] }
                                        };

            var expectedOrganismDestinations = organismIntendedDestinations.ToDictionary(
                    intendedDestination => intendedDestination.Key,
                    intendedDestination => intendedDestination.Value);

            // expect B to not have moved
            expectedOrganismDestinations[this.organismsById["B"]] = organismHabitats[this.organismsById["B"]];

            var updateSummary = this.CreateAndUpdateEcosystem(organismHabitats, organismIntendedDestinations);

            var expectedCoordinates = expectedOrganismDestinations.Values.Select(expectedDestination => this.habitatCoordinates[expectedDestination]).ToList();
            var actualCoordinates = updateSummary.CurrentOrganismCoordinates.Values.ToList();
            Assert.AreEqual(actualCoordinates, expectedCoordinates);
        }

        [Test]
        public void MultipleConflictingMovements()
        {
            /* take a grid and populate with organisms: |___|_B_|___|_A_|___|___|_Y_|___|_Z_|___|
             * make A choose leftmost stimulus & make B choose rightmost stimulus
             * make Y choose rightmost stimulus & make Z choose leftmost stimulus 
             * make the ecosystem choose the first organism when >1 organism want to move to the same space
             * (therefore, when A wants to move left and B wants to move right, A will win)
             * (therefore, when Y wants to move right and Z wants to move left, Y will win)
             * result of test:                          |___|_B_|_A_|___|___|___|___|_Y_|_Z_|___| */

            var organismHabitats = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[3, 0] },
                                            { this.organismsById["B"], this.habitats[1, 0] },
                                            { this.organismsById["Y"], this.habitats[6, 0] },
                                            { this.organismsById["Z"], this.habitats[0, 0] }
                                        };

            var organismIntendedDestinations = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[2, 0] },
                                            { this.organismsById["B"], this.habitats[2, 0] },
                                            { this.organismsById["Y"], this.habitats[7, 0] },
                                            { this.organismsById["Z"], this.habitats[7, 0] }
                                        };

            var expectedOrganismDestinations = organismIntendedDestinations.ToDictionary(
                    intendedDestination => intendedDestination.Key,
                    intendedDestination => intendedDestination.Value);

            // expect B, Z to not have moved
            expectedOrganismDestinations[this.organismsById["B"]] = organismHabitats[this.organismsById["B"]];
            expectedOrganismDestinations[this.organismsById["Z"]] = organismHabitats[this.organismsById["Z"]];

            var updateSummary = this.CreateAndUpdateEcosystem(organismHabitats, organismIntendedDestinations);

            var expectedCoordinates = expectedOrganismDestinations.Values.Select(expectedDestination => this.habitatCoordinates[expectedDestination]).ToList();
            var actualCoordinates = updateSummary.CurrentOrganismCoordinates.Values.ToList();
            Assert.AreEqual(actualCoordinates, expectedCoordinates);
        }

        [Test]
        public void IndividualTrailingMovements()
        {
            /* take a grid and populate with organisms: |_A_|_B_|_C_|_D_|___|___|___|___|___|___|
             * make A, B, C, D choose rightmost stimulus
             * all organisms are moving in convoy to the right, and each will be able to go to their desired destination
             * result of test:                          |___|_A_|_B_|_C_|_D_|___|___|___|___|___| */

            var organismHabitats = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[0, 0] },
                                            { this.organismsById["B"], this.habitats[1, 0] },
                                            { this.organismsById["C"], this.habitats[2, 0] },
                                            { this.organismsById["D"], this.habitats[3, 0] }
                                        };

            var organismIntendedDestinations = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[1, 0] },
                                            { this.organismsById["B"], this.habitats[2, 0] },
                                            { this.organismsById["C"], this.habitats[3, 0] },
                                            { this.organismsById["D"], this.habitats[4, 0] }
                                        };

            var expectedOrganismDestinations = organismIntendedDestinations.ToDictionary(
                    intendedDestination => intendedDestination.Key,
                    intendedDestination => intendedDestination.Value);

            var updateSummary = this.CreateAndUpdateEcosystem(organismHabitats, organismIntendedDestinations);

            var expectedCoordinates = expectedOrganismDestinations.Values.Select(expectedDestination => this.habitatCoordinates[expectedDestination]).ToList();
            var actualCoordinates = updateSummary.CurrentOrganismCoordinates.Values.ToList();
            Assert.AreEqual(actualCoordinates, expectedCoordinates);
        }

        [Test]
        public void MultipleTrailingMovements()
        {
            /* take a grid and populate with organisms: |_A_|_B_|_C_|_D_|___|_W_|_X_|_Y_|_Z_|___|
             * make A, B, C, D choose rightmost stimulus
             * make W, X, Y, Z choose rightmost stimulus
             * all organisms are moving in convoy to the right, and each will be able to go to their desired destination
             * result of test:                          |___|_A_|_B_|_C_|_D_|___|_W_|_X_|_Y_|_Z_| */

            var organismHabitats = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[0, 0] },
                                            { this.organismsById["B"], this.habitats[1, 0] },
                                            { this.organismsById["C"], this.habitats[2, 0] },
                                            { this.organismsById["D"], this.habitats[3, 0] },
                                            { this.organismsById["W"], this.habitats[5, 0] },
                                            { this.organismsById["X"], this.habitats[6, 0] },
                                            { this.organismsById["Y"], this.habitats[7, 0] },
                                            { this.organismsById["Z"], this.habitats[8, 0] }
                                        };

            var organismIntendedDestinations = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[1, 0] },
                                            { this.organismsById["B"], this.habitats[2, 0] },
                                            { this.organismsById["C"], this.habitats[3, 0] },
                                            { this.organismsById["D"], this.habitats[4, 0] },
                                            { this.organismsById["W"], this.habitats[6, 0] },
                                            { this.organismsById["X"], this.habitats[7, 0] },
                                            { this.organismsById["Y"], this.habitats[8, 0] },
                                            { this.organismsById["Z"], this.habitats[9, 0] }
                                        };

            var expectedOrganismDestinations = organismIntendedDestinations.ToDictionary(
                    intendedDestination => intendedDestination.Key,
                    intendedDestination => intendedDestination.Value);

            var updateSummary = this.CreateAndUpdateEcosystem(organismHabitats, organismIntendedDestinations);

            var expectedCoordinates = expectedOrganismDestinations.Values.Select(expectedDestination => this.habitatCoordinates[expectedDestination]).ToList();
            var actualCoordinates = updateSummary.CurrentOrganismCoordinates.Values.ToList();
            Assert.AreEqual(actualCoordinates, expectedCoordinates);
        }

        [Test]
        public void IndividualConflictingAndTrailingMovements()
        {
            /* take a grid and populate with organisms: |_A_|_B_|___|_C_|_D_|___|___|___|___|___|
             * make A, B choose rightmost stimulus & make C, D choose leftmost stimulus
             * make the ecosystem choose the first organism when >1 organism want to move to the same space
             * (therefore, when B wants to move right and C wants to move left, B will win)
             * A is trailing B, and will be able to move when B wins the vacant destination
             * result of test:                          |___|_A_|_B_|_C_|_D_|___|___|___|___|___| */

            var organismHabitats = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[0, 0] },
                                            { this.organismsById["B"], this.habitats[1, 0] },
                                            { this.organismsById["C"], this.habitats[3, 0] },
                                            { this.organismsById["D"], this.habitats[4, 0] }
                                        };

            var organismIntendedDestinations = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[1, 0] },
                                            { this.organismsById["B"], this.habitats[2, 0] },
                                            { this.organismsById["C"], this.habitats[2, 0] },
                                            { this.organismsById["D"], this.habitats[3, 0] }
                                        };

            var expectedOrganismDestinations = organismIntendedDestinations.ToDictionary(
                    intendedDestination => intendedDestination.Key,
                    intendedDestination => intendedDestination.Value);

            // expect C, D to not have moved
            expectedOrganismDestinations[this.organismsById["C"]] = organismHabitats[this.organismsById["C"]];
            expectedOrganismDestinations[this.organismsById["D"]] = organismHabitats[this.organismsById["D"]];

            var updateSummary = this.CreateAndUpdateEcosystem(organismHabitats, organismIntendedDestinations);

            var expectedCoordinates = expectedOrganismDestinations.Values.Select(expectedDestination => this.habitatCoordinates[expectedDestination]).ToList();
            var actualCoordinates = updateSummary.CurrentOrganismCoordinates.Values.ToList();
            Assert.AreEqual(actualCoordinates, expectedCoordinates);
        }

        [Test]
        public void MultipleConflictingAndTrailingMovements()
        {
            /* take a grid and populate with organisms: |_A_|_B_|___|_C_|_D_|_X_|_Y_|___|_W_|_Z_|
             * make A, B choose rightmost stimulus & make C, D choose leftmost stimulus
             * make X, Y choose rightmost stimulus & make W, Z choose leftmost stimulus
             * make the ecosystem choose the first organism when >1 organism want to move to the same space
             * (therefore, when B wants to move right and C wants to move left, B will win)
             * (therefore, when W wants to move left and Y wants to move right, W will win) 
             * A is trailing B, and will be able to move when B wins the vacant destination
             * Z is trailing W, and will be able to move when W wins the vacant destination 
             * result of test:                          |___|_A_|_B_|_C_|_D_|_X_|_Y_|_W_|_Z_|___| */

            var organismHabitats = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[0, 0] },
                                            { this.organismsById["B"], this.habitats[1, 0] },
                                            { this.organismsById["C"], this.habitats[3, 0] },
                                            { this.organismsById["D"], this.habitats[4, 0] },
                                            { this.organismsById["W"], this.habitats[8, 0] },
                                            { this.organismsById["X"], this.habitats[5, 0] },
                                            { this.organismsById["Y"], this.habitats[6, 0] },
                                            { this.organismsById["Z"], this.habitats[9, 0] }
                                        };

            var organismIntendedDestinations = new Dictionary<Organism, Habitat>
                                        {
                                            { this.organismsById["A"], this.habitats[1, 0] },
                                            { this.organismsById["B"], this.habitats[2, 0] },
                                            { this.organismsById["C"], this.habitats[2, 0] },
                                            { this.organismsById["D"], this.habitats[3, 0] },
                                            { this.organismsById["W"], this.habitats[7, 0] },
                                            { this.organismsById["X"], this.habitats[6, 0] },
                                            { this.organismsById["Y"], this.habitats[7, 0] },
                                            { this.organismsById["Z"], this.habitats[8, 0] }
                                        };

            var expectedOrganismDestinations = organismIntendedDestinations.ToDictionary(
                    intendedDestination => intendedDestination.Key,
                    intendedDestination => intendedDestination.Value);

            // expect C, D, X, Y to not have moved
            expectedOrganismDestinations[this.organismsById["C"]] = organismHabitats[this.organismsById["C"]];
            expectedOrganismDestinations[this.organismsById["D"]] = organismHabitats[this.organismsById["D"]];
            expectedOrganismDestinations[this.organismsById["X"]] = organismHabitats[this.organismsById["X"]];
            expectedOrganismDestinations[this.organismsById["Y"]] = organismHabitats[this.organismsById["Y"]];

            var updateSummary = this.CreateAndUpdateEcosystem(organismHabitats, organismIntendedDestinations);

            var expectedCoordinates = expectedOrganismDestinations.Values.Select(expectedDestination => this.habitatCoordinates[expectedDestination]).ToList();
            var actualCoordinates = updateSummary.CurrentOrganismCoordinates.Values.ToList();
            Assert.AreEqual(actualCoordinates, expectedCoordinates);
        }

        private static Habitat[,] GenerateBaseHabitats(int width, int height)
        {
            var habitats = new Habitat[width,height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var environment = new Environment();
                    habitats[x, y] = new Habitat(environment, null);
                }
            }

            return habitats;
        }

        private UpdateSummary CreateAndUpdateEcosystem(Dictionary<Organism, Habitat> organismHabitats, Dictionary<Organism, Habitat> organismIntendedDestinations)
        {
            foreach (var organismHabitat in organismHabitats)
            {
                var organism = organismHabitat.Key;
                var habitat = organismHabitat.Value;

                habitat.AddOrganism(organism);
            }

            EcosystemLogic.OverrideDesiredOrganismHabitats = organismIntendedDestinations;
            EcosystemLogic.OverrideDecideOrganismFunction = organisms => organisms.First();
            var ecosystem = new Ecosystem(this.habitats, organismHabitats);
            return ecosystem.Update();
        }
    }
}

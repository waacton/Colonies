﻿namespace Wacton.Colonies.Domain.Organisms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;

    using Wacton.Colonies.Domain.Plugins;
    using Wacton.Tovarisch.Lexicon;
    using Wacton.Tovarisch.Numbers;
    using Wacton.Tovarisch.Randomness;
    using Wacton.Tovarisch.Strings;

    public class OrganismFactory
    {
        private readonly List<ColonyPluginData> colonyDatas; 
        private readonly WordProvider wordProvider = new WordProvider();
        private readonly List<string> usedNames = new List<string>();

        public OrganismFactory(List<ColonyPluginData> colonyDatas)
        {
            this.colonyDatas = colonyDatas;
        }

        public IOrganism CreateDummyOrganism(Color color)
        {
            var colonyId = Guid.NewGuid();
            var name = this.GenerateFullName("Dummy");
            var organism = new Organism(colonyId, name, color, new DummyLogic());
            return organism;
        }

        public Dictionary<Guid, List<IOrganism>> CreateOrganismRoster()
        {
            var roster = new Dictionary<Guid, List<IOrganism>>();

            foreach (var colonyData in this.colonyDatas)
            {
                var colonyLogicTypes = colonyData.ColonyLogicTypes;
                var organisms = new List<IOrganism>();
                foreach (var logicType in colonyLogicTypes)
                {
                    var organismLogicType = logicType.Item;
                    var organism = this.CreateOrganism(colonyData, organismLogicType);
                    organisms.Add(organism);
                }

                roster.Add(colonyData.ColonyId, organisms);
            }

            return roster;
        }

        public IOrganism CreateOffspringOrganism(IOrganism parentOrganism)
        {
            var colonyData = this.GetColonyData(parentOrganism.ColonyId);
            return this.CreateOrganism(colonyData);
        }

        private IOrganism CreateOrganism(ColonyPluginData colonyData, Type organismLogicType = null)
        {
            var colonyId = colonyData.ColonyId;
            var colonyName = colonyData.ColonyName;
            var colonyColor = colonyData.ColonyColor;
            var name = this.GenerateFullName(colonyName);
            var logicType = organismLogicType ?? RandomSelection.SelectOne(colonyData.ColonyLogicTypes);
            var logic = (IOrganismLogic)Activator.CreateInstance(logicType);
            var organism = new Organism(colonyId, name, colonyColor, logic);
            return organism;
        }

        private ColonyPluginData GetColonyData(Guid colonyId)
        {
            return this.colonyDatas.Single(data => data.ColonyId.Equals(colonyId));
        }

        private string GenerateFullName(string colonyName)
        {
            var givenName = this.wordProvider.GetRandomWord(WordClass.Noun).ToTitleCase();
            var nickName = this.wordProvider.GetRandomWord(WordClass.Verb).ToTitleCase();
            var name = $"{givenName} \"{nickName}\" {colonyName}";

            var nameCount = this.usedNames.Count(usedName => usedName.Equals(name));
            this.usedNames.Add(name);

            // update name with numeral suffix (II, III, IV, etc.) if name already exists
            if (nameCount > 0)
            {
                var numeralSuffix = nameCount + 1;
                name = string.Concat(name, " ", numeralSuffix.ToRomanNumerals());
            }

            return name;
        }
    }
}

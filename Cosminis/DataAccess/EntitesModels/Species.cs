﻿using System;
using System.Collections.Generic;
using Models;

namespace DataAccess.Entities
{
    public partial class Species
    { 
        public Species(Models.Species Species)
        {
            Companions = new HashSet<Companion>();
            this.BaseDex = Species.BaseDex;
            this.BaseInt = Species.BaseInt;
            this.BaseStr = Species.BaseStr;
            this.Description = Species.Description;
            this.SpeciesId = Species.SpeciesId;
            this.SpeciesName = Species.SpeciesName;
            this.FoodElementIdFk = Species.FoodElementIdFk;
            this.ElementType = Species.ElementToString(Species.ElementType); //This is the only important line that allows us to communicate internally in enum but when we talk to the database it will be string
        }
    }
}

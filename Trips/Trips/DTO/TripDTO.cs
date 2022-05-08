using System;
using System.Collections.Generic;

namespace Trips.DTO
{
    public class TripDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int MaxPeople { get; set; }
        public IList<CountryDTO> Countries { get; set; }
        public IList<ClientDTO> Clients { get; set; }

    }
}

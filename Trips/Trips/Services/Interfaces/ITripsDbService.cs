using System.Collections.Generic;
using System.Threading.Tasks;
using Trips.DTO;

namespace Trips.Services.Interfaces
{
    public interface ITripsDbService
    {
        Task<IList<TripDTO>> GetTripsListAsync();

        Task<MethodResultDTO> AddClientToTripAsync(ClientToTripDTO clientToTripDTO);
    }
}

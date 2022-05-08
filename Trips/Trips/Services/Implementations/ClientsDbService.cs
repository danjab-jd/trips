using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trips.DTO;
using Trips.Entities;
using Trips.Services.Interfaces;

namespace Trips.Services.Implementations
{
    public class ClientsDbService : IClientsDbService
    {
        private readonly TripsContext _context;

        public ClientsDbService(TripsContext context)
        {
            _context = context;
        }



        public async Task<MethodResultDTO> DeleteClientAsync(int idClient)
        {
            Client clientFromDb = await _context.Clients
                .Include(x => x.ClientTrips)
                .SingleOrDefaultAsync(x => x.IdClient == idClient);

            if (clientFromDb == null)
            {
                return new MethodResultDTO
                {
                    HttpStatusCode = HttpStatusCode.NotFound,
                    Message = "Client does not exist in the db"
                };
            }

            if (clientFromDb.ClientTrips.Any())
            {
                return new MethodResultDTO
                {
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Message = "Client has assigned trips"
                };
            }

            _context.Clients.Remove(clientFromDb);

            await _context.SaveChangesAsync();

            return new MethodResultDTO
            {
                HttpStatusCode = HttpStatusCode.OK
            };
        }
    }
}

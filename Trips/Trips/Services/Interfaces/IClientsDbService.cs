using System.Threading.Tasks;
using Trips.DTO;

namespace Trips.Services.Interfaces
{
    public interface IClientsDbService
    {
        Task<MethodResultDTO> DeleteClientAsync(int idClient);
    }
}

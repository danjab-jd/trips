using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trips.DTO;
using Trips.Services.Interfaces;

namespace Trips.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsDbService _clientsDbService;

        public ClientsController(IClientsDbService clientsDbService)
        {
            _clientsDbService = clientsDbService;
        }

        [HttpDelete("{idClient}")]
        public async Task<IActionResult> DeleteClient(int idClient)
        {
            MethodResultDTO deleteResult = await _clientsDbService.DeleteClientAsync(idClient);

            return StatusCode((int)deleteResult.HttpStatusCode, deleteResult.Message);
        }
    }
}

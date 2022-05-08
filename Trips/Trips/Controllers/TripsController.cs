using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Trips.DTO;
using Trips.Services.Interfaces;

namespace Trips.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsDbService _tripsDbService;

        public TripsController(ITripsDbService tripsDbService)
        {
            _tripsDbService = tripsDbService;
        }


        [HttpGet]
        public async Task<IActionResult> GetTripsList()
        {
            IList<TripDTO> tripsList = await _tripsDbService.GetTripsListAsync();

            return Ok(tripsList);
        }

        [HttpPost("{idTrip}/clients")]
        public async Task<IActionResult> AddClientToTrip(ClientToTripDTO clientToTripDto)
        {
            MethodResultDTO deleteResult = await _tripsDbService.AddClientToTripAsync(clientToTripDto);

            return StatusCode((int)deleteResult.HttpStatusCode, deleteResult.Message);
        }


    }
}

using System.Net;

namespace Trips.DTO
{
    public class MethodResultDTO
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public string Message { get; set; }
    }
}

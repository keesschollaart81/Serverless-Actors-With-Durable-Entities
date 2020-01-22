using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RoomDemo.RoomBookingService;

namespace RoomDemo
{
    public class RoomFunctions
    {
        private readonly IRoomBookingService _roomBookingService;

        public RoomFunctions(IRoomBookingService roomBookingService)
        {
            _roomBookingService = roomBookingService;
        }

        [FunctionName(nameof(GetRoomDetails))]
        public async Task<IActionResult> GetRoomDetails(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req,
            ILogger log)
        {
            var roomNumber = req.Query["RoomNumber"];
            log.LogInformation("Got room booking request for room {roomNumber}", roomNumber);

            await _roomBookingService.BookRoomAsync(roomNumber);

            return new OkObjectResult($"Thanks for your booking!");
        }
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RoomDemo.RoomBookingService;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;

namespace RoomDemo
{
    public class RoomFunctions
    {
        private readonly IRoomBookingService _roomBookingService;

        public RoomFunctions(IRoomBookingService roomBookingService)
        {
            _roomBookingService = roomBookingService;
        } 
        
        [FunctionName(nameof(BookRoom))]
        public async Task<IActionResult> BookRoom(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req,
            [DurableClient] IDurableEntityClient durableEntityClient,
            ILogger log)
        {
            var roomNumber = req.Query["RoomNumber"];
            var entityId = new EntityId(nameof(RoomEntity), roomNumber);

            log.LogInformation("Got room booking request for room {roomNumber}", roomNumber);

            var roomEntity = await durableEntityClient.ReadEntityStateAsync<RoomEntity>(entityId);
            if (roomEntity.EntityExists && roomEntity.EntityState.IsBooked)
            {
                throw new Exception("Room already booked");
            }

            await durableEntityClient.SignalEntityAsync(entityId, nameof(RoomEntity.BookRoomAsync));

            return new OkObjectResult("Room booked");
        }
    }
}

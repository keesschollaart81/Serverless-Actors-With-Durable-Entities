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
using System.Linq;

namespace RoomDemo
{
    public class RoomFunctions
    {
        private readonly IRoomBookingService _roomBookingService;

        public RoomFunctions(IRoomBookingService roomBookingService)
        {
            _roomBookingService = roomBookingService;
        }

        [FunctionName(nameof(ChangeBookedRoom))]
        public async Task<IActionResult> ChangeBookedRoom(
         [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req,
         [DurableClient] IDurableEntityClient durableEntityClient,
         [DurableClient] IDurableOrchestrationClient durableOrchestrationClient,
         ILogger log)
        {
            var fromRoomNumber = req.Query["FromRoomNumber"];
            var toRoomNumber = req.Query["ToRoomNumber"]; 

            log.LogInformation("Got room booking change request for room {fromRoomNumber} to room {toRoomNumber}", fromRoomNumber, toRoomNumber);

            var orchestrationId = await durableOrchestrationClient.StartNewAsync(nameof(ChangeBookedRoomOrchestrator), $"orch.{fromRoomNumber}.{toRoomNumber}", new ChangeRoomOrchestratorInput(fromRoomNumber, toRoomNumber));

            return durableOrchestrationClient.CreateCheckStatusResponse(req, orchestrationId);
        }

        [FunctionName(nameof(ChangeBookedRoomOrchestrator))]
        public async Task<string> ChangeBookedRoomOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var input = context.GetInput<ChangeRoomOrchestratorInput>();

            if (!context.IsReplaying)
            {
                log.LogInformation("Starting room booking change orchestrator from room {FromRoomNumber} to room {ToRoomNumber}", input.FromRoomNumber, input.ToRoomNumber);
            }

            var fromRoomEntityId = new EntityId(nameof(RoomEntity), input.FromRoomNumber);
            var toRoomEntityId = new EntityId(nameof(RoomEntity), input.ToRoomNumber);
           
            // lock entities to prevent race condition
            using (await context.LockAsync(fromRoomEntityId, toRoomEntityId))
            {
                var fromRoomEntityProxy = context.CreateEntityProxy<IRoomEntity>(fromRoomEntityId);
                var toRoomEntityProxy = context.CreateEntityProxy<IRoomEntity>(toRoomEntityId);
                 
                var toRoomIsCurrentlyBooked = await fromRoomEntityProxy.IsCurrentlyBookedAsync();
                if (toRoomIsCurrentlyBooked)
                {
                    throw new Exception("Cannot change as to-room is already booked");
                }

                await fromRoomEntityProxy.UnBookRoomAsync();
                await toRoomEntityProxy.BookRoomAsync();
            }

            return "Room booking changed!";
        }

        public class ChangeRoomOrchestratorInput
        {
            public string FromRoomNumber { get; set; }
            public string ToRoomNumber { get; set; }

            public ChangeRoomOrchestratorInput()
            {

            }

            public ChangeRoomOrchestratorInput(string fromRoomNumber, string toRoomNumber)
            {
                FromRoomNumber = fromRoomNumber;
                ToRoomNumber = toRoomNumber;
            }
        }
    }
}

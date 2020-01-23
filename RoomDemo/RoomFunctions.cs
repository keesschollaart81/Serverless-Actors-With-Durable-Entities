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
            [DurableClient] IDurableOrchestrationClient durableOrchestrationClient,
            ILogger log)
        {
            var roomNumber = req.Query["RoomNumber"];
            log.LogInformation("Got room booking request for room {roomNumber}", roomNumber);

            var orchestrationId = await durableOrchestrationClient.StartNewAsync(nameof(BookRoomOrchestrator), $"orch.{roomNumber}", new BookRoomOrchestratorInput(roomNumber));

            return durableOrchestrationClient.CreateCheckStatusResponse(req, orchestrationId);
        }

        [FunctionName(nameof(BookRoomOrchestrator))]
        public async Task<IActionResult> BookRoomOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var input = context.GetInput<BookRoomOrchestratorInput>();

            if (!context.IsReplaying)
            {
                log.LogInformation("Starting room booking orchestrator for room {roomNumber}", input.RoomNumber);
            }

            var isRoomBooked = await context.CallActivityAsync<bool>(nameof(IsRoomBooked), input.RoomNumber);

            if (!isRoomBooked)
            {
                await _roomBookingService.BookRoomAsync(input.RoomNumber);
            }
            else
            {
                throw new Exception("Room is already booked!");
            }

            var informAttendeesTasks = new List<Task>();
            foreach (var attendee in new string[] { "jan", "kees", "piet" })
            {
                var task = context.CallActivityAsync<bool>(nameof(InformAttendee), attendee);
                informAttendeesTasks.Add(task);
            }
            await Task.WhenAll(informAttendeesTasks);

            return new OkObjectResult($"Thanks for your booking!");
        }

        [FunctionName(nameof(IsRoomBooked))]
        public async Task<bool> IsRoomBooked(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            var roomNumber = context.GetInput<string>();
            log.LogInformation("Checking if room {roomNumber} is booked", roomNumber);

            return await _roomBookingService.IsRoomBookedAsync(roomNumber);
        }


        [FunctionName(nameof(InformAttendee))]
        public async Task InformAttendee(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            var attendee = context.GetInput<string>();
            log.LogInformation("Informing attendee {attendee}", attendee);

            await _roomBookingService.InformAttendeeAsync(attendee);
        }

        public class BookRoomOrchestratorInput
        {
            public BookRoomOrchestratorInput()
            {

            }

            public BookRoomOrchestratorInput(string roomNumber)
            {
                RoomNumber = roomNumber;
            }
            public string RoomNumber { get; set; }
        }
    }
}

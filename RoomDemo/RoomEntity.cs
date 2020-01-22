using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace RoomDemo
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RoomEntity
    {
        [JsonProperty]
        public string RoomNumber { get; set; }

        [JsonProperty]
        public bool IsBooked { get; set; }

        private readonly ILogger _logger;

        public RoomEntity(ILogger logger, string roomNumber)
        {
            _logger = logger;
            RoomNumber = roomNumber;
        }

        [FunctionName(nameof(RoomEntity))]
        public static async Task HandleEntityOperation(
            [EntityTrigger] IDurableEntityContext context,
            ILogger logger)
        {
            await context.DispatchAsync<RoomEntity>(logger, context.EntityKey);
        }


        public Task<bool> IsRoomBookedAsync()
        {
            var isBooked = RoomNumber == "123";
            return Task.FromResult(isBooked);
        }
        
        public Task BookRoomAsync(string roomNumber)
        {
            IsBooked = true;
            return Task.CompletedTask;
        }
    }
}

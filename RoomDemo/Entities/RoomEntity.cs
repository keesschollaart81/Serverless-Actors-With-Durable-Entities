using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace RoomDemo
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RoomEntity : IRoomEntity
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


        public Task<string> GetAttendeeInfoAsync(string username)
        {
            return Task.FromResult(username.ToUpper());
        }

        public Task BookRoomAsync()
        {
            IsBooked = true;
            return Task.CompletedTask;
        }

        public Task<bool> IsCurrentlyBookedAsync()
        {
            return Task.FromResult(IsBooked);
        }

        public Task UnBookRoomAsync()
        {
            IsBooked = false;
            return Task.CompletedTask;
        }
    }
}

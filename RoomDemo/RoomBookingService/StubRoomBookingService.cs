using System.Threading.Tasks;

namespace RoomDemo.RoomBookingService
{
    public class StubRoomBookingService : IRoomBookingService
    {
        public Task BookRoomAsync(string roomNumber)
        {
            return Task.CompletedTask;
        }

        public Task<bool> IsRoomBookedAsync(string roomNumber)
        {
            return Task.FromResult(roomNumber == "123");
        }

        public Task InformAttendeeAsync(string attendee)
        {
            return Task.CompletedTask;
        }
    }
}

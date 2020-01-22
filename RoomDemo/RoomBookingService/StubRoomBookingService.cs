using System.Threading.Tasks;

namespace RoomDemo.RoomBookingService
{
    public class StubRoomBookingService : IRoomBookingService
    {
        public Task BookRoomAsync(string roomNumber)
        {
            return Task.CompletedTask;
        }
    }
}

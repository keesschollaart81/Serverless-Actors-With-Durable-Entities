using System.Threading.Tasks;

namespace RoomDemo.RoomBookingService
{
    public interface IRoomBookingService
    {
        Task BookRoomAsync(string roomNumber);
    }
}

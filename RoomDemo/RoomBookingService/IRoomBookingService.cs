using System.Threading.Tasks;

namespace RoomDemo.RoomBookingService
{
    public interface IRoomBookingService
    {
        Task BookRoomAsync(string roomNumber);
        Task<bool> IsRoomBookedAsync(string roomNumber);
        Task InformAttendeeAsync(string attendee);
    }
}

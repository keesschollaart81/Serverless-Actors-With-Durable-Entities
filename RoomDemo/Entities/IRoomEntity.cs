using System.Threading.Tasks;

namespace RoomDemo
{
    public interface IRoomEntity
    { 
        Task BookRoomAsync();
        Task UnBookRoomAsync();
        Task<string> GetAttendeeInfoAsync(string username);
        Task<bool> IsCurrentlyBookedAsync();
    }
}

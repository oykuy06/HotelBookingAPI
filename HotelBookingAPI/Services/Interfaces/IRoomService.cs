using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomResponseDto>> GetAllRoomsAsync();
        Task<RoomResponseDto?> GetRoomByIdAsync(long id);
        Task<Room> CreateRoomAsync(Room room);
        Task<Room?> UpdateRoomAsync(long id, Room room);
        Task<bool> DeleteRoomAsync(long id);
    }
}

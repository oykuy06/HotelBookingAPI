using HotelBookingAPI.Dto.RequestDto;
using HotelBookingAPI.Dto.ResponseDto;
using HotelBookingAPI.Entity.Models;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;

        public RoomController(IRoomService roomService, IUserService userService)
        {
            _roomService = roomService;
            _userService = userService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            var roomDtos = rooms.Select(r => new RoomResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                HotelId = r.HotelId,
                HotelName = r.HotelName,
                RoomNumber = r.RoomNumber,
                Capacity = r.Capacity,
                Price = r.Price,
                Description = r.Description,
                Photo = r.Photo
            });

            return Ok(roomDtos);
        }

        [HttpGet("{id:long}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoomById(long id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
                return NotFound(new { message = "Room not found" });

            var roomDto = new RoomResponseDto
            {
                Id = room.Id,
                Name = room.Name,
                HotelId = room.HotelId,
                HotelName = room.HotelName,
                RoomNumber = room.RoomNumber,
                Capacity = room.Capacity,
                Price = room.Price,
                Description = room.Description,
                Photo = room.Photo
            };

            return Ok(roomDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> CreateRoom([FromBody] RoomDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.IsNullOrEmpty(dto.Photo) &&
                !Uri.IsWellFormedUriString(dto.Photo, UriKind.Absolute))
            {
                return BadRequest(new { message = "Photo must be a valid URL." });
            }

            long? hotelId;

            if (User.IsInRole("Admin"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null ||
                    !long.TryParse(userIdClaim.Value, out var userId))
                {
                    return Forbid();
                }

                hotelId = await _userService.GetHotelIdByUserIdAsync(userId);

                if (hotelId == null)
                    return Forbid();
            }
            else
            {
                if (dto.HotelId == null)
                    return BadRequest(new { message = "HotelId is required for SuperAdmin." });

                hotelId = dto.HotelId;
            }

            var room = new Room
            {
                Name = dto.Name,
                HotelId = hotelId.Value,
                RoomNumber = dto.RoomNumber,
                Capacity = dto.Capacity,
                Price = dto.Price,
                Description = dto.Description,
                Photo = dto.Photo
            };

            Room createdRoom;

            try
            {
                createdRoom = await _roomService.CreateRoomAsync(room);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            var responseDto = new RoomResponseDto
            {
                Id = createdRoom.Id,
                Name = createdRoom.Name,
                HotelId = createdRoom.HotelId,
                HotelName = createdRoom.Hotel?.Name ?? "",
                RoomNumber = createdRoom.RoomNumber,
                Capacity = createdRoom.Capacity,
                Price = createdRoom.Price,
                Description = createdRoom.Description,
                Photo = createdRoom.Photo
            };

            return CreatedAtAction(
                nameof(GetRoomById),
                new { id = createdRoom.Id },
                responseDto);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateRoom(long id, [FromBody] RoomDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.IsNullOrEmpty(dto.Photo) &&
                !Uri.IsWellFormedUriString(dto.Photo, UriKind.Absolute))
            {
                return BadRequest(new { message = "Photo must be a valid URL." });
            }

            var existingRoom = await _roomService.GetRoomByIdAsync(id);

            if (existingRoom == null)
                return NotFound(new { message = "Room not found" });

            if (User.IsInRole("Admin"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null ||
                    !long.TryParse(userIdClaim.Value, out var userId))
                    return Forbid();

                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(userId);

                if (adminHotelId == null || existingRoom.HotelId != adminHotelId)
                    return Forbid();

                if (dto.HotelId.HasValue && dto.HotelId.Value != adminHotelId)
                    return Forbid();
            }

            var updatedRoom = await _roomService.UpdateRoomAsync(id, new Room
            {
                Name = dto.Name,
                HotelId = existingRoom.HotelId,
                RoomNumber = dto.RoomNumber,
                Capacity = dto.Capacity,
                Price = dto.Price,
                Description = dto.Description,
                Photo = dto.Photo
            });

            if (updatedRoom == null)
                return NotFound(new { message = "Room not found" });

            return Ok(new { message = "Room updated successfully" });
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteRoom(long id)
        {
            if (User.IsInRole("Admin"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null ||
                    !long.TryParse(userIdClaim.Value, out var userId))
                    return Forbid();

                var adminHotelId = await _userService.GetHotelIdByUserIdAsync(userId);

                if (adminHotelId == null)
                    return Forbid();

                var room = await _roomService.GetRoomByIdAsync(id);

                if (room == null)
                    return NotFound(new { message = "Room not found" });

                if (room.HotelId != adminHotelId)
                    return Forbid();
            }

            try
            {
                var success = await _roomService.DeleteRoomAsync(id);

                if (!success)
                    return NotFound(new { message = "Room not found" });

                return Ok(new { message = "Room deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}

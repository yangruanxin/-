using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelMap.Data;
using TravelMap.Models;
using Microsoft.EntityFrameworkCore;  

namespace TravelMap.Controllers
{
    // Controllers/CheckInController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CheckInController(AppDbContext context)
        {
            _context = context;
        }

        // 获取单个打卡记录
        [HttpGet("{id}")]
        public async Task<ActionResult<CheckIn>> GetCheckIn(int id)
        {
            var checkIn = await _context.CheckIns.FindAsync(id);

            if (checkIn == null)
            {
                return NotFound();
            }

            return checkIn;
        }
        // 获取用户的所有打卡记录
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<CheckIn>>> GetUserCheckIns(string userId)
        {
            return await _context.CheckIns.Where(c => c.UserId == userId).ToListAsync();
        }

        // 创建新的打卡记录
        [HttpPost]
        public async Task<ActionResult<CheckIn>> CreateCheckIn([FromBody] CheckInDto checkInDto)
        {
            var checkIn = new CheckIn
            {
                UserId = checkInDto.UserId,
                LocationId = checkInDto.LocationId,
                CheckInTime = DateTime.Now,
                Note = checkInDto.Note,
                PhotoUrl = checkInDto.PhotoUrl
            };

            _context.CheckIns.Add(checkIn);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCheckIn), new { id = checkIn.Id }, checkIn);
        }

        // 上传打卡照片
        [HttpPost("upload-photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var photoUrl = $"/uploads/{uniqueFileName}";
            return Ok(new { photoUrl });
        }
    }

    // CheckInDto.cs
    public class CheckInDto
    {
        public string UserId { get; set; }
        public int LocationId { get; set; }
        public string Note { get; set; }
        public string PhotoUrl { get; set; }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DawaeeBackend.Data;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ProfileController(ApplicationDbContext db) { _db = db; }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        var totalSchedules = await _db.MedicationSchedules.CountAsync(s => s.UserId == userId);
        var takenDoses = await _db.MedicationSchedules.CountAsync(s => s.UserId == userId && s.IsTaken);
        var chatCount = await _db.ChatMessages.CountAsync(c => c.UserId == userId);

        return Ok(new {
            user.Id, user.FirstName, user.LastName, user.Email, user.Role, user.CreatedAt,
            Stats = new {
                TotalSchedules = totalSchedules,
                TakenDoses = takenDoses,
                ChatMessages = chatCount
            }
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
        if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest(new { message = "Current password is incorrect" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Profile updated successfully", user.FirstName, user.LastName, user.Email });
    }
}

public class UpdateProfileDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
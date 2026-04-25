using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DawaeeBackend.Data;
using DawaeeBackend.DTOs;
using DawaeeBackend.Models;

namespace DawaeeBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    [HttpGet("schedules")]
    public async Task<ActionResult<List<ScheduleResponseDto>>> GetSchedules()
    {
        var userId = GetUserId();
        var schedules = await _context.MedicationSchedules
            .Include(s => s.Medication)
            .Where(s => s.UserId == userId && s.ScheduledTime.Date == DateTime.UtcNow.Date)
            .OrderBy(s => s.ScheduledTime)
            .ToListAsync();

        return schedules.Select(s => new ScheduleResponseDto
        {
            Id = s.Id,
            MedicationId = s.MedicationId,
            MedicationNameAr = s.Medication.NameAr,
            MedicationNameEn = s.Medication.NameEn,
            ScheduledTime = s.ScheduledTime,
            IsTaken = s.IsTaken
        }).ToList();
    }

    [HttpPost("schedules")]
    public async Task<ActionResult<ScheduleResponseDto>> AddSchedule(CreateScheduleDto dto)
    {
        var userId = GetUserId();
        
        var medication = await _context.Medications.FindAsync(dto.MedicationId);
        if (medication == null)
            return BadRequest(new { message = "Medication not found" });

        var schedule = new MedicationSchedule
        {
            UserId = userId,
            MedicationId = dto.MedicationId,
            ScheduledTime = dto.ScheduledTime.ToUniversalTime(),
            IsTaken = false
        };

        _context.MedicationSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return Ok(new ScheduleResponseDto
        {
            Id = schedule.Id,
            MedicationId = schedule.MedicationId,
            MedicationNameAr = medication.NameAr,
            MedicationNameEn = medication.NameEn,
            ScheduledTime = schedule.ScheduledTime,
            IsTaken = schedule.IsTaken
        });
    }

    [HttpPut("schedules/toggle")]
    public async Task<IActionResult> ToggleTaken([FromBody] ToggleTakenDto dto)
    {
        var userId = GetUserId();
        var schedule = await _context.MedicationSchedules
            .FirstOrDefaultAsync(s => s.Id == dto.ScheduleId && s.UserId == userId);

        if (schedule == null)
            return NotFound(new { message = "Schedule not found" });

        schedule.IsTaken = dto.IsTaken;
        schedule.TakenAt = dto.IsTaken ? DateTime.UtcNow : (DateTime?)null;
        
        await _context.SaveChangesAsync();
        
        return Ok(new { success = true, isTaken = schedule.IsTaken });
    }

    [HttpDelete("schedules/{id}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var userId = GetUserId();
        var schedule = await _context.MedicationSchedules
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (schedule == null)
            return NotFound();

        _context.MedicationSchedules.Remove(schedule);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Schedule deleted successfully" });
    }

    [HttpGet("weekly-report")]
    public async Task<ActionResult<WeeklyReportDto>> GetWeeklyReport()
    {
        var userId = GetUserId();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var schedules = await _context.MedicationSchedules
            .Where(s => s.UserId == userId && s.ScheduledTime >= startDate && s.ScheduledTime <= endDate)
            .ToListAsync();

        var total = schedules.Count;
        var taken = schedules.Count(s => s.IsTaken);
        var adherence = total > 0 ? Math.Round((double)taken / total * 100, 2) : 0;

        return Ok(new WeeklyReportDto
        {
            TotalMeds = total,
            TakenMeds = taken,
            AdherencePercentage = adherence
        });
    }
}

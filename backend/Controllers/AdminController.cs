using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DawaeeBackend.Data;
using DawaeeBackend.DTOs;
using DawaeeBackend.Models;
using BCrypt.Net;

namespace DawaeeBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all patients
    [HttpGet("patients")]
    public async Task<ActionResult<List<PatientListDto>>> GetPatients()
    {
        var patients = await _context.Users
            .Where(u => u.Role == "Patient")
            .Include(u => u.MedicationSchedules)
            .ToListAsync();

        var result = new List<PatientListDto>();
        foreach (var patient in patients)
        {
            var totalSchedules = patient.MedicationSchedules.Count;
            var takenSchedules = patient.MedicationSchedules.Count(s => s.IsTaken);
            var adherenceRate = totalSchedules > 0 ? (double)takenSchedules / totalSchedules * 100 : 0;

            result.Add(new PatientListDto
            {
                Id = patient.Id,
                Name = $"{patient.FirstName} {patient.LastName}",
                Email = patient.Email,
                ChronicDiseases = patient.ChronicDiseases,
                RegisteredAt = patient.CreatedAt,
                TotalSchedules = totalSchedules,
                AdherenceRate = Math.Round(adherenceRate, 2)
            });
        }
        return Ok(result);
    }

    // Get patient details
    [HttpGet("patients/{id}")]
    public async Task<ActionResult<PatientDetailsDto>> GetPatientDetails(int id)
    {
        var patient = await _context.Users
            .Include(u => u.MedicationSchedules)
                .ThenInclude(s => s.Medication)
            .Include(u => u.ChatMessages)
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == "Patient");

        if (patient == null)
            return NotFound();

        var totalSchedules = patient.MedicationSchedules.Count;
        var takenSchedules = patient.MedicationSchedules.Count(s => s.IsTaken);
        var adherenceRate = totalSchedules > 0 ? (double)takenSchedules / totalSchedules * 100 : 0;

        return Ok(new PatientDetailsDto
        {
            Id = patient.Id,
            Name = $"{patient.FirstName} {patient.LastName}",
            Email = patient.Email,
            ChronicDiseases = patient.ChronicDiseases,
            RegisteredAt = patient.CreatedAt,
            TotalSchedules = totalSchedules,
            AdherenceRate = Math.Round(adherenceRate, 2),
            CurrentSchedules = patient.MedicationSchedules.Select(s => new ScheduleResponseDto
            {
                Id = s.Id,
                MedicationId = s.MedicationId,
                MedicationNameAr = s.Medication.NameAr,
                MedicationNameEn = s.Medication.NameEn,
                ScheduledTime = s.ScheduledTime,
                IsTaken = s.IsTaken
            }).ToList(),
            ChatHistory = patient.ChatMessages.Select(c => new ChatHistoryDto
            {
                Id = c.Id,
                Message = c.Message,
                Response = c.Response,
                CreatedAt = c.CreatedAt
            }).ToList()
        });
    }

    // Delete patient
    [HttpDelete("patients/{id}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var patient = await _context.Users.FindAsync(id);
        if (patient == null || patient.Role != "Patient")
            return NotFound();

        _context.Users.Remove(patient);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Patient deleted successfully" });
    }

    // Get all medications
    [HttpGet("medications")]
    public async Task<ActionResult<List<MedicationDto>>> GetMedications()
    {
        var medications = await _context.Medications.ToListAsync();
        return Ok(medications.Select(m => new MedicationDto
        {
            Id = m.Id,
            NameAr = m.NameAr,
            NameEn = m.NameEn,
            ActiveIngredientAr = m.ActiveIngredientAr,
            ActiveIngredientEn = m.ActiveIngredientEn,
            DescriptionAr = m.DescriptionAr,
            DescriptionEn = m.DescriptionEn,
            WarningsAr = m.WarningsAr,
            WarningsEn = m.WarningsEn,
            DangerLevel = m.DangerLevel
        }));
    }

    // Create medication
    [HttpPost("medications")]
    public async Task<ActionResult<MedicationDto>> CreateMedication(CreateMedicationDto dto)
    {
        var medication = new Medication
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            ActiveIngredientAr = dto.ActiveIngredientAr,
            ActiveIngredientEn = dto.ActiveIngredientEn,
            DescriptionAr = dto.DescriptionAr,
            DescriptionEn = dto.DescriptionEn,
            WarningsAr = dto.WarningsAr,
            WarningsEn = dto.WarningsEn,
            DangerLevel = dto.DangerLevel
        };

        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        return Ok(new MedicationDto
        {
            Id = medication.Id,
            NameAr = medication.NameAr,
            NameEn = medication.NameEn,
            ActiveIngredientAr = medication.ActiveIngredientAr,
            ActiveIngredientEn = medication.ActiveIngredientEn,
            DescriptionAr = medication.DescriptionAr,
            DescriptionEn = medication.DescriptionEn,
            WarningsAr = medication.WarningsAr,
            WarningsEn = medication.WarningsEn,
            DangerLevel = medication.DangerLevel
        });
    }

    // Update medication
    [HttpPut("medications/{id}")]
    public async Task<IActionResult> UpdateMedication(int id, CreateMedicationDto dto)
    {
        var medication = await _context.Medications.FindAsync(id);
        if (medication == null)
            return NotFound();

        medication.NameAr = dto.NameAr;
        medication.NameEn = dto.NameEn;
        medication.ActiveIngredientAr = dto.ActiveIngredientAr;
        medication.ActiveIngredientEn = dto.ActiveIngredientEn;
        medication.DescriptionAr = dto.DescriptionAr;
        medication.DescriptionEn = dto.DescriptionEn;
        medication.WarningsAr = dto.WarningsAr;
        medication.WarningsEn = dto.WarningsEn;
        medication.DangerLevel = dto.DangerLevel;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Medication updated successfully" });
    }

    // Delete medication
    [HttpDelete("medications/{id}")]
    public async Task<IActionResult> DeleteMedication(int id)
    {
        var medication = await _context.Medications.FindAsync(id);
        if (medication == null)
            return NotFound();

        _context.Medications.Remove(medication);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Medication deleted successfully" });
    }
}

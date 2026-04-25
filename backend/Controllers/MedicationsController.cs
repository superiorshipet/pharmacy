using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DawaeeBackend.Data;
using DawaeeBackend.DTOs;
using DawaeeBackend.Models;

namespace DawaeeBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MedicationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<MedicationDto>>> GetAll([FromQuery] string? search)
    {
        var query = _context.Medications.AsQueryable();
        
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => 
                m.NameAr.Contains(search) || 
                m.NameEn.Contains(search) ||
                m.ActiveIngredientAr.Contains(search) ||
                m.ActiveIngredientEn.Contains(search));
        }
        
        var medications = await query.ToListAsync();
        
        return medications.Select(m => new MedicationDto
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
        }).ToList();
    }
}

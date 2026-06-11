using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJATK_APBD_Cw5_s29953.DTOs;
using PJATK_APBD_Cw5_s29953.Models;

namespace PJATK_APBD_Cw5_s29953.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly HospitalDbContext _context;

    public PatientsController(HospitalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var query = _context.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, $"%{search}%") ||
                EF.Functions.Like(p.LastName, $"%{search}%"));
        }

        var patients = await query
            .Select(p => new PatientResponseDto
            {
                Pesel = p.Pesel,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Age = p.Age,
                Sex = p.Sex ? "Male" : "Female",

                Admissions = p.Admissions.Select(a => new AdmissionResponseDto
                {
                    Id = a.Id,
                    AdmissionDate = a.AdmissionDate,
                    DischargeDate = a.DischargeDate,
                    Ward = new WardResponseDto
                    {
                        Id = a.Ward.Id,
                        Name = a.Ward.Name,
                        Description = a.Ward.Description
                    }
                }).ToList(),

                BedAssignments = p.BedAssignments.Select(ba => new BedAssignmentResponseDto
                {
                    Id = ba.Id,
                    From = ba.From,
                    To = ba.To,
                    Bed = new BedResponseDto
                    {
                        Id = ba.Bed.Id,
                        BedType = new BedTypeResponseDto
                        {
                            Id = ba.Bed.BedType.Id,
                            Name = ba.Bed.BedType.Name,
                            Description = ba.Bed.BedType.Description
                        },
                        Room = new RoomResponseDto
                        {
                            Id = ba.Bed.Room.Id,
                            HasTv = ba.Bed.Room.HasTv,
                            Ward = new WardResponseDto
                            {
                                Id = ba.Bed.Room.Ward.Id,
                                Name = ba.Bed.Room.Ward.Name,
                                Description = ba.Bed.Room.Ward.Description
                            }
                        }
                    }
                }).ToList()
            })
            .ToListAsync();

        return Ok(patients);
    }
    
    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> CreateBedAssignment(
        string pesel,
        [FromBody] CreateBedAssignmentRequestDto request)
    {
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Pesel == pesel);

        if (!patientExists)
        {
            return NotFound($"Patient with PESEL {pesel} was not found.");
        }

        var ward = await _context.Wards
            .FirstOrDefaultAsync(w => w.Name == request.Ward);

        if (ward is null)
        {
            return NotFound($"Ward '{request.Ward}' was not found.");
        }

        var bedType = await _context.BedTypes
            .FirstOrDefaultAsync(bt => bt.Name == request.BedType);

        if (bedType is null)
        {
            return NotFound($"Bed type '{request.BedType}' was not found.");
        }

        var requestTo = request.To ?? new DateTime(9999, 12, 31);

        var availableBed = await _context.Beds
            .Include(b => b.Room)
            .Include(b => b.BedAssignments)
            .Where(b =>
                b.BedTypeId == bedType.Id &&
                b.Room.WardId == ward.Id &&
                !b.BedAssignments.Any(ba =>
                    request.From < (ba.To ?? new DateTime(9999, 12, 31)) &&
                    requestTo > ba.From))
            .FirstOrDefaultAsync();

        if (availableBed is null)
        {
            return NotFound($"No available bed of type '{request.BedType}' was found in ward '{request.Ward}' for the selected time period.");
        }

        var bedAssignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = availableBed.Id,
            From = request.From,
            To = request.To
        };

        _context.BedAssignments.Add(bedAssignment);
        await _context.SaveChangesAsync();

        return Created($"/api/patients/{pesel}/bedassignments/{bedAssignment.Id}", new
        {
            bedAssignment.Id,
            bedAssignment.PatientPesel,
            bedAssignment.BedId,
            bedAssignment.From,
            bedAssignment.To
        });
    }
}
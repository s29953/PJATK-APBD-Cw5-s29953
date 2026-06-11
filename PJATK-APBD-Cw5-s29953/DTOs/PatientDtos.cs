namespace PJATK_APBD_Cw5_s29953.DTOs;

public class PatientResponseDto
{
    public string Pesel { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int Age { get; set; }
    public string Sex { get; set; } = null!;
    public List<AdmissionResponseDto> Admissions { get; set; } = [];
    public List<BedAssignmentResponseDto> BedAssignments { get; set; } = [];
}

public class AdmissionResponseDto
{
    public int Id { get; set; }
    public DateTime AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public WardResponseDto Ward { get; set; } = null!;
}

public class BedAssignmentResponseDto
{
    public int Id { get; set; }
    public DateTime From { get; set; }
    public DateTime? To { get; set; }
    public BedResponseDto Bed { get; set; } = null!;
}

public class BedResponseDto
{
    public int Id { get; set; }
    public BedTypeResponseDto BedType { get; set; } = null!;
    public RoomResponseDto Room { get; set; } = null!;
}

public class BedTypeResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class RoomResponseDto
{
    public string Id { get; set; } = null!;
    public bool HasTv { get; set; }
    public WardResponseDto Ward { get; set; } = null!;
}

public class WardResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class CreateBedAssignmentRequestDto
{
    public DateTime From { get; set; }
    public DateTime? To { get; set; }
    public string BedType { get; set; } = null!;
    public string Ward { get; set; } = null!;
}
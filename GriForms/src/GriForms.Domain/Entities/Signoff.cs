using GriForms.Domain.Enums;

namespace GriForms.Domain.Entities;

public class Signoff
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public RoleName Role { get; set; }
    public string UserId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? JobTitle { get; set; }
    public DateTime SignedAt { get; set; }
}

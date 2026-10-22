using GriForms.Domain.Enums;

namespace GriForms.Domain.Entities;

public class UserSiteRole
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int? SiteId { get; set; }
    public RoleName Role { get; set; }

    public Site? Site { get; set; }
}

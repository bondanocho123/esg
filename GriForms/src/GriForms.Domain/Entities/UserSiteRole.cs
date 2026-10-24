using GriForms.Domain.Enums;

namespace GriForms.Domain.Entities;

// Keanggotaan tenant. SiteId null (mis. Admin) berarti semua site di tenant baris ini saja.
public class UserSiteRole : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string UserId { get; set; } = "";
    public int? SiteId { get; set; }
    public RoleName Role { get; set; }

    public Site? Site { get; set; }
}

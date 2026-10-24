namespace GriForms.Domain.Entities;

// Pengelola platform: mengatur tenant, site, dan definisi form. Tidak otomatis melihat data tenant mana pun.
// Disimpan terpisah dari UserSiteRole agar UserSiteRole.TenantId tetap wajib.
public class PlatformAdmin
{
    public string UserId { get; set; } = "";
    public DateTime GrantedAt { get; set; }
}

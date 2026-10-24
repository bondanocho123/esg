namespace GriForms.Domain.Entities;

public class Site : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Code { get; set; } = "";   // unik per tenant; kolom A (Site_ID) di Excel lama
    public string Name { get; set; } = "";
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
}

namespace GriForms.Domain.Entities;

public class Site
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
}

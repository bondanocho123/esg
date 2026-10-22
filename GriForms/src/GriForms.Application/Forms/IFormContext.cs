namespace GriForms.Application.Forms;

// Konteks submission yang sedang diisi; diisi halaman form saat memuat submission
public interface IFormContext
{
    int SiteId { get; }
    int Year { get; }
}

public sealed class FormContext : IFormContext
{
    public int SiteId { get; private set; }
    public int Year { get; private set; } = DateTime.Today.Year;

    public void Use(int siteId, int year) => (SiteId, Year) = (siteId, year);
}

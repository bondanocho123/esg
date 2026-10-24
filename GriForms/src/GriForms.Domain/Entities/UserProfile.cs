namespace GriForms.Domain.Entities;

// Preferensi dan flag akun yang dibaca Infrastructure tanpa bergantung ke ApplicationUser di proyek Web.
// Pengguna lama tanpa baris di sini berarti tidak wajib ganti kata sandi.
public class UserProfile
{
    public string UserId { get; set; } = "";
    public int? ActiveTenantId { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime UpdatedAt { get; set; }
}

using System.Text.RegularExpressions;
using GriForms.Application.Abstractions;
using GriForms.Application.Telemetry;
using GriForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Telemetry;

// Site dan kode device dicek terhadap tenant aktif saja (filter tenant di AppDbContext):
// site tenant lain tidak ditemukan, dan kode yang sama boleh dipakai tenant lain.
public sealed partial class TelemetryDevices(IAppReadDb db, IUnitOfWork uow, ITenantContext tenant) : ITelemetryDevices
{
    private const int MaxCodeLength = 50;
    private const int MaxNameLength = 100;

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._-]*$")]
    private static partial Regex CodePattern();

    public async Task<string> RegisterAsync(TelemetryAccess access, string code, string name, int siteId, CancellationToken ct)
    {
        if (access is null || !access.IsAdmin || tenant.TenantId is not { } tenantId)
            throw new UnauthorizedAccessException("Hanya Admin yang boleh mendaftarkan device.");

        code = code?.Trim() ?? "";
        name = name?.Trim() ?? "";

        if (code.Length == 0)
            throw new InvalidOperationException("Kode device wajib diisi.");
        if (code.Length > MaxCodeLength || !CodePattern().IsMatch(code))
            throw new InvalidOperationException($"Kode device hanya boleh huruf, angka, '.', '_' atau '-' (maksimal {MaxCodeLength} karakter).");
        if (name.Length > MaxNameLength)
            throw new InvalidOperationException($"Nama device maksimal {MaxNameLength} karakter.");
        if (await db.TelemetryDevices.AnyAsync(d => d.Code == code, ct))
            throw new InvalidOperationException($"Kode device {code} sudah dipakai.");
        if (!await db.Sites.AnyAsync(s => s.Id == siteId && s.IsActive, ct))
            throw new InvalidOperationException($"Site {siteId} tidak ditemukan.");

        var key = DeviceApiKey.Generate();
        var device = new TelemetryDevice
        {
            TenantId = tenantId,
            Code = code,
            Name = name.Length == 0 ? code : name,
            SiteId = siteId,
            ApiKeyHash = DeviceApiKey.Hash(key),
        };
        uow.Add(device);
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            uow.Remove(device);

            // Pendaftaran bersamaan dengan kode sama lolos cek di atas, tapi ditolak unique index.
            // Kegagalan lain (mis. 'database is locked') diteruskan, bukan disamarkan sebagai kode duplikat.
            if (await db.TelemetryDevices.AnyAsync(d => d.Code == code, ct))
                throw new InvalidOperationException($"Kode device {code} sudah dipakai.");
            throw;
        }

        return key;
    }
}

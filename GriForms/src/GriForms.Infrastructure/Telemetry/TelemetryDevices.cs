using System.Text.RegularExpressions;
using GriForms.Application.Abstractions;
using GriForms.Application.Telemetry;
using GriForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Telemetry;

public sealed partial class TelemetryDevices(IAppReadDb db, IUnitOfWork uow) : ITelemetryDevices
{
    private const int MaxCodeLength = 50;
    private const int MaxNameLength = 100;

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._-]*$")]
    private static partial Regex CodePattern();

    public async Task<string> RegisterAsync(TelemetryAccess access, string code, string name, int siteId, CancellationToken ct)
    {
        if (access is null || !access.IsAdmin)
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
            // Pendaftaran bersamaan dengan kode sama lolos cek di atas, tapi ditolak unique index
            uow.Remove(device);
            throw new InvalidOperationException($"Kode device {code} sudah dipakai.");
        }

        return key;
    }
}

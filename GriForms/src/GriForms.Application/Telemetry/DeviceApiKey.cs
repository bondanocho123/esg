using System.Security.Cryptography;
using System.Text;

namespace GriForms.Application.Telemetry;

// API key device dibuat sekali dan ditampilkan sekali; yang disimpan hanya hash SHA-256-nya
public static class DeviceApiKey
{
    public static string Generate() => RandomNumberGenerator.GetHexString(32, lowercase: true);

    public static string Hash(string key) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key.Trim())));
}

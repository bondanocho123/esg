using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GriForms.Infrastructure.Persistence;

// Mode WAL dan busy_timeout untuk SQLite (satu penulis pada satu waktu). Dijalankan tiap koneksi dibuka.
public sealed class SqlitePragmaInterceptor : DbConnectionInterceptor
{
    private const string Pragmas = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;";
    private const string ReadOnlyPragmas = "PRAGMA busy_timeout=5000;";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        => Apply(connection);

    public override async Task ConnectionOpenedAsync(
        DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = PragmasFor(connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void Apply(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = PragmasFor(connection);
        command.ExecuteNonQuery();
    }

    // EF membuka koneksi Mode=ReadOnly untuk mengecek keberadaan database (dotnet ef database update);
    // mengganti journal_mode di koneksi itu ditolak SQLite ("readonly database").
    private static string PragmasFor(DbConnection connection)
        => new SqliteConnectionStringBuilder(connection.ConnectionString).Mode == SqliteOpenMode.ReadOnly
            ? ReadOnlyPragmas
            : Pragmas;
}

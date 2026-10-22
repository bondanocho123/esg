using GriForms.Application.Abstractions;
using GriForms.Infrastructure.Consolidation;
using GriForms.Infrastructure.Excel;
using GriForms.Infrastructure.Forms;
using GriForms.Infrastructure.Persistence;
using GriForms.Infrastructure.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GriForms.Infrastructure;

public static class DependencyInjection
{
    // ICurrentUser didaftarkan oleh host (Web), karena identitas pengguna bergantung pada cara login
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["Database:Provider"] ?? "Sqlite";
        var connectionString = config.GetConnectionString("AppConnection")
            ?? config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'AppConnection' atau 'DefaultConnection' tidak ditemukan.");

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<RowVersionInterceptor>();
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            UseProvider(options, provider, connectionString);
            options.AddInterceptors(
                new SqlitePragmaInterceptor(),
                sp.GetRequiredService<RowVersionInterceptor>(),
                sp.GetRequiredService<AuditInterceptor>());
        });
        services.AddScoped<IAppReadDb>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IFormVersionResolver, FormVersionResolver>();
        services.AddScoped<ISubmissionQueries, SubmissionQueries>();
        services.AddScoped<IAnswerWriter, AnswerWriter>();
        services.AddScoped<ISubmissionWorkflow, SubmissionWorkflow>();
        services.AddScoped<IConsolidationQuery, ConsolidationQuery>();

        services.AddSingleton<IExcelImporter, DataKonsolidasiImporter>();
        services.AddSingleton<IConsolidationExporter, ClosedXmlConsolidationExporter>();
        services
            .AddCardWriter<NarrativeCardWriter>()
            .AddCardWriter<QuantitativeCardWriter>();

        return services;
    }

    public static IServiceCollection AddCardWriter<TWriter>(this IServiceCollection services)
        where TWriter : class, ICardWriter
        => services.AddSingleton<ICardWriter, TWriter>();

    // Provider dipilih lewat konfigurasi "Database:Provider". Pindah ke SQL Server atau PostgreSQL
    // berarti menambah paket provider dan satu cabang di sini.
    private static void UseProvider(DbContextOptionsBuilder options, string provider, string connectionString)
    {
        if (!string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Provider database '{provider}' belum didukung.");

        options.UseSqlite(connectionString);
    }
}

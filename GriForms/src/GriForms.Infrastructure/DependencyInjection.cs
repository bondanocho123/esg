using GriForms.Application.Abstractions;
using GriForms.Application.Tenancy;
using GriForms.Infrastructure.Consolidation;
using GriForms.Infrastructure.Excel;
using GriForms.Infrastructure.Forms;
using GriForms.Infrastructure.Persistence;
using GriForms.Infrastructure.Submissions;
using GriForms.Infrastructure.Telemetry;
using GriForms.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GriForms.Infrastructure;

public static class DependencyInjection
{
    // ICurrentUser dan IUserAccounts didaftarkan oleh host (Web), karena identitas pengguna bergantung pada cara login
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["Database:Provider"] ?? "Sqlite";
        var connectionString = config.GetConnectionString("AppConnection")
            ?? config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'AppConnection' atau 'DefaultConnection' tidak ditemukan.");

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<RowVersionInterceptor>();
        services.AddScoped<AuditInterceptor>();

        // Tenant aktif per circuit atau request; diisi TenantSession, dibaca filter tenant di AppDbContext
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantSession, TenantSession>();
        services.AddScoped<IUserProfiles, UserProfiles>();
        services.AddScoped<IOrganizationAdmin, OrganizationAdmin>();
        services.AddScoped<ITenantSettings, TenantSettings>();
        services.AddScoped<IFormCatalog, FormCatalog>();
        services.AddScoped<IPlatformBootstrap, PlatformBootstrapper>();
        services.AddSingleton(ReadBootstrapOptions(config));

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
        services.AddScoped<IReportingWorkspace, ReportingWorkspace>();
        services.AddScoped<IReportingPeriods, ReportingPeriods>();
        services.AddScoped<IConsolidationQuery, ConsolidationQuery>();
        services.AddScoped<ITelemetryIngest, TelemetryIngest>();
        services.AddScoped<ITelemetryQueries, TelemetryQueries>();
        services.AddScoped<ITelemetryDevices, TelemetryDevices>();

        services.AddSingleton<IExcelImporter, DataKonsolidasiImporter>();
        services.AddScoped<ILegacySiteMapper, LegacySiteMapper>();
        services.AddSingleton<IConsolidationExporter, ClosedXmlConsolidationExporter>();
        services
            .AddCardWriter<NarrativeCardWriter>()
            .AddCardWriter<QuantitativeCardWriter>();

        return services;
    }

    public static IServiceCollection AddCardWriter<TWriter>(this IServiceCollection services)
        where TWriter : class, ICardWriter
        => services.AddSingleton<ICardWriter, TWriter>();

    // Bootstrap:PlatformAdmin:{Email,Password} dan Bootstrap:TenantAdmins:{KodeTenant} (daftar email, atau satu
    // string dipisah koma). Diisi lewat environment atau user-secrets, bukan appsettings.json yang di-commit.
    private static BootstrapOptions ReadBootstrapOptions(IConfiguration config)
    {
        var tenantAdmins = config.GetSection("Bootstrap:TenantAdmins").GetChildren()
            .ToDictionary(
                tenant => tenant.Key,
                tenant => (IReadOnlyList<string>)(tenant.Value is { } list
                        ? list.Split(new[] { ',', ';' },StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        : tenant.GetChildren().Select(e => e.Value?.Trim()).OfType<string>().Where(e => e.Length > 0))
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        return new BootstrapOptions(
            config["Bootstrap:PlatformAdmin:Email"],
            config["Bootstrap:PlatformAdmin:Password"],
            tenantAdmins);
    }

    // Provider dipilih lewat konfigurasi "Database:Provider". Pindah ke SQL Server atau PostgreSQL
    // berarti menambah paket provider dan satu cabang di sini.
    private static void UseProvider(DbContextOptionsBuilder options, string provider, string connectionString)
    {
        if (!string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Provider database '{provider}' belum didukung.");

        options.UseSqlite(connectionString);
    }
}

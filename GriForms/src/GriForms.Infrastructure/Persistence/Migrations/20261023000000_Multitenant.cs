using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GriForms.Infrastructure.Persistence.Migrations
{
    // Multitenant: semua data lama menjadi milik tenant 1 (Z Resource, ZRES). Migration ini tidak menghapus peran apa pun;
    // pencabutan peran PlatformAdmin dilakukan bootstrap saat startup (PlatformBootstrapper).
    // Tabel telemetry belum punya migration sebelumnya, jadi dibuat di sini langsung dengan TenantId.
    // SQLite: AddForeignKey ke tabel lama membuat EF membangun ulang tabel itu di akhir migration. Backup app.db dulu.
    /// <inheritdoc />
    public partial class Multitenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Code", "IsActive", "Name" },
                values: new object[] { 1, "ZRES", true, "Z Resource" });

            // Baris lama otomatis milik tenant 1 lewat nilai default
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AuditEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ConsolidationSnapshots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ReportingPeriods",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Sites",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Submissions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            // Termasuk Admin dengan SiteId null: semua peran lama tetap ada, menjadi peran di tenant 1
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "UserSiteRoles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Sites",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Code site lama = Id lama (Site_ID 1-6), agar file Excel lama tetap bisa diimport
            migrationBuilder.Sql("UPDATE \"Sites\" SET \"Code\" = CAST(\"Id\" AS TEXT) WHERE \"Code\" = '';");

            migrationBuilder.DropIndex(
                name: "IX_ReportingPeriods_Year",
                table: "ReportingPeriods");

            migrationBuilder.CreateTable(
                name: "PlatformAdmins",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformAdmins", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ActiveTenantId = table.Column<int>(type: "INTEGER", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Tenants_ActiveTenantId",
                        column: x => x.ActiveTenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TelemetryDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ApiKeyHash = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelemetryDevices_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelemetryDevices_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryReadings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Metric = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelemetryReadings_TelemetryDevices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "TelemetryDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Code",
                table: "Tenants",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_TenantId",
                table: "AuditEntries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidationSnapshots_TenantId",
                table: "ConsolidationSnapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingPeriods_TenantId_Year",
                table: "ReportingPeriods",
                columns: new[] { "TenantId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_TenantId_Code",
                table: "Sites",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_TenantId",
                table: "Submissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSiteRoles_TenantId",
                table: "UserSiteRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_ActiveTenantId",
                table: "UserProfiles",
                column: "ActiveTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryDevices_ApiKeyHash",
                table: "TelemetryDevices",
                column: "ApiKeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryDevices_SiteId",
                table: "TelemetryDevices",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryDevices_TenantId_Code",
                table: "TelemetryDevices",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryReadings_DeviceId_Metric_RecordedAt",
                table: "TelemetryReadings",
                columns: new[] { "DeviceId", "Metric", "RecordedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_Tenants_TenantId",
                table: "AuditEntries",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsolidationSnapshots_Tenants_TenantId",
                table: "ConsolidationSnapshots",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportingPeriods_Tenants_TenantId",
                table: "ReportingPeriods",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_Tenants_TenantId",
                table: "Sites",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Tenants_TenantId",
                table: "Submissions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSiteRoles_Tenants_TenantId",
                table: "UserSiteRoles",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_Tenants_TenantId",
                table: "AuditEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsolidationSnapshots_Tenants_TenantId",
                table: "ConsolidationSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportingPeriods_Tenants_TenantId",
                table: "ReportingPeriods");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_Tenants_TenantId",
                table: "Sites");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Tenants_TenantId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSiteRoles_Tenants_TenantId",
                table: "UserSiteRoles");

            migrationBuilder.DropTable(
                name: "PlatformAdmins");

            migrationBuilder.DropTable(
                name: "TelemetryReadings");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "TelemetryDevices");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_UserSiteRoles_TenantId",
                table: "UserSiteRoles");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_TenantId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Sites_TenantId_Code",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_ReportingPeriods_TenantId_Year",
                table: "ReportingPeriods");

            migrationBuilder.DropIndex(
                name: "IX_ConsolidationSnapshots_TenantId",
                table: "ConsolidationSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_AuditEntries_TenantId",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "UserSiteRoles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ReportingPeriods");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ConsolidationSnapshots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AuditEntries");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingPeriods_Year",
                table: "ReportingPeriods",
                column: "Year",
                unique: true);
        }
    }
}

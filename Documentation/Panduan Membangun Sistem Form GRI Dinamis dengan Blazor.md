# Panduan Membangun Sistem Form GRI Dinamis dengan Blazor

Oct 22, 2026 · @Bondan

## 1. Tujuan dan ruang lingkup

Dokumen ini memandu pembangunan aplikasi web Blazor dari nol yang menggantikan 138 file Excel (23 topik × 6 site) dan Konsolidasi Runner dengan satu sistem, di mana definisi form GRI bisa dikonfigurasi dan diberi versi tanpa deploy ulang.

Pemetaan dari cara kerja sekarang ke aplikasi:

| Sekarang (Excel + Runner) | Di aplikasi Blazor |
| --- | --- |
| 23 file sumber per site, 138 file total | Satu definisi form per topik, jawaban disimpan per site dan tahun |
| Sel kuning di sheet disclosure | Field form yang dirender dari definisi |
| Sheet Data-Konsolidasi (12 kolom A–L) | Tabel `Answer` dengan kontrak kolom yang sama |
| set-cover menulis COVER!F11 dan F12 | Site dan tahun berasal dari konteks login dan pilihan periode, tidak diketik |
| Blok Otorisasi Data di COVER | Workflow status Draft, Reviewed, Approved |
| Konsolidasi Runner dan recalc via Excel | Query konsolidasi di server dan export ke Excel |
| KARANTINA (Site\_ID tidak cocok folder) | Hilang dengan sendirinya karena foreign key |

### Prinsip desain

1. Skema jawaban stabil, definisi form dinamis. Hanya definisi yang sering berubah mengikuti GRI.
2. Definisi form diberi versi dan periode berlaku, sehingga data 2025 tetap terbaca dengan definisi 2025.
3. Tipe input dibatasi pada set yang terkontrol, bukan form builder bebas.
4. Konsolidasi hanya mengambil data berstatus Approved.
5. Excel tetap dipakai sebagai format import dan export, bukan tempat menyimpan data sumber.

### Di luar ruang lingkup

Form builder drag-and-drop bebas, aplikasi mobile native, dan integrasi ke sistem pelaporan eksternal. Semuanya bisa ditambahkan setelah fase inti stabil.

## 2. Keputusan arsitektur

Pakai Blazor Web App di .NET 10 (LTS) dengan render mode Interactive Server, EF Core untuk akses data, dan SQLite sebagai database awal. Pilihan ini paling cepat dibangun oleh tim .NET dan mudah dirawat, dengan satu catatan soal konektivitas site yang dibahas di bawah tabel.

| Keputusan | Pilihan | Alasan |
| --- | --- | --- |
| Model Blazor | Blazor Web App, Interactive Server | Logika form dinamis tetap di server, tidak ada unduhan WebAssembly, pengguna internal dalam jumlah terbatas |
| Database | SQLite untuk tahap awal (satu file, tanpa server database), dengan jalur pindah ke SQL Server atau PostgreSQL | Jawaban disimpan relasional, definisi form disimpan sebagai JSON |
| Akses data | EF Core dengan migrations | Skema berversi bersama kode, mendukung kolom JSON |
| Komponen UI | MudBlazor (alternatif: Radzen, Fluent UI Blazor) | Tabel, dialog, form, dan validasi siap pakai, lisensi open source |
| Autentikasi | ASP.NET Core Identity, atau OpenID Connect jika perusahaan sudah punya identity provider | Peran dan akses per site dikelola di aplikasi |
| Excel (import dan export) | ClosedXML | Lisensi MIT, cukup untuk baca tulis sel tanpa Excel terpasang |
| Validasi | FluentValidation ditambah aturan dari definisi form | Aturan umum di kode, aturan per pertanyaan di konfigurasi |
| Pekerjaan latar belakang | `BackgroundService`, atau Hangfire bila perlu dashboard job | Konsolidasi dan import tidak memblokir UI |
| Logging | Serilog ke file dan tabel | Menggantikan log \[PERINGATAN\], \[GAGAL\], \[KARANTINA\] di Runner |
| Testing | xUnit, bUnit, SQLite in-memory | Unit, komponen Blazor, dan integrasi database sungguhan |

### Catatan konektivitas site

Interactive Server bergantung pada koneksi SignalR yang terus terbuka. Site tambang sering punya jaringan tidak stabil, jadi dua pengaman perlu ada sejak awal:

- Simpan draft otomatis per field atau per kelompok field, sehingga koneksi putus tidak menghilangkan isian.
- Tampilkan status koneksi dan UI reconnect yang jelas.

Jika setelah uji lapangan koneksi tetap bermasalah, bagian form bisa dipindah ke render mode WebAssembly atau Auto tanpa mengubah model data, karena logika bisnis ada di layer Application yang terpisah dari UI.

### Catatan SQLite

SQLite cukup untuk tahap awal: pengguna terbatas pada enam site, satu instance aplikasi, dan tidak ada server database yang dirawat. Beberapa batasannya mempengaruhi desain, jadi tangani sejak awal:

- **Satu penulis pada satu waktu.** Aktifkan mode WAL (`PRAGMA journal_mode=WAL`) dan atur `busy_timeout` pada connection string atau saat koneksi dibuka. Beban simpan otomatis per sel dari puluhan pengguna masih wajar, asalkan simpan dikelompokkan (bab 8).
- **Decimal disimpan sebagai TEXT.** EF Core tidak bisa menjumlah atau membandingkan `decimal` di sisi database pada SQLite. Hitung total dan agregasi di memori dari DTO konsolidasi (bab 12), jangan lewat `Sum()` di query.
- **Tidak ada rowversion otomatis.** Pakai kolom versi manual sebagai concurrency token (bab 8).
- **`ALTER TABLE` terbatas.** EF Core membangun ulang tabel saat migrations tertentu. Tinjau migrations sebelum diterapkan ke file produksi, dan backup dulu.
- **View dan seed lewat migrations.** Buat `vw_DataKonsolidasi` dengan `migrationBuilder.Sql(...)`.
- **Satu instance, disk lokal.** Letakkan file `.db` di disk lokal server, bukan di network share atau folder yang disinkronkan OneDrive, karena penguncian file tidak andal di sana.

Jaga jalur pindah tetap terbuka: jangan memakai SQL khusus SQLite di luar migrations, dan pilih provider lewat konfigurasi. Pindah ke SQL Server atau PostgreSQL saat penulisan bersamaan mulai sering terjadi, aplikasi perlu lebih dari satu instance, atau database butuh failover.

## 3. Prasyarat dan setup solution

Setup dari nol selesai dengan sekitar sepuluh perintah `dotnet` CLI dan menghasilkan solution empat proyek ditambah proyek test.

### Prasyarat

- .NET 10 SDK
- Visual Studio terbaru, atau VS Code dengan C# Dev Kit
- SQLite (tidak perlu instalasi server, file database dibuat otomatis)
- Git, dan satu repositori untuk seluruh solution
- Tool EF Core: `dotnet tool install --global dotnet-ef`

### Membuat solution dan proyek

```bash
mkdir GriForms && cd GriForms
git init
dotnet new sln -n GriForms

dotnet new classlib -n GriForms.Domain         -o src/GriForms.Domain
dotnet new classlib -n GriForms.Application    -o src/GriForms.Application
dotnet new classlib -n GriForms.Infrastructure -o src/GriForms.Infrastructure
dotnet new blazor   -n GriForms.Web            -o src/GriForms.Web --interactivity Server --all-interactive --auth Individual

dotnet new xunit -n GriForms.Tests -o tests/GriForms.Tests

dotnet sln add src/GriForms.Domain src/GriForms.Application src/GriForms.Infrastructure src/GriForms.Web tests/GriForms.Tests
```

### Referensi antar proyek

Arah dependensi satu arah ke dalam: Web dan Infrastructure bergantung ke Application, Application ke Domain, dan Domain tidak bergantung ke apa pun.

```bash
dotnet add src/GriForms.Application    reference src/GriForms.Domain
dotnet add src/GriForms.Infrastructure reference src/GriForms.Application
dotnet add src/GriForms.Web            reference src/GriForms.Application src/GriForms.Infrastructure
dotnet add tests/GriForms.Tests        reference src/GriForms.Application src/GriForms.Infrastructure src/GriForms.Web
```

### Paket NuGet

```bash
dotnet add src/GriForms.Application    package FluentValidation
dotnet add src/GriForms.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/GriForms.Infrastructure package ClosedXML
dotnet add src/GriForms.Web            package Microsoft.EntityFrameworkCore.Design
dotnet add src/GriForms.Web            package MudBlazor
dotnet add src/GriForms.Web            package Serilog.AspNetCore

dotnet add tests/GriForms.Tests package bunit
dotnet add tests/GriForms.Tests package Microsoft.EntityFrameworkCore.Sqlite
```

Template Blazor dengan `--auth Individual` sudah memakai SQLite untuk Identity, jadi satu file database bisa menampung tabel aplikasi dan tabel akun. Saat nanti pindah ke SQL Server atau PostgreSQL, ganti paket provider EF Core, ubah connection string, dan buat migrations ulang untuk provider itu.

### Titik awal `Program.cs`

Kerangka berikut cukup untuk menjalankan aplikasi kosong dengan MudBlazor dan Serilog. Registrasi layanan lain ditambahkan di bab berikutnya.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices();

// Bab 4 dan seterusnya: AddApplication(), AddInfrastructure(builder.Configuration)

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
```

Jalankan `dotnet run --project src/GriForms.Web` untuk memastikan halaman awal tampil sebelum lanjut.

## 4. Struktur solution dan layering

Bagi kode menjadi empat layer dengan aturan satu arah: UI tidak menyentuh `DbContext` langsung, semua lewat service di layer Application. Aturan ini yang membuat render mode Blazor bisa diganti nanti tanpa menulis ulang logika.

&#91;embedded content: layering solution · 4 proyek\]

Panah menunjuk ke layer yang diandalkan. Hanya proyek Web yang mengenal semua layer, karena ia yang merakit dependency injection di `Program.cs`.

```text
src/
  GriForms.Domain/            entitas dan aturan inti, tanpa dependensi luar
    Entities/                 Site, ReportingPeriod, FormDefinition, FormVersion,
                              Submission, Answer, AuditEntry
    Enums/                    InputType, SubmissionStatus, RoleName
  GriForms.Application/       use case, DTO, interface, validasi
    Forms/                    model definisi form, IFormDefinitionService
    Submissions/              simpan draft, ajukan, review, setujui
    Validation/               mesin validasi dari definisi
    Consolidation/            query konsolidasi lintas site
    Abstractions/             IAppDbContext, IExcelService, IClock, ICurrentUser
  GriForms.Infrastructure/    implementasi teknis
    Persistence/              AppDbContext, konfigurasi EF, migrations, seed
    Excel/                    import dan export dengan ClosedXML
    Identity/                 ICurrentUser dan penetapan akses per site
    Jobs/                     konsolidasi dan import sebagai background job
  GriForms.Web/               Blazor Web App
    Components/Pages/         halaman: daftar form, isi form, review, konsolidasi
    Components/Forms/         DynamicForm dan renderer per tipe input
    Components/Layout/        layout, navigasi, status koneksi
tests/
  GriForms.Tests/             unit, bUnit, integrasi
```

| Layer | Tanggung jawab | Tidak boleh |
| --- | --- | --- |
| Domain | Entitas, enum, aturan yang tidak bergantung teknologi | Mereferensi EF Core, Blazor, atau library lain |
| Application | Use case, kontrak (interface), validasi, query konsolidasi | Mengetahui detail database, Excel, atau HTML |
| Infrastructure | EF Core, ClosedXML, Identity, job | Berisi aturan bisnis |
| Web | Halaman, komponen, binding, navigasi | Memanggil `DbContext` atau menulis query langsung |

Untuk mendaftarkan layanan, buat satu extension method per layer (`AddApplication()` dan `AddInfrastructure(config)`) dan panggil keduanya di `Program.cs`.

## 5. Model domain dan skema database

Inti skema adalah tabel `Answer` yang kolomnya setara 12 kolom Data-Konsolidasi, ditambah `Submission` yang menggantikan satu file Excel. Aturan "satu file = satu topik = satu site = satu tahun" menjadi unique index pada `Submission`.

| Tabel | Isi | Catatan |
| --- | --- | --- |
| `Site` | Id (1 sampai 6), Name, Location, IsActive | Id sama dengan Site\_ID sekarang, di-seed |
| `ReportingPeriod` | Id, Year, Status (Open, Closed) | Periode ditutup setelah konsolidasi final |
| `Topic` | Id, Code (mis. 403), Category (Env, Soc, GRI 14), Name | 23 topik sekarang, di-seed dari Lampiran L1 |
| `FormVersion` | Id, TopicId, VersionNo, StandardRef, EffectiveFromYear, EffectiveToYear, DefinitionJson, IsPublished | Definisi form disimpan sebagai JSON (bab 6 dan 7) |
| `Submission` | Id, SiteId, PeriodId, TopicId, FormVersionId, Status | Unique (SiteId, PeriodId, TopicId) |
| `Answer` | Id, SubmissionId, DisclosureNo, QuestionKey, PeriodKey, NumberValue, StringValue, Unit, ValueState, UpdatedBy, UpdatedAt | Satu baris per sel jawaban |
| `Signoff` | SubmissionId, Role, UserId, DisplayName, JobTitle, SignedAt | Pengganti blok Otorisasi Data di COVER |
| `AuditEntry` | Id, EntityType, EntityId, Action, OldValue, NewValue, UserId, At | Jejak perubahan untuk audit |

### Entitas inti

```csharp
public enum SubmissionStatus { Draft = 0, Submitted = 1, Reviewed = 2, Approved = 3, Rejected = 4 }
public enum ValueState { Filled = 0, NotApplicable = 1 }   // "belum diisi" = tidak ada baris Answer

public class Submission
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    public int PeriodId { get; set; }
    public int TopicId { get; set; }
    public int FormVersionId { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;

    public Site Site { get; set; } = null!;
    public ReportingPeriod Period { get; set; } = null!;
    public Topic Topic { get; set; } = null!;
    public FormVersion FormVersion { get; set; } = null!;
    public List<Answer> Answers { get; set; } = new();
    public List<Signoff> Signoffs { get; set; } = new();
}

public class Answer
{
    public long Id { get; set; }
    public int SubmissionId { get; set; }
    public string DisclosureNo { get; set; } = "";   // No_Qz, mis. 403-5
    public string QuestionKey { get; set; } = "";    // kunci stabil dari definisi
    public string? PeriodKey { get; set; }           // "2026-01" untuk bulanan, null untuk tahunan
    public decimal? NumberValue { get; set; }
    public string? StringValue { get; set; }
    public string? Unit { get; set; }
    public ValueState ValueState { get; set; } = ValueState.Filled;  // Filled atau NotApplicable
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Konfigurasi EF Core

```csharp
protected override void OnModelCreating(ModelBuilder b)
{
    b.Entity<Submission>()
        .HasIndex(s => new { s.SiteId, s.PeriodId, s.TopicId })
        .IsUnique();

    b.Entity<Answer>()
        .HasIndex(a => new { a.SubmissionId, a.QuestionKey, a.PeriodKey })
        .IsUnique();

    // SQLite menyimpan decimal sebagai TEXT; total dihitung di memori (lihat catatan SQLite di bab 2)
    b.Entity<FormVersion>().Property(f => f.DefinitionJson).IsRequired();

    // Data referensi di-seed: 6 site dan 23 topik
    b.Entity<Site>().HasData(
        new Site { Id = 1, Name = "Head Office" },
        new Site { Id = 2, Name = "Bakan Mine" },
        new Site { Id = 3, Name = "Penjom Mine" },
        new Site { Id = 4, Name = "Seruyung Mine" },
        new Site { Id = 5, Name = "North Lanut Mine" },
        new Site { Id = 6, Name = "Doup Mine" });
}
```

### Kontrak 12 kolom sebagai view

Agar laporan dan export tetap memakai kontrak A sampai L yang sudah dikenal tim pelaporan, buat view `vw_DataKonsolidasi` yang menggabungkan `Answer`, `Submission`, `Site`, `Topic`, dan definisi pertanyaan. Kolom `Qz` (teks pertanyaan) diambil dari definisi form versi yang dipakai submission, bukan disalin ke tiap baris jawaban.

| Kolom view | Sumber |
| --- | --- |
| Site\_ID, Site\_Name, Site\_Location | `Site` |
| Tahun | `ReportingPeriod.Year` |
| Kategori, Topic\_Name | `Topic` |
| No\_Qz, Qz | `Answer.DisclosureNo` dan teks pertanyaan dari definisi |
| Period | `Answer.PeriodKey` |
| Number\_Value, String\_Value, Unit | `Answer` |

## 6. Definisi form (JSON)

Setiap topik GRI didefinisikan sebagai satu dokumen JSON berhierarki topik, disclosure, lalu pertanyaan, dan disimpan di `FormVersion.DefinitionJson`. Renderer Blazor, validasi, import Excel, dan export semuanya membaca dokumen yang sama, sehingga mengubah standar berarti mengubah JSON, bukan kode.

### Contoh: potongan topik 403

Contoh berikut ilustrasi struktur. Teks pertanyaan resmi tetap harus diambil dari standar GRI yang berlaku.

```json
{
  "formCode": "403",
  "title": "403 Keselamatan dan Kesehatan Kerja",
  "standardRef": "GRI 403: Occupational Health and Safety",
  "versionNo": 1,
  "effectiveFromYear": 2025,
  "effectiveToYear": null,
  "disclosures": [
    {
      "no": "403-5",
      "title": "Pelatihan pekerja tentang K3",
      "questions": [
        {
          "key": "403-5-A",
          "type": "text",
          "label": "Deskripsi tentang pelatihan kesehatan dan keselamatan",
          "helpText": "Sebutkan pelatihan umum dan pelatihan bahaya khusus pekerjaan.",
          "required": true,
          "validation": { "maxLength": 4000 }
        },
        {
          "key": "403-5-B",
          "type": "monthlyTable",
          "label": "Jumlah peserta pelatihan per bulan",
          "valueType": "number",
          "unit": "orang",
          "rows": [ { "key": "k3-dasar", "label": "Pelatihan K3 Dasar" } ],
          "allowCustomRows": true,
          "validation": { "min": 0, "integerOnly": true }
        }
      ]
    }
  ]
}
```

### Tipe input yang didukung

Batasi tipe pada daftar berikut. Menambah tipe baru berarti menambah satu komponen renderer, bukan mengubah skema jawaban.

| Tipe | Dipakai untuk | Disimpan ke `Answer` |
| --- | --- | --- |
| `text` | Pertanyaan naratif (uraian per site) | `StringValue` |
| `number` | Satu angka tahunan dengan unit | `NumberValue`, `Unit` |
| `select` | Pilihan tunggal dari daftar | `StringValue` |
| `boolean` | Ya atau tidak | `StringValue` (`true` atau `false`) atau `NumberValue` (1 atau 0), pilih satu dan konsisten |
| `monthlyTable` | Angka Jan sampai Des per baris | Satu `Answer` per sel, `PeriodKey` = `2026-01` dan seterusnya |
| `rowsTable` | Tabel baris dinamis dengan kolom tetap | Satu `Answer` per sel, `QuestionKey` memuat kunci baris dan kolom |

### Model C# untuk definisi

```csharp
public enum InputType { Text, Number, Select, Boolean, MonthlyTable, RowsTable }

public record FormDefinition(
    string FormCode, string Title, string StandardRef, int VersionNo,
    int EffectiveFromYear, int? EffectiveToYear,
    List<DisclosureDef> Disclosures);

public record DisclosureDef(string No, string Title, string? Description, List<QuestionDef> Questions);

public record QuestionDef(
    string Key, InputType Type, string Label,
    string? HelpText = null, bool Required = false,
    string? ValueType = null, string? Unit = null,
    List<OptionDef>? Options = null,
    List<RowDef>? Rows = null, bool AllowCustomRows = false,
    ValidationDef? Validation = null,
    string? VisibleWhen = null,   // mis. "403-1-A == 'true'"
    string? CalcRef = null);      // mis. "ghg_total_v2", rujukan ke kalkulasi di kode

public record OptionDef(string Value, string Label);
public record RowDef(string Key, string Label);
public record ValidationDef(decimal? Min = null, decimal? Max = null, bool IntegerOnly = false, int? MaxLength = null);
```

Baca JSON dengan `System.Text.Json` memakai `JsonSerializerDefaults.Web` dan `JsonStringEnumConverter`.

### Aturan yang menjaga data tetap bisa dikonsolidasikan

1. `key` pertanyaan tidak pernah diubah atau dipakai ulang setelah dipublikasikan. Mengubah makna pertanyaan berarti `key` baru.
2. Label dan teks bantuan boleh berubah antar versi, karena jawaban merujuk ke `key`, bukan ke label.
3. Definisi divalidasi terhadap JSON Schema atau validator di kode sebelum boleh dipublikasikan: `key` unik, tipe dikenal, opsi tidak kosong untuk `select`.
4. Definisi yang sudah dipublikasikan dan dipakai submission tidak boleh diedit. Perubahan selalu lewat versi baru (bab 7).

### Mengganti konvensi 0, "-", dan kosong

Panduan Pengguna sekarang meminta pengguna membedakan 0, tanda "-", dan sel kosong secara manual. Di aplikasi, bedakan lewat status eksplisit per pertanyaan: *Terisi*, *Tidak berlaku*, atau *Belum diisi*. Tambahkan kolom `ValueState` pada `Answer`; angka 0 tetap berarti nol sungguhan, dan pertanyaan yang tidak berlaku ditandai lewat pilihan "Tidak berlaku" di UI, bukan lewat mengetik "-".

## 7. Versioning dan pemetaan antar versi

Revisi standar GRI selalu menghasilkan `FormVersion` baru, tidak pernah mengedit versi yang sudah dipakai. Dengan begitu data 2025 tetap dibaca memakai definisi 2025, dan data 2026 memakai definisi baru, hal yang sekarang tidak bisa dijamin oleh file Excel yang ditimpa.

### Siklus hidup versi

1. **Draft**: bisa diedit bebas oleh admin, belum terlihat pengguna site.
2. **Published**: terkunci dan tidak bisa diedit, bisa dipakai submission.
3. **Retired**: tidak dipakai untuk submission baru, tetap terbaca untuk data lama.

Begitu `Submission` dibuat, `FormVersionId`-nya dikunci. Memilih versi untuk submission baru dilakukan satu kali, saat submission dibuat, dengan aturan: versi Published untuk topik tersebut yang `EffectiveFromYear` ≤ tahun dan `EffectiveToYear` kosong atau ≥ tahun, ambil `VersionNo` tertinggi.

```csharp
public async Task<FormVersion> ResolveAsync(int topicId, int year, CancellationToken ct)
{
    var version = await _db.FormVersions
        .Where(v => v.TopicId == topicId
                 && v.IsPublished
                 && v.EffectiveFromYear <= year
                 && (v.EffectiveToYear == null || v.EffectiveToYear >= year))
        .OrderByDescending(v => v.VersionNo)
        .FirstOrDefaultAsync(ct);

    return version ?? throw new InvalidOperationException(
        $"Tidak ada versi form terbit untuk topik {topicId} tahun {year}.");
}
```

### Dampak tiap jenis perubahan

| Perubahan | Cara menanganinya | Dampak ke data lama |
| --- | --- | --- |
| Ubah label atau teks bantuan | Versi baru, `key` sama | Tidak ada, jawaban merujuk ke `key` |
| Tambah pertanyaan | Versi baru, `key` baru | Submission lama tidak punya jawaban untuk `key` itu |
| Hapus pertanyaan | Hapus dari versi baru, `key` tidak dipakai ulang | Data lama tetap terbaca lewat versi lamanya |
| Ubah makna atau unit | `key` baru dan catat pemetaan | Tren antar tahun dibaca lewat pemetaan |
| Standar diganti total | Topik baru berjalan berdampingan dengan topik lama | Keduanya konsolidasi terpisah |

Kasus terakhir sudah terlihat di Lampiran L1: topik 103 (Energi, standar terbaru) dan 302 (Energi), serta 101 dan 304 untuk keanekaragaman hayati, berjalan berdampingan sebagai target konsolidasi terpisah.

### Tabel pemetaan pertanyaan

Untuk membandingkan tahun yang memakai versi berbeda, simpan pemetaan eksplisit:

```csharp
public class QuestionMapping
{
    public int Id { get; set; }
    public int FromFormVersionId { get; set; }
    public string FromKey { get; set; } = "";
    public int ToFormVersionId { get; set; }
    public string ToKey { get; set; } = "";
    public MappingKind Kind { get; set; }   // Direct, Sum, Split, NoEquivalent
    public string? Note { get; set; }
}
```

Pemetaan dipakai di dua tempat: laporan tren multi-tahun, dan fitur "upgrade submission" untuk memindahkan submission berstatus Draft ke versi baru selama periode masih Open. Submission yang sudah Approved tidak pernah dipindah.

### Alur mengubah definisi

1. Admin menyalin versi Published menjadi Draft baru.
2. Admin mengedit JSON (atau editor form yang membungkusnya) dan mengisi pemetaan untuk `key` yang berubah.
3. Sistem memvalidasi definisi dan menampilkan diff terhadap versi sebelumnya: `key` baru, `key` dihapus, tipe atau unit berubah.
4. Admin mempublikasikan dan menentukan `EffectiveFromYear`.
5. Aksi publish dicatat di `AuditEntry`.

## 8. Renderer form dinamis di Blazor

Renderer terdiri dari tiga bagian: registry yang memetakan `InputType` ke komponen, state form yang menyimpan semua jawaban sebagai kamus sel, dan komponen `DynamicForm` yang menelusuri definisi lalu merender satu komponen per pertanyaan lewat `DynamicComponent`. Menambah tipe input baru hanya menambah satu komponen dan satu baris registry.

### State jawaban

Setiap sel jawaban punya kunci `QuestionKey|PeriodKey` (bagian periode kosong untuk jawaban tahunan). Dengan kunci seperti itu, `number`, `text`, dan tabel bulanan memakai satu mekanisme penyimpanan yang sama.

```csharp
public record AnswerValue(string? Text, decimal? Number, ValueState State = ValueState.Filled);

public class FormState
{
    private readonly Dictionary<string, AnswerValue> _cells = new();
    private readonly HashSet<string> _dirty = new();

    public event Action? Changed;

    public static string CellKey(string questionKey, string? periodKey = null)
        => periodKey is null ? questionKey : $"{questionKey}|{periodKey}";

    public AnswerValue? Get(string questionKey, string? periodKey = null)
        => _cells.GetValueOrDefault(CellKey(questionKey, periodKey));

    public void Set(string questionKey, string? periodKey, AnswerValue? value)
    {
        var key = CellKey(questionKey, periodKey);
        if (value is null) _cells.Remove(key); else _cells[key] = value;
        _dirty.Add(key);
        Changed?.Invoke();
    }

    public IReadOnlyCollection<string> TakeDirty()
    {
        var copy = _dirty.ToArray();
        _dirty.Clear();
        return copy;
    }
}
```

### Registry tipe ke komponen

```csharp
public static class FieldRegistry
{
    public static readonly IReadOnlyDictionary<InputType, Type> Map =
        new Dictionary<InputType, Type>
        {
            [InputType.Text]         = typeof(TextField),
            [InputType.Number]       = typeof(NumberField),
            [InputType.Select]       = typeof(SelectField),
            [InputType.Boolean]      = typeof(BooleanField),
            [InputType.MonthlyTable] = typeof(MonthlyTableField),
            [InputType.RowsTable]    = typeof(RowsTableField),
        };
}
```

### Komponen dasar dan contoh satu field

Semua field mewarisi satu kelas dasar sehingga `DynamicComponent` bisa memberi parameter yang seragam.

```csharp
public abstract class FieldBase : ComponentBase
{
    [Parameter, EditorRequired] public QuestionDef Question { get; set; } = null!;
    [Parameter, EditorRequired] public FormState State { get; set; } = null!;
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
}
```

```razor
@* NumberField.razor *@
@inherits FieldBase

<MudNumericField T="decimal?"
                 Label="@Question.Label"
                 HelperText="@Question.HelpText"
                 Value="@State.Get(Question.Key)?.Number"
                 ValueChanged="OnChanged"
                 Disabled="@(ReadOnly || NotApplicable)"
                 Error="@(Errors.Count > 0)"
                 ErrorText="@string.Join(" ", Errors)"
                 Adornment="Adornment.End"
                 AdornmentText="@Question.Unit" />

<MudCheckBox T="bool"
             Label="Tidak berlaku untuk site ini"
             Value="NotApplicable"
             ValueChanged="OnNotApplicableChanged"
             Disabled="ReadOnly" />

@code {
    private bool NotApplicable => State.Get(Question.Key)?.State == ValueState.NotApplicable;

    private void OnChanged(decimal? v) =>
        State.Set(Question.Key, null, v is null ? null : new AnswerValue(null, v));

    private void OnNotApplicableChanged(bool na) =>
        State.Set(Question.Key, null, na ? new AnswerValue(null, null, ValueState.NotApplicable) : null);
}
```

### Tabel bulanan

`MonthlyTableField` menggambar baris dari `Question.Rows` (ditambah baris kustom bila `AllowCustomRows`) dan 12 kolom bulan. Kunci sel memakai `QuestionKey` yang memuat kunci baris, misalnya `403-5-B/k3-dasar`, dan `PeriodKey` berformat `yyyy-MM`.

```razor
@* MonthlyTableField.razor, ringkas *@
@inherits FieldBase
@inject IFormContext Ctx

<MudText Typo="Typo.subtitle1">@Question.Label (@Question.Unit)</MudText>
<MudSimpleTable Dense="true">
    <thead>
        <tr>
            <th>Baris</th>
            @for (var m = 1; m <= 12; m++) { <th>@MonthLabel(m)</th> }
        </tr>
    </thead>
    <tbody>
        @foreach (var row in Rows)
        {
            <tr @key="row.Key">
                <td>@row.Label</td>
                @for (var m = 1; m <= 12; m++)
                {
                    var month = m;
                    <td>
                        <MudNumericField T="decimal?" Variant="Variant.Text" HideSpinButtons="true"
                                         Disabled="ReadOnly"
                                         Value="@State.Get($"{Question.Key}/{row.Key}", PeriodKey(month))?.Number"
                                         ValueChanged="v => Set(row.Key, month, v)" />
                    </td>
                }
            </tr>
        }
    </tbody>
</MudSimpleTable>

@code {
    private IEnumerable<RowDef> Rows => Question.Rows ?? new List<RowDef>();
    private string PeriodKey(int m) => $"{Ctx.Year}-{m:00}";
    private static string MonthLabel(int m) => new DateTime(2000, m, 1).ToString("MMM");

    private void Set(string rowKey, int month, decimal? v) =>
        State.Set($"{Question.Key}/{rowKey}", PeriodKey(month), v is null ? null : new AnswerValue(null, v));
}
```

Fitur yang layak ditambahkan setelah versi dasar berjalan: tempel (paste) blok angka dari Excel ke tabel, total per baris dan per kolom, dan penambahan baris kustom.

### Komponen DynamicForm

```razor
@* DynamicForm.razor *@
@foreach (var d in Definition.Disclosures)
{
    <MudPaper Class="pa-4 mb-4" @key="d.No">
        <MudText Typo="Typo.h6">@d.No - @d.Title</MudText>

        @foreach (var q in d.Questions.Where(VisibleRules.IsVisible(State)))
        {
            <DynamicComponent @key="q.Key"
                              Type="FieldRegistry.Map[q.Type]"
                              Parameters="ParametersFor(q)" />
        }
    </MudPaper>
}

@code {
    [Parameter, EditorRequired] public FormDefinition Definition { get; set; } = null!;
    [Parameter, EditorRequired] public FormState State { get; set; } = null!;
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public ValidationResult Validation { get; set; } = ValidationResult.Empty;

    private IDictionary<string, object?> ParametersFor(QuestionDef q) => new Dictionary<string, object?>
    {
        [nameof(FieldBase.Question)] = q,
        [nameof(FieldBase.State)]    = State,
        [nameof(FieldBase.ReadOnly)] = ReadOnly,
        [nameof(FieldBase.Errors)]   = Validation.For(q.Key),
    };
}
```

### Simpan otomatis

Halaman yang memuat `DynamicForm` berlangganan ke `State.Changed` dan menyimpan sel yang berubah setelah jeda sekitar satu detik tanpa perubahan baru. Ini pengaman utama untuk koneksi tidak stabil di site (bab 2).

```csharp
private CancellationTokenSource? _debounce;

private void OnStateChanged()
{
    _debounce?.Cancel();
    _debounce = new CancellationTokenSource();
    var token = _debounce.Token;

    _ = InvokeAsync(async () =>
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(1), token);
            var cells = State.TakeDirty();
            await Submissions.SaveCellsAsync(SubmissionId, cells.Select(k => (k, State.GetByCellKey(k))), token);
            SavedAt = DateTime.Now;
            StateHasChanged();
        }
        catch (OperationCanceledException) { /* perubahan baru menggantikan */ }
    });
}
```

### Hal yang perlu diperhatikan

- **Aturan tampil (`VisibleWhen`)**: implementasikan evaluator kecil sendiri untuk sintaks terbatas seperti `kunci == 'nilai'`. Jangan mengevaluasi string dari JSON sebagai kode C#, karena definisi bisa diedit dan itu membuka celah eksekusi kode.
- **Pengisian bersamaan**: Contributor bisa sampai tiga orang pada satu file. SQLite tidak punya rowversion otomatis, jadi tambahkan kolom versi (angka yang dinaikkan tiap simpan dan ditandai concurrency token), misalnya `RowVersion` pada `Submission` atau `Answer` dan tangani `DbUpdateConcurrencyException` dengan pesan yang jelas, bukan menimpa diam-diam.
- **Form besar**: topik seperti emisi atau limbah bisa punya ratusan sel. Render per disclosure dalam panel yang bisa dilipat, dan kirim hanya sel yang berubah, bukan seluruh form.
- **Mode baca**: `ReadOnly` ditentukan oleh status submission dan peran pengguna (bab 10), bukan oleh komponen field itu sendiri.
- **Pengujian**: setiap komponen field diuji dengan bUnit, dan `DynamicForm` diuji dengan definisi contoh yang memuat semua tipe input.

## 9. Validasi dan kalkulasi

Validasi berjalan dua lapis: aturan per sel yang dibaca dari `ValidationDef` di definisi form, dan aturan lintas field serta kalkulasi yang berupa kode terkontrol dan dirujuk lewat `CalcRef`. Keduanya dijalankan di server, saat menyimpan dan sekali lagi sebelum submission diajukan, sehingga UI tidak bisa dilewati.

### Aturan per sel dari definisi

| Aturan | Sumber di definisi | Tingkat |
| --- | --- | --- |
| Wajib diisi (atau ditandai Tidak berlaku) | `required` | Error, memblokir pengajuan |
| Batas bawah dan atas angka | `validation.min`, `validation.max` | Error |
| Hanya bilangan bulat | `validation.integerOnly` | Error |
| Panjang teks maksimum | `validation.maxLength` | Error |
| Nilai `select` harus ada di daftar opsi | `options` | Error |
| Pertanyaan Tidak berlaku tidak boleh punya angka | `ValueState` | Error |

Satuan tidak diketik pengguna. Unit diambil dari `QuestionDef.Unit` saat jawaban disimpan, sehingga masalah "kg" versus "Kg" atau "ton" versus "ton metrik" yang diperingatkan Panduan Pengguna hilang dengan sendirinya, dan kolom `Unit` di hasil konsolidasi selalu seragam antar site.

### Mesin validasi

```csharp
public enum Severity { Error, Warning }

public record ValidationIssue(string QuestionKey, string? PeriodKey, string Message, Severity Severity);

public class ValidationResult
{
    public static readonly ValidationResult Empty = new(Array.Empty<ValidationIssue>());
    private readonly ILookup<string, ValidationIssue> _byKey;

    public ValidationResult(IEnumerable<ValidationIssue> issues)
    {
        Issues = issues.ToList();
        _byKey = Issues.ToLookup(i => i.QuestionKey);
    }

    public IReadOnlyList<ValidationIssue> Issues { get; }
    public bool HasErrors => Issues.Any(i => i.Severity == Severity.Error);
    public IReadOnlyList<string> For(string key) => _byKey[key].Select(i => i.Message).ToList();
}

public interface IValidationEngine
{
    ValidationResult Validate(FormDefinition definition, IReadOnlyDictionary<string, AnswerValue> cells, bool forSubmit);
}
```

Pola implementasi: satu method per aturan, dan engine hanya menelusuri definisi lalu memanggil aturan yang relevan untuk tiap pertanyaan.

```csharp
private static IEnumerable<ValidationIssue> CheckNumber(QuestionDef q, string cellKey, AnswerValue? v, bool forSubmit)
{
    if (v is null)
    {
        if (forSubmit && q.Required)
            yield return new(q.Key, null, "Wajib diisi atau tandai Tidak berlaku.", Severity.Error);
        yield break;
    }

    if (v.State == ValueState.NotApplicable) yield break;

    var rule = q.Validation;
    if (v.Number is { } n && rule is not null)
    {
        if (rule.Min is { } min && n < min)
            yield return new(q.Key, null, $"Nilai minimal {min}.", Severity.Error);
        if (rule.Max is { } max && n > max)
            yield return new(q.Key, null, $"Nilai maksimal {max}.", Severity.Error);
        if (rule.IntegerOnly && n != decimal.Truncate(n))
            yield return new(q.Key, null, "Harus bilangan bulat.", Severity.Error);
    }
}
```

Peringatan (`Severity.Warning`) tidak memblokir pengajuan tetapi ditampilkan ke Reviewer. Contoh yang bisa ditambahkan nanti: lonjakan angka bulanan yang jauh dari bulan sebelumnya.

### Kalkulasi lewat `CalcRef`

Sel abu-abu di Excel yang berisi formula menjadi kalkulasi di kode, bukan ekspresi di JSON. Alasannya: kalkulasi seperti total emisi atau intensitas energi harus bisa diuji, diberi versi, dan diaudit, sedangkan ekspresi bebas di konfigurasi sulit dijamin benar.

```csharp
public interface ICalculation
{
    string Ref { get; }                        // mis. "ghg_total_v2"
    IEnumerable<ComputedCell> Compute(CalcContext ctx);
}

public record ComputedCell(string QuestionKey, string? PeriodKey, decimal Value, string? Unit);

public class CalcRegistry
{
    private readonly IReadOnlyDictionary<string, ICalculation> _byRef;
    public CalcRegistry(IEnumerable<ICalculation> all) => _byRef = all.ToDictionary(c => c.Ref);
    public ICalculation Get(string calcRef) => _byRef[calcRef];
}
```

Aturan pakainya:

1. Nama kalkulasi memuat versi (`ghg_total_v2`). Mengubah rumus berarti kalkulasi baru, dan definisi form baru menunjuk ke nama itu.
2. Hasil dihitung ulang setiap kali sel masukannya berubah, ditampilkan sebagai field read-only, dan disimpan ke `Answer` saat submission diajukan agar view konsolidasi cukup membaca satu tabel. Tandai baris hasil kalkulasi dengan kolom tambahan `IsComputed` pada `Answer`.
3. Definisi yang menunjuk `CalcRef` yang tidak terdaftar ditolak saat publish.
4. Setiap kalkulasi punya unit test dengan angka dari file Excel lama sebagai kasus uji (bab 13).

## 10. Workflow otorisasi dan audit trail

Blok Otorisasi Data di sheet COVER berubah menjadi workflow bertahap di sistem: Contributor mengajukan, Reviewer meninjau, Approver menyetujui, dan hanya data berstatus Approved yang masuk ke konsolidasi. Setiap transisi dicatat siapa, kapan, dan alasannya.

&#91;embedded content: status submission · 5 status\]

Garis putus-putus adalah satu-satunya jalan mundur dari Approved, dan hanya Admin yang boleh menempuhnya.

### Transisi yang diizinkan

| Dari | Ke | Dilakukan oleh | Syarat | Efek |
| --- | --- | --- | --- | --- |
| Draft | Submitted | Contributor site | Validasi pengajuan tanpa Error | Form dikunci untuk Contributor |
| Rejected | Submitted | Contributor site | Validasi pengajuan tanpa Error | Form dikunci lagi |
| Submitted | Reviewed | Reviewer site | Catatan opsional | Tanda tangan Reviewer tercatat |
| Submitted | Rejected | Reviewer site | Komentar wajib | Form terbuka untuk Contributor |
| Reviewed | Approved | Approver site | Periode masih Open | Tanda tangan Approver tercatat, masuk konsolidasi |
| Reviewed | Rejected | Approver site | Komentar wajib | Form terbuka untuk Contributor |
| Approved | Draft | Admin | Alasan wajib, periode masih Open | Dicatat di audit, tanda tangan sebelumnya diarsipkan |

Submission hanya bisa diedit saat statusnya Draft atau Rejected dan pengguna adalah Contributor di site itu. Setelah periode ditutup, semua submission read-only.

### Aturan transisi di domain

Taruh tabel transisi di entitas `Submission` supaya aturannya tidak tersebar di halaman Blazor.

```csharp
public enum RoleName { Contributor, Reviewer, Approver, Admin }

public partial class Submission
{
    private static readonly Dictionary<(SubmissionStatus From, SubmissionStatus To), RoleName> Allowed = new()
    {
        [(SubmissionStatus.Draft,     SubmissionStatus.Submitted)] = RoleName.Contributor,
        [(SubmissionStatus.Rejected,  SubmissionStatus.Submitted)] = RoleName.Contributor,
        [(SubmissionStatus.Submitted, SubmissionStatus.Reviewed)]  = RoleName.Reviewer,
        [(SubmissionStatus.Submitted, SubmissionStatus.Rejected)]  = RoleName.Reviewer,
        [(SubmissionStatus.Reviewed,  SubmissionStatus.Approved)]  = RoleName.Approver,
        [(SubmissionStatus.Reviewed,  SubmissionStatus.Rejected)]  = RoleName.Approver,
        [(SubmissionStatus.Approved,  SubmissionStatus.Draft)]     = RoleName.Admin,
    };

    public void Transition(SubmissionStatus to, RoleName actorRole, string? comment)
    {
        if (!Allowed.TryGetValue((Status, to), out var required) || required != actorRole)
            throw new InvalidOperationException($"Transisi {Status} ke {to} tidak diizinkan untuk peran {actorRole}.");

        if (to is SubmissionStatus.Rejected or SubmissionStatus.Draft && string.IsNullOrWhiteSpace(comment))
            throw new InvalidOperationException("Komentar atau alasan wajib saat menolak atau membuka kembali.");

        Status = to;
    }

    public bool IsEditableBy(RoleName role) =>
        role == RoleName.Contributor && Status is SubmissionStatus.Draft or SubmissionStatus.Rejected;
}
```

### Tanda tangan dan identitas

Nama dan Jabatan tidak diketik ulang di setiap file seperti di COVER. Saat Reviewer atau Approver melakukan transisi, sistem menyalin nama dan jabatan dari profil pengguna ke tabel `Signoff` sebagai potret saat itu, lengkap dengan waktunya. Dengan begitu perubahan jabatan di kemudian hari tidak mengubah catatan persetujuan lama. Contributor (hingga tiga orang per file) tercatat otomatis dari siapa saja yang menyimpan jawaban.

### Audit trail

Catat dua hal: perubahan jawaban dan transisi status. Perubahan jawaban cukup dicatat lewat `SaveChangesInterceptor` yang membaca `ChangeTracker` untuk entitas `Answer`.

```csharp
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _user;
    private readonly IClock _clock;
    public AuditInterceptor(ICurrentUser user, IClock clock) => (_user, _clock) = (user, clock);

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData e, InterceptionResult<int> result, CancellationToken ct = default)
    {
        var db = e.Context!;
        var entries = db.ChangeTracker.Entries<Answer>()
            .Where(x => x.State is EntityState.Modified or EntityState.Added or EntityState.Deleted)
            .ToList();

        foreach (var x in entries)
        {
            db.Add(new AuditEntry
            {
                EntityType = nameof(Answer),
                EntityId = x.Entity.Id.ToString(),
                Action = x.State.ToString(),
                OldValue = x.State == EntityState.Added ? null : Describe(x.OriginalValues),
                NewValue = x.State == EntityState.Deleted ? null : Describe(x.CurrentValues),
                UserId = _user.UserId,
                At = _clock.UtcNow,
            });
        }
        return base.SavingChangesAsync(e, result, ct);
    }

    private static string Describe(PropertyValues v) =>
        System.Text.Json.JsonSerializer.Serialize(new { Number = v[nameof(Answer.NumberValue)], Text = v[nameof(Answer.StringValue)], Unit = v[nameof(Answer.Unit)] });
}
```

### Pantauan progres untuk Admin

Sediakan halaman matriks 6 site × 23 topik dengan status tiap submission. Halaman ini menggantikan langkah "laporkan ke Admin bahwa file sudah siap" di checklist Pengguna, dan menjawab pertanyaan "siapa yang belum selesai" tanpa menunggu laporan manual.

Notifikasi email atau Teams ke Reviewer saat ada pengajuan, dan ke Contributor saat ditolak, bisa ditambahkan lewat satu abstraksi `INotifier` tanpa mengubah workflow.

## 11. Autentikasi dan otorisasi

Akses ditentukan oleh tabel `UserSiteRole` yang berisi pasangan pengguna, site, dan peran, lalu diperiksa di server setiap kali submission dibaca atau diubah. Aturan penting: site tidak pernah dipercaya dari input klien, selalu diambil dari data yang dimuat dan diperiksa terhadap pengguna yang login.

### Peran dan cakupannya

| Peran | Cakupan | Hak |
| --- | --- | --- |
| Contributor | Per site | Mengisi dan mengajukan submission di site itu |
| Reviewer | Per site | Meninjau, menolak, atau meneruskan submission di site itu |
| Approver | Per site | Menyetujui atau menolak submission yang sudah Reviewed |
| Admin | Global | Kelola definisi form, periode, pengguna; menjalankan konsolidasi; membuka kembali submission |
| ReportViewer | Global, hanya baca | Membaca hasil konsolidasi seluruh site (tim pelaporan pusat) |

Seorang pengguna bisa punya beberapa peran di beberapa site, misalnya Contributor di dua site. `UserSiteRole` menyimpan satu baris per kombinasi.

### Pengaturan akun

- Template Blazor dengan `--auth Individual` sudah membawa ASP.NET Core Identity. Matikan pendaftaran mandiri dan buat akun lewat halaman Admin.
- Jika perusahaan sudah punya identity provider (misalnya Microsoft Entra ID), pakai OpenID Connect supaya tidak ada manajemen kata sandi di aplikasi ini dan akun mengikuti siklus karyawan keluar masuk.
- Satu orang satu akun. Hindari akun bersama per site, karena audit trail dan tanda tangan jadi tidak bermakna.

### Otorisasi berbasis sumber daya

Periksa akses terhadap objek `Submission` yang sudah dimuat, bukan terhadap parameter URL.

```csharp
public class SubmissionAccessRequirement(RoleName role) : IAuthorizationRequirement
{
    public RoleName Role { get; } = role;
}

public class SubmissionAccessHandler(IUserSiteRoleStore store)
    : AuthorizationHandler<SubmissionAccessRequirement, Submission>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx, SubmissionAccessRequirement req, Submission submission)
    {
        var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return;

        var allowed = req.Role == RoleName.Admin
            ? ctx.User.IsInRole(nameof(RoleName.Admin))
            : await store.HasRoleAsync(userId, submission.SiteId, req.Role);

        if (allowed) ctx.Succeed(req);
    }
}
```

```csharp
// Program.cs
builder.Services.AddScoped<IAuthorizationHandler, SubmissionAccessHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly",    p => p.RequireRole(nameof(RoleName.Admin)))
    .AddPolicy("ReportViewer", p => p.RequireRole(nameof(RoleName.Admin), nameof(RoleName.ReportViewer)));
```

Di service Application, panggil `IAuthorizationService.AuthorizeAsync(user, submission, new SubmissionAccessRequirement(RoleName.Contributor))` sebelum menyimpan jawaban, dan serupa untuk Reviewer dan Approver sebelum transisi status. Memeriksanya di service, bukan di komponen Blazor, menjaga aturan tetap berlaku jika UI diganti.

### Hal khusus Blazor

- Beri `[Authorize]` pada halaman, dan pakai `AuthorizeView` hanya untuk menyembunyikan tombol. Menyembunyikan tombol bukan kontrol keamanan.
- Pada Interactive Server, identitas pengguna melekat pada circuit. Selalu muat entitas dari database berdasarkan id, lalu otorisasi, jangan memercayai `SiteId` yang dikirim komponen.
- Simpan kunci Data Protection secara persisten (file share atau database) agar sesi tidak rusak setelah aplikasi restart atau dipindah server.
- Simpan connection string dan rahasia di user-secrets saat pengembangan, dan di variabel lingkungan atau secret store di server.

## 12. Konsolidasi lintas site dan export

Konsolidasi menjadi query atas `Answer` berstatus Approved, ditampilkan langsung di halaman dan diekspor ke Excel dalam format kartu yang sama dengan hasil Runner. Tiga tahap Runner (set-cover, konsolidasi, recalc) menyusut menjadi satu, dan sebagian besar kategori log hilang karena penyebabnya tidak lagi mungkin terjadi.

| Di Konsolidasi Runner | Di aplikasi |
| --- | --- |
| Tahap 1, set-cover menulis Site\_ID dan Tahun | Tidak ada, `SiteId` dan `PeriodId` adalah foreign key |
| Tahap 2, kumpulkan data 6 site | Query satu kali atas data Approved |
| Tahap 3, recalc via Excel | Tidak ada, kalkulasi dijalankan server (bab 9); export tidak butuh Excel terpasang |
| \[KARANTINA\], Site\_ID tidak cocok folder | Tidak mungkin terjadi |
| \[PERINGATAN\], formula tidak dikenali resolver | Tidak ada resolver formula |
| \[GAGAL\], satu topik gagal diproses | Error job dicatat Serilog dan tampil di halaman konsolidasi |
| "File terkunci karena terbuka di Excel" | Tidak ada, data ada di database |

### Query konsolidasi

Hasil selalu menyertakan cakupan: site mana yang sudah Approved dan mana yang belum, supaya angka yang tampil tidak diam-diam kurang dari enam site.

```csharp
public record ConsolidatedRow(
    int SiteId, string SiteName, string DisclosureNo, string QuestionKey,
    string? PeriodKey, decimal? Number, string? Text, string? Unit, ValueState State);

public record Coverage(IReadOnlyList<int> ApprovedSiteIds, IReadOnlyList<int> MissingSiteIds);

public record ConsolidationResult(int TopicId, int Year, Coverage Coverage, IReadOnlyList<ConsolidatedRow> Rows);

public async Task<ConsolidationResult> BuildAsync(int topicId, int year, CancellationToken ct)
{
    var approved = _db.Submissions.AsNoTracking()
        .Where(s => s.TopicId == topicId && s.Period.Year == year && s.Status == SubmissionStatus.Approved);

    var rows = await approved
        .SelectMany(s => s.Answers.Select(a => new ConsolidatedRow(
            s.SiteId, s.Site.Name, a.DisclosureNo, a.QuestionKey,
            a.PeriodKey, a.NumberValue, a.StringValue, a.Unit, a.ValueState)))
        .ToListAsync(ct);

    var approvedIds = await approved.Select(s => s.SiteId).Distinct().ToListAsync(ct);
    var allIds = await _db.Sites.Where(s => s.IsActive).Select(s => s.Id).ToListAsync(ct);

    return new ConsolidationResult(topicId, year,
        new Coverage(approvedIds, allIds.Except(approvedIds).ToList()), rows);
}
```

### Dua jenis kartu

Hasil dikelompokkan menjadi dua bentuk, sama seperti kartu di Panduan Pengguna:

- **Kartu naratif**: baris per site dengan kolom Uraian. Site yang menandai Tidak berlaku tampil sebagai "Tidak berlaku", bukan "0" atau "-".
- **Kartu kuantitatif**: baris per site dengan 12 kolom bulan dan total. Total dihitung dari sel yang terisi saja.

Cukup dua DTO untuk menampung keduanya; halaman Blazor dan export Excel membaca DTO yang sama.

```csharp
public record NarrativeCard(string DisclosureNo, string Title, IReadOnlyList<NarrativeRow> Rows);
public record NarrativeRow(string SiteName, string? Text, bool NotApplicable, bool Missing);

public record QuantitativeCard(string DisclosureNo, string Title, string? Unit, IReadOnlyList<QuantRow> Rows);
public record QuantRow(string SiteName, string RowLabel, decimal?[] Months /* 12 */, decimal? Total, bool NotApplicable);
```

### Export ke Excel

Gunakan ClosedXML dan palet dari Panduan Pengguna supaya tampilan hasil tetap dikenali tim pelaporan: teal `#2FA394` untuk header tabel, bronze `#A9895D` untuk spine kartu, teal sangat muda `#EAF4F2` untuk baris selang-seling. Contoh satu kartu kuantitatif:

```csharp
private static int WriteQuantitativeCard(IXLWorksheet ws, int row, QuantitativeCard card)
{
    var teal   = XLColor.FromHtml("#2FA394");
    var bronze = XLColor.FromHtml("#A9895D");
    var band   = XLColor.FromHtml("#EAF4F2");

    ws.Cell(row, 2).Value = $"{card.DisclosureNo}  {card.Title}";
    ws.Cell(row, 2).Style.Font.Bold = true;
    ws.Cell(row, 1).Style.Fill.BackgroundColor = bronze;     // spine
    row++;

    string[] header = { "Site", "Baris", "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des", "Total" };
    for (var c = 0; c < header.Length; c++)
        ws.Cell(row, c + 2).Value = header[c];

    var hr = ws.Range(row, 2, row, header.Length + 1);
    hr.Style.Fill.BackgroundColor = teal;
    hr.Style.Font.FontColor = XLColor.White;
    hr.Style.Font.Bold = true;
    row++;

    var i = 0;
    foreach (var r in card.Rows)
    {
        ws.Cell(row, 2).Value = r.SiteName;
        ws.Cell(row, 3).Value = r.RowLabel;
        for (var m = 0; m < 12; m++)
            if (r.Months[m] is { } v) ws.Cell(row, 4 + m).Value = v;
        if (r.Total is { } t) ws.Cell(row, 16).Value = t;

        if (i++ % 2 == 1) ws.Range(row, 2, row, 16).Style.Fill.BackgroundColor = band;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = bronze;
        row++;
    }
    return row + 1;   // baris kosong antar kartu
}
```

Bandingkan hasil export secara visual dengan satu file konsolidasi lama untuk topik yang sama, dan jadikan perbandingan itu bagian dari tes penerimaan (bab 13).

### Snapshot saat finalisasi

Saat periode difinalkan, simpan snapshot hasil konsolidasi per topik (`ConsolidationSnapshot`: PeriodId, TopicId, CreatedAt, CreatedBy, PayloadJson). Laporan tahunan lalu membaca snapshot, bukan query hidup, sehingga angka yang sudah dilaporkan tidak berubah jika Admin kemudian membuka kembali satu submission.

### Keputusan yang perlu disepakati dengan tim pelaporan

File hasil Runner sekarang memuat sheet analisis manual buatan tim pelaporan yang dipertahankan antar proses. Aplikasi tidak menulis ke workbook mereka. Dua opsi:

1. **Direkomendasikan**: tim pelaporan memindahkan analisisnya ke workbook sendiri yang mengambil data dari export aplikasi (Power Query atau salin nilai), sehingga data dan analisis terpisah dan tidak saling menimpa.
2. Export mengisi ulang template yang sudah memuat sheet analisis. Lebih mirip cara kerja sekarang tetapi rapuh, karena setiap perubahan layout sheet analisis menuntut perubahan kode export.

Pilih satu sebelum fase konsolidasi dikerjakan (bab 15).

## 13. Migrasi data dan import dari Excel

Migrasi paling mudah dilakukan lewat sheet Data-Konsolidasi, karena sheet itu sudah menyimpan jawaban dalam 12 kolom yang sama dengan skema `Answer`. Importer cukup membaca kolom A sampai L dari setiap file sumber, tanpa perlu memahami tata letak sheet disclosure yang berbeda di tiap topik.

### Urutan migrasi

1. **Seed referensi**: 6 site dan 23 topik dari Lampiran L1 Panduan Administrator.
2. **Bangun definisi form versi 1** untuk tiap topik dari file Excel yang ada (lihat di bawah).
3. **Impor data historis** 2025 dan 2026 dari 138 file per tahun, dengan mode simulasi (dry-run) lebih dulu.
4. **Rekonsiliasi** hasil aplikasi terhadap file konsolidasi lama sebelum Excel dipensiunkan.

### Membangun definisi dari file yang ada

Untuk tiap topik, ambil satu file site (misalnya Head Office) sebagai contoh dan baca sheet Data-Konsolidasi: `No_Qz` menjadi nomor disclosure, `Qz` menjadi label pertanyaan, dan kolom terisi (`Number_Value` atau `String_Value`) menunjukkan tipe `number` atau `text`. Tabel bulanan terdeteksi dari `Period` yang berulang per bulan. Hasilnya berupa kerangka JSON yang kemudian ditinjau manual: label resmi, teks bantuan, satuan, dan aturan wajib isi tidak bisa ditebak dari data.

Sel abu-abu yang berisi formula di sheet disclosure diinventarisasi satu per satu dan diputuskan: dipindahkan menjadi `CalcRef` (bab 9), atau dihapus bila hanya rujukan tampilan.

### Aturan pemetaan untuk data lama

| Kondisi di Excel | Hasil di `Answer` |
| --- | --- |
| Site\_ID di COVER!F11 berbeda dari nomor folder | Pakai nomor folder sebagai kebenaran (sama seperti set-cover), catat di laporan impor |
| Sel bernilai angka | `NumberValue`, `ValueState = Filled` |
| Sel bertuliskan "-" | `ValueState = NotApplicable`, tanpa nilai |
| Sel kosong | Tidak membuat baris `Answer` (belum diisi) |
| Teks "0" pada pertanyaan naratif | Tandai untuk ditinjau manual, jangan ditebak |
| `No_Qz` tidak ditemukan di definisi | Masuk laporan impor sebagai tidak terpetakan, tidak diimpor |
| Folder lama (mis. `2. Bakan\`) dan `Backup\` | Diabaikan, sama seperti Runner |
| Folder kategori `2. Soc` atau `2. Sos` | Keduanya dikenali sebagai kategori Soc |

Beberapa kasus di atas diambil dari data nyata yang disebut Panduan Pengguna, misalnya kartu 403-5-A yang menampilkan campuran "0" dan "-" untuk situasi yang mirip. Itu sebabnya angka 0 pada teks naratif tidak diubah otomatis.

### Pembaca Data-Konsolidasi

```csharp
public record LegacyRow(
    int? SiteId, string NoQz, string Qz, string? Period,
    decimal? Number, string? Text, string? Unit);

public IEnumerable<LegacyRow> ReadDataKonsolidasi(string path)
{
    using var wb = new XLWorkbook(path);
    var ws = wb.Worksheet("Data-Konsolidasi");
    var used = ws.RangeUsed();
    if (used is null) yield break;

    foreach (var r in used.RowsUsed().Skip(1))   // lewati baris header
    {
        var number = r.Cell(10).TryGetValue<decimal>(out var n) ? n : (decimal?)null;
        yield return new LegacyRow(
            SiteId: r.Cell(1).TryGetValue<int>(out var sid) ? sid : null,
            NoQz:   r.Cell(7).GetString().Trim(),
            Qz:     r.Cell(8).GetString().Trim(),
            Period: NullIfEmpty(r.Cell(9).GetString()),
            Number: number,
            Text:   NullIfEmpty(r.Cell(11).GetString()),
            Unit:   NullIfEmpty(r.Cell(12).GetString()));
    }
}

private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
```

Sheet Data-Konsolidasi berisi formula, jadi importer harus memakai nilai yang tersimpan di file, bukan menghitung ulang. Ini sebabnya Panduan Administrator meminta file sumber dibuka dan disimpan ulang di Excel bila hasil terlihat lama: nilai tersimpan hanya diperbarui saat Excel menyimpan file. Uji perilaku versi ClosedXML yang dipakai terhadap satu file nyata sebelum mengimpor semuanya, dan lakukan satu putaran buka-simpan di Excel pada file yang nilainya meragukan.

### Mode simulasi dan laporan impor

Importer harus punya mode simulasi seperti `--dry-run` di Runner: membaca semua file, memetakan, dan menghasilkan laporan tanpa menulis ke database. Laporan memuat jumlah baris per topik dan site, serta daftar kasus yang butuh keputusan manusia (tabel pemetaan di atas). Tulis ke database hanya setelah laporan simulasi ditinjau.

### Kriteria penerimaan rekonsiliasi

Sebelum Excel dipensiunkan untuk satu tahun, hasil konsolidasi aplikasi dibandingkan dengan file `*-Konsolidasi.xlsx` hasil Runner untuk tahun yang sama:

- Jumlah baris data per topik dan per site sama.
- Total angka per kartu sama, atau selisihnya terjelaskan satu per satu (misalnya baris yang dulu terkena KARANTINA).
- Teks naratif per site sama setelah normalisasi spasi.
- Daftar selisih ditandatangani tim pelaporan sebelum cutover.

## 14. Testing, deployment, dan operasi

Prioritaskan tes pada tiga hal yang paling mahal bila salah: aturan transisi workflow, validasi dan kalkulasi, serta query konsolidasi. Untuk deployment, mulai sederhana dengan satu instance, karena pengguna internal terbatas pada enam site.

### Strategi testing

| Tingkat | Yang diuji | Alat |
| --- | --- | --- |
| Unit | Transisi status, validasi per sel, kalkulasi `CalcRef`, pemilihan versi form | xUnit |
| Komponen | Setiap field renderer dan `DynamicForm` dengan definisi yang memuat semua tipe input | bUnit |
| Integrasi | Unique index, `ResolveAsync`, query konsolidasi, audit interceptor, terhadap database sungguhan | xUnit dan SQLite (in-memory atau file sementara) |
| End-to-end | Alur lengkap Contributor, Reviewer, Approver, lalu konsolidasi dan export | Playwright untuk .NET |
| Paritas | Hasil aplikasi dibandingkan dengan hasil Runner untuk 2025 (bab 13) | Skrip rekonsiliasi |

Contoh tes aturan workflow:

```csharp
public class SubmissionTransitionTests
{
    [Fact]
    public void Contributor_cannot_approve()
    {
        var s = new Submission { Status = SubmissionStatus.Reviewed };

        Assert.Throws<InvalidOperationException>(() =>
            s.Transition(SubmissionStatus.Approved, RoleName.Contributor, comment: null));
    }

    [Fact]
    public void Rejecting_requires_comment()
    {
        var s = new Submission { Status = SubmissionStatus.Submitted };

        Assert.Throws<InvalidOperationException>(() =>
            s.Transition(SubmissionStatus.Rejected, RoleName.Reviewer, comment: " "));
    }

    [Fact]
    public void Approver_can_approve_reviewed_submission()
    {
        var s = new Submission { Status = SubmissionStatus.Reviewed };
        s.Transition(SubmissionStatus.Approved, RoleName.Approver, comment: null);
        Assert.Equal(SubmissionStatus.Approved, s.Status);
    }
}
```

Catatan untuk tes komponen dengan bUnit: komponen MudBlazor memerlukan `AddMudServices()` pada service collection tes dan `JSInterop` dalam mode longgar. Sesuaikan nama kelas dasar tes dengan versi bUnit yang dipasang, karena namanya berubah antar versi mayor.

Untuk tes integrasi, pakai satu koneksi SQLite in-memory per tes (jaga koneksi tetap terbuka selama tes), terapkan migrations, lalu isi data minimal (satu topik, dua site, satu periode). Tes konsolidasi wajib memuat kasus site yang belum Approved supaya `Coverage.MissingSiteIds` terbukti terisi.

### Deployment

| Aspek | Rekomendasi awal |
| --- | --- |
| Hosting | Windows Server dengan IIS dan ASP.NET Core Hosting Bundle jika infrastruktur sekarang berbasis Windows, atau container Linux bila tim sudah terbiasa |
| WebSockets | Aktifkan di server dan reverse proxy, karena Interactive Server memakai SignalR |
| Jumlah instance | Satu instance dulu. Bila nanti lebih dari satu, perlu sticky session atau layanan SignalR terkelola, plus penyimpanan kunci Data Protection bersama |
| Konfigurasi | `appsettings.Production.json` tanpa rahasia; connection string dan kunci dari variabel lingkungan atau secret store |
| Migrasi database | Terapkan lewat `dotnet ef migrations bundle` pada pipeline, bukan `Migrate()` saat aplikasi start di produksi |
| HTTPS | Wajib, dengan HSTS dan redirect dari HTTP |
| Akun database | Tidak ada akun database di SQLite; batasi hak akses file database hanya untuk akun layanan aplikasi |

Pipeline CI minimal: restore, build, jalankan semua tes, publish artefak, lalu deploy ke staging sebelum produksi. Staging berisi salinan data tahun berjalan yang sudah dianonimkan bila perlu, dan dipakai untuk simulasi impor serta latihan publish versi form baru.

### Backup dan pemulihan

- Backup file SQLite harian lewat perintah VACUUM INTO atau backup API SQLite, bukan salin file biasa saat aplikasi sedang menulis, ditambah satu backup manual sebelum setiap publish versi form dan sebelum konsolidasi final.
- Uji pemulihan ke server lain minimal sekali per kuartal. Backup yang tidak pernah diuji belum bisa dianggap ada.
- Snapshot konsolidasi (bab 12) dan `AuditEntry` ikut dalam backup dan tidak boleh dihapus selama masa retensi audit yang ditetapkan perusahaan.

### Logging dan pemantauan

- Serilog ke file bergulir, dan ke tabel atau Seq bila tersedia, dengan properti `UserId`, `SiteId`, dan `SubmissionId` pada log yang relevan.
- Endpoint `/health` memakai `AddHealthChecks()` dengan pemeriksaan database untuk dipantau oleh monitoring server.
- Pantau jumlah circuit yang terputus dan frekuensi simpan otomatis yang gagal per site. Dua angka ini cepat menunjukkan masalah konektivitas di lapangan sebelum pengguna melapor.

### Runbook singkat untuk Admin

| Kejadian | Tindakan |
| --- | --- |
| Pengguna merasa isiannya hilang | Cek `AuditEntry` untuk submission itu dan log simpan otomatis pada jam yang dilaporkan |
| Submission Approved perlu dikoreksi | Admin membuka kembali ke Draft dengan alasan wajib, selama periode masih Open |
| Standar GRI berubah | Ikuti alur mengubah definisi di bab 7, jangan edit versi yang sudah terbit |
| Karyawan keluar atau pindah site | Nonaktifkan akun atau ubah `UserSiteRole`, jangan hapus akun karena audit merujuk ke sana |
| Periode selesai dilaporkan | Buat snapshot konsolidasi, lalu tutup periode |

## 15. Roadmap implementasi

Bangun dalam enam fase dengan tiga gerbang yang harus lolos sebelum lanjut, dan jangan memigrasi data sebelum satu topik pilot berjalan penuh dari definisi sampai konsolidasi. Bagan di bawah menunjukkan urutan fase dan gerbangnya; durasi sengaja tidak ditetapkan karena bergantung pada ukuran tim.

&#91;embedded content: roadmap · 6 fase, 3 gerbang\]

Baris kedua melanjutkan baris pertama: fase 3 mengalir ke fase 4. Berlian bernomor adalah gerbang yang harus lolos sebelum fase berikutnya dimulai.

### Checklist per fase

**Fase 1, Fondasi**

- [ ] Solution, empat proyek, dan referensi antar proyek (bab 3 dan 4)
- [ ] Skema EF Core, migrations, seed 6 site dan 23 topik (bab 5)
- [ ] Autentikasi, `UserSiteRole`, dan halaman Admin untuk mengelola pengguna (bab 11)

**Fase 2, Form inti**

- [ ] Model definisi, validator definisi, dan penyimpanan `FormVersion` (bab 6 dan 7)
- [ ] Renderer untuk semua tipe input dan simpan otomatis (bab 8)
- [ ] Validasi per sel dan satu kalkulasi contoh (bab 9)
- [ ] **Gerbang 1**: satu topik pilot berjalan dari definisi sampai tersimpan di `Answer`. Disarankan topik 403, karena memuat kartu naratif dan kuantitatif

**Fase 3, Workflow dan audit**

- [ ] Transisi status, tanda tangan, dan aturan edit (bab 10)
- [ ] Audit interceptor untuk perubahan jawaban
- [ ] Matriks progres 6 site × 23 topik untuk Admin

**Fase 4, Konsolidasi dan export**

- [ ] Putuskan opsi sheet analisis bersama tim pelaporan (bab 12)
- [ ] Query konsolidasi, kartu naratif dan kuantitatif, halaman pratinjau
- [ ] Export Excel berformat kartu dan snapshot finalisasi
- [ ] **Gerbang 2**: hasil konsolidasi untuk data uji sama dengan hasil Runner

**Fase 5, Migrasi dan pilot**

- [ ] Definisi versi 1 untuk seluruh 23 topik, ditinjau manual (bab 13)
- [ ] Importer dengan mode simulasi, lalu impor data 2025
- [ ] Rekonsiliasi terhadap file konsolidasi lama, selisih ditandatangani tim pelaporan
- [ ] Pilot di satu atau dua site selama satu siklus pengisian
- [ ] **Gerbang 3**: pilot disetujui tim pelaporan dan Admin

**Fase 6, Rollout**

- [ ] Seluruh site, dengan pelatihan Contributor, Reviewer, dan Approver
- [ ] Panduan Pengguna dan Panduan Administrator diperbarui untuk aplikasi
- [ ] Cutover tahun berjalan; Runner dan file Excel menjadi arsip baca-saja

### Risiko yang paling mungkin menahan jadwal

- **Konektivitas site**: koneksi tidak stabil mengurangi kenyamanan Interactive Server. Uji di site sebelum Fase 3 dan siapkan opsi WebAssembly atau Auto (bab 2).
- **Keputusan tim pelaporan** soal sheet analisis manual (bab 12) terlambat, sehingga Fase 4 tertahan.
- **Mutu definisi hasil ekstraksi**: kerangka JSON dari Data-Konsolidasi tidak memuat label resmi, satuan, atau aturan wajib isi. Sisihkan waktu tinjauan manual untuk 23 topik (bab 13).

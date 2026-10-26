# Desain Struktur Data Pelaporan GRI - End to End

Oct 25, 2026

Dokumen ini membakukan prototipe Excel pelaporan keberlanjutan (folder `kertas - coba deadlock/2025/` dan `Dashboard/Dashboard.xlsx`) menjadi struktur data baku: dari pengisian per site, normalisasi, konsolidasi, sampai dashboard. Struktur ini juga disiapkan untuk otomasi pengisian lewat telemetry. Rincian per file (146 workbook + Dashboard) ada di **Lampiran - Katalog File Prototipe**. Cara membangun aplikasinya ada di **Panduan Membangun Sistem Form GRI Dinamis dengan Blazor**.

Semua angka di dokumen ini dihitung ulang dari isi file dengan skrip read-only, bukan disalin dari catatan.

## 1. Ringkasan

| Aspek | Prototipe Excel | Struktur baku |
| --- | --- | --- |
| Unit isian | 1 workbook per site per topik (23 topik × 6 site) + 77 workbook Head Office | 1 definisi form berversi per topik; jawaban per submission (site, periode, topik) |
| Kunci data | `Kategori` + `No_Qz` (nomor urut baris) + `Period` (angka bulan) | `QuestionKey` + `RowKey` (slug stabil) + `PeriodKey` (`YYYY-MM` atau tahunan) |
| Normalisasi | Sheet tersembunyi `Data-Konsolidasi` dengan ribuan rumus per file | Tabel `Answer` diisi langsung saat menyimpan |
| Rumus | Disalin per file (faktor energi/emisi, total, rasio) | Tabel referensi berversi + kalkulasi `CalcRef` berversi di server |
| Hubungan antar topik | Link antar workbook (sebagian ke URL OneDrive pribadi) | Relasi data antar `QuestionKey` dalam submission/periode yang sama |
| Konsolidasi | Workbook `*-Konsolidasi.xlsx` berisi nilai tempelan 6 site | Query server atas data Approved |
| Dashboard | `SUMIFS/SUMPRODUCT` ke tabel `Data` per posisi baris | KPI didefinisikan per `QuestionKey`/`RowKey`, bukan posisi |
| Otomasi | Tidak ada | Variabel kelayakan telemetry per pertanyaan/baris, wajib diisi sebelum publish |

Inventaris yang dibakukan:

| Kelompok | File | Punya `Data-Konsolidasi` | Baris `Data-Konsolidasi` | Kode `Kategori` |
| --- | --- | --- | --- | --- |
| Site Bakan Mine | 23 | 23 | 4.194 | 212 |
| Site Penjom Mine | 23 | 23 | 4.194 | 212 (identik dengan Bakan) |
| Konsolidasi All Site | 23 | 23 | 25.164 (6 site × 4.194) | 212 |
| General / Head Office | 77 | 3 (201, 204, 413) | 287 | 21 |
| Dashboard | 1 | tabel `Data` | 35.164 | - |

## 2. Alur data prototipe saat ini

```
[1] PENGISIAN (file site, mis. 2. Bakan Mine/1. Env/302.xlsx)
    COVER (Site ID F11, Tahun F12, Topik F13)
    SUMMARY (daftar indikator + hyperlink)
    Sheet per disclosure: blok "INPUT DATA", item A, A.1, B ...
        - tabel bulanan Jan..Dec + Total
        - tabel tahunan Deskripsi | Nilai | Satuan
        - naratif "Uraian" + "Link Referensi & Evidence"
    Sheet bantu: Ref (tblEnergy), Factor Calculation, master (6 site)
          |
          v  rumus per sel
[2] NORMALISASI: sheet tersembunyi Data-Konsolidasi (12 kolom)
    Site_ID | Site_Name | Site_Location | Tahun | Kategori | Topic_Name |
    No_Qz | Qz | Period | Number_Value | String_Value | Unit
          |
          v  disalin sebagai nilai (manual / Konsolidasi Runner)
[3] KONSOLIDASI: 0. Konsolidasi/2. All Site/<topik>-Konsolidasi.xlsx
    Data-Konsolidasi gabungan 6 site + kartu "Konsolidasi 302-1" dst.
    SUMIFS(tblKonsolidasi[Number_Value], Site_Name, Kategori, No_Qz, Period)
          |
          v  ditempel ke tabel Data (Power Query di file ini kosong)
[4] DASHBOARD: Dashboard.xlsx
    Data Status, Energi, Emisi, K3, Air, 201-1, 204-1, 413
    SUMIFS / COUNTIFS / MAXIFS / SUMPRODUCT per Kategori + No_Qz + Period + Site + Tahun
```

Jalur Head Office berbeda: 77 file `0. Konsolidasi/1. General` diisi sekali untuk grup (COVER hanya Tahun dan Topik). Hanya 201, 204, dan 413 yang punya `Data-Konsolidasi`; sisanya berhenti di workbook dan dirujuk lewat link file oleh file lain (misalnya `B.1` → `204`, `C.x`/`G.1`/`205` → workbook GRI 2).

### 2.1 Pola sheet disclosure

Semua file site memakai tata letak yang sama: judul disclosure di `C4`, penanda `INPUT DATA`, lalu blok item dengan kode di kolom C (`A`, `A.1`, `B` ...) dan judul di kolom D. Ada tiga bentuk isian yang berulang:

| Bentuk | Contoh | Usulan `InputType` |
| --- | --- | --- |
| Tabel bulanan, baris = jenis/sumber | 302-1 A.1 (liter BBM per jenis), 303-3 A (ML per sumber air), 306-3 (ton limbah per jenis) | `monthlyTable` |
| Tabel tahunan, baris = komponen | 201-1 A (komponen nilai ekonomi), 403-8 A.1 (jumlah pekerja per kategori), 304-3 A (luas lahan) | `rowsTable` dengan kolom `nilai` |
| Naratif | Hampir semua item "Deskripsi ...", seluruh GRI 101/102/103/GRI 14 | `text` |
| Bukti pendukung | Blok "Link Referensi & Evidence" di banyak sheet | belum ada; usulan tipe `evidence` (lihat bagian 4.3) |

### 2.2 Aturan `Data-Konsolidasi` yang dipakai sekarang

- `Kategori` = kode disclosure dari judul sheet + kode item, misalnya `LEFT('302-1'!$C$4, FIND(" ", ...)-1) & "-A.1"` menghasilkan `302-1-A.1`.
- `No_Qz` = nomor urut baris dalam blok (1, 2, 3 ...). Kadang dihitung dengan rumus posisi seperti `INT((ROW()-11)/12)+1`.
- `Period` = 1–12 untuk tabel bulanan; kosong untuk nilai tahunan dan teks; `Tahunan` (teks) di 303-5-A.2 dan 305-6-A; `0` untuk anggaran 413-1-C.
- `Number_Value` atau `String_Value` diisi tergantung jenis isian; `Unit` diambil dari sel judul tabel.
- `Site_*` dan `Tahun` dibaca dari COVER dan sheet `master`.

## 3. Masalah prototipe yang dibakukan

| No | Temuan | Dampak | Tindakan pembakuan |
| --- | --- | --- | --- |
| 1 | `No_Qz` adalah posisi baris. Dashboard membaca KPI per posisi: K3 Tahunan memakai rentang `No_Qz` 1–5, 6–16, 17–19, 21–22; Emisi menganggap baris 21 sebagai target | Menyisipkan satu baris di template menggeser semua KPI tanpa peringatan | `RowKey` berupa slug stabil (`diesel-b40`, `air-tanah`, `ltifr`) dan target sebagai item tersendiri (bagian 4.1) |
| 2 | Encoding `Period` campur: `1–12`, kosong, `Tahunan`, `0` (anggaran) | Rumus dashboard harus menebak arti tiap nilai | `PeriodKey` = `YYYY-MM` atau `null` (tahunan); anggaran jadi item tersendiri, bukan periode 0 |
| 3 | Satuan tidak seragam: `Ton CO2 Eq`, ` Ton Co2 eq` (spasi depan), `TonCo2 Eq`; `GJ` dan `Gigajoule`; `oz` dan `Oz`; `Liter` dan `liter`; `Metrik Ton (Ton)`, `Ton`, `ton` | Penjumlahan per satuan pecah; dashboard harus `SUBSTITUTE` teks satuan | Kamus satuan baku (bagian 4.4); satuan berasal dari definisi, tidak diketik |
| 4 | Kode `Kategori` tidak seragam: `306-3-A.2-Organik`, `-Anorganik`, `-Recycle`, `-Inert`, `-Residu`, `-Intensitas Limbah` | Kode bergantung pada teks label | Kategori limbah jadi `RowKey`, bukan bagian kode item |
| 5 | Link antar workbook: 303/305/306 → 302, 403 → 405 (Head Office), 201 → 302-Konsolidasi, B.2 → 302/304/305-Konsolidasi, B.1 → 204, F.2 → B.1, F.19 → 408/409, C.2–C.5/G.1/205 → GRI 2 | Rusak saat file dipindah; 2 link ke URL OneDrive pribadi | Relasi data antar `QuestionKey` (bagian 6) |
| 6 | `303.xlsx` Penjom menautkan ke `.../Kertas - sharepoint/2025/1. Head Office/1. Env/302.xlsx`, yaitu 302 milik Head Office, bukan milik Penjom | Intensitas air Penjom memakai data energi/penjualan site lain | Hilang dengan sendirinya: relasi dibatasi submission site yang sama |
| 7 | 74 dari 77 file General tidak punya `Data-Konsolidasi` | Topik Head Office tidak pernah sampai konsolidasi/dashboard | Semua topik memakai definisi form dan tabel `Answer` yang sama; scope "grup" untuk Head Office (bagian 4.2) |
| 8 | Nilai turunan diketik manual: rasio K3 (TRIFR, LTIFR, ISR, fatality rate) di 403-9/403-10, GJ cakupan 3 (302-1-A.6/B.6), pengurangan energi 302-4-A | Tidak konsisten dengan data dasar, rawan salah hitung | Ditandai `derived`, dihitung `CalcRef` (bagian 5) |
| 9 | `305-7-A` (NOx, SOx, POP, VOC, HAP) bersatuan tCO2e | Salah satuan; polutan udara bukan CO2e | Satuan baku ton atau kg per polutan |
| 10 | `Konsolidasi 302-2` baris 18–24: `SUMIFS(tblKonsolidasi[String_Value], tblKonsolidasi[Site_Location], $E$5, ...)` dengan kriteria kolom tertukar | Kartu menampilkan angka salah/nol | Tidak dibawa; konsolidasi berbasis query |
| 11 | Sisa prototipe: sheet tersembunyi `103-1 - 103-51`, `401-01-old`, `405-02`, `Sheet1`, `Sheet2`, `Konsolidasi 304-backup`, `Konsolidasi 301-1-`, `Konsolidasi 14.10-` | Membingungkan; beberapa berisi sel error | Tidak dibawa ke definisi |
| 12 | Sel error tersimpan: 205-2 (48 sel), E.2 `data_source` (8), `306-Konsolidasi` `Data-Konsolidasi` (5), COVER 302 (rumus path `LET`), 2-21 (2) | Nilai tidak valid ikut terbaca | Validasi definisi dan isian di server |
| 13 | Dashboard: Power Query kosong, tabel `Data` berisi tempelan dengan tahun uji 2028–2032; 824 baris tanpa `Site_Name`; `413.xlsx` bertahun 2028 di COVER | Panduan Refresh tidak sesuai isi file; data uji bercampur data nyata | Dashboard membaca langsung dari database; data uji di environment terpisah |
| 14 | Panduan Refresh memeriksa kode `Kategori` `ECO`/`B.1`, padahal data memakai kode `302-1-E` dan seterusnya | Cek duplikat tidak pernah menemukan apa pun | Kode tunggal `<disclosure>-<item>` |
| 15 | Progres pengisian (`Data Status`) hanya dihitung dari `302-1-E` | Site yang belum mengisi topik lain tetap tampak 100% | Progres per submission dan per topik dari status workflow |
| 16 | `201-1` Pendapatan (`E12`) = `[1]Helper!$D$5` di `302-Konsolidasi.xlsx`, padahal sheet `Helper` hanya berisi baris 2–4 (rekap energi) | Pendapatan tercatat 0, sehingga nilai ekonomi ditahan dan `Laporan 201-1 Tahunan` salah | Pendapatan jadi isian `201-1-A/pendapatan` (atau relasi ke item penjualan yang sama dengan penyebut 302-3) |
| 17 | `B.2` "Penggunaan Biodiesel" = `Helper!D3` di `302-Konsolidasi` = Σ `302-1-A.2` + `302-2-A.2`, yaitu GJ BBM cakupan 1 **tak terbarukan** (termasuk Diesel B40) | Definisi angka tidak jelas: campuran B40 atau biodiesel murni (`B.x` terbarukan) | Perlu dikonfirmasi pemilik data; setelah itu ditetapkan sebagai kalkulasi bernama dengan definisi tertulis |

## 4. Model data baku

### 4.1 Kunci jawaban

Setiap angka atau teks yang diisi menjadi satu baris `Answer` dengan kunci:

| Kunci | Arti | Aturan | Contoh |
| --- | --- | --- | --- |
| Submission | site + periode + topik + versi form | sudah ada di GriForms (`Submission`) | Bakan Mine, 2025, topik 302, v1 |
| `QuestionKey` | item disclosure | `<disclosure>-<item>` seperti `Kategori` sekarang, tanpa sufiks nama; untuk sel tabel ditambah `/<rowKey>` dan untuk `rowsTable` `/<rowKey>/<colKey>` | `302-1-A.1/diesel-b40`, `303-3-A/air-tanah`, `204-1-A/lokal/transaksi` |
| `RowKey` | baris tetap dalam tabel | slug huruf kecil dari tabel referensi atau daftar tetap; tidak pernah dipakai ulang untuk arti lain | `pln`, `air-permukaan`, `minyak-pelumas-bekas`, `ltifr` |
| `PeriodKey` | bulan | `YYYY-MM` untuk tabel bulanan, `null` untuk nilai tahunan dan teks | `2025-03` |
| `ValueState` | status isian | Terisi / Tidak berlaku (pengganti tanda "-") / belum diisi (tidak ada baris) | `NotApplicable` |
| Nilai | angka atau teks | `NumberValue` (decimal) atau `StringValue`; satuan dari definisi | `1000`, `"Pelatihan K3 dasar ..."` |

Pemetaan langsung dari kolom `Data-Konsolidasi` ke struktur baku:

| `Data-Konsolidasi` | Struktur baku |
| --- | --- |
| `Site_ID`, `Site_Name`, `Site_Location` | `Submission.SiteId` → tabel `Site` (master) |
| `Tahun` | `ReportingPeriod.Year` |
| `Kategori` | bagian pertama `QuestionKey` |
| `Topic_Name` | `QuestionDef.Label` di definisi form (tidak disalin ke jawaban) |
| `No_Qz` + `Qz` | `RowKey` + `RowDef.Label` |
| `Period` | `PeriodKey` |
| `Number_Value` / `String_Value` | `NumberValue` / `StringValue` |
| `Unit` | `QuestionDef.Unit` (satuan baku) |

**Target** bukan baris tabel realisasi. Baris "Target" di 302-1-A.2/A.4/A.6/B.x/E, 302-2, 305-1/2/3-A (baris 21), dan target di 303-5-A.2 dipindah menjadi item tersendiri dengan sufiks `.target`, misalnya `302-1-E.target` (bulanan, GJ). Dashboard membaca target lewat kunci itu, bukan lewat posisi baris.

**Anggaran** 413-1-C yang sekarang disandikan sebagai `Period = 0` menjadi item `413-1-C.budget` (tahunan, USD per bidang).

### 4.2 Master data

| Master | Isi | Sumber prototipe |
| --- | --- | --- |
| Site | Kode, nama, lokasi, perusahaan, luas lahan, negara | sheet `master` (6 site: Head Office, Bakan, Penjom, Seruyung, North Lanut, Doup) |
| Topik | Kode, nama, kategori, standar | COVER F13 dan struktur folder |
| Kategori topik | `Eco`, `Env`, `Soc`, `Gov`, `Pu` (publik/profil POJK), `GRI 2`, `GRI 3`, `GRI 14` | nama folder `1. Eco` ... `7. GRI 3`, `3. GRI 14` |
| Lingkup isian | `site` (diisi tiap site: 101–306, 203, 403, GRI 14) atau `group` (diisi sekali oleh Head Office: 201–207, 401–415, GRI 2, GRI 3, A.1–G.4) | pemisahan folder site vs `1. General` |
| Indeks POJK 51/2017 | Kode A.1–G.4 dan judulnya, dipetakan ke `QuestionKey` GRI yang menjadi sumber angkanya | file `A.1`, `B.1`–`B.3`, `C.1`–`C.6`, `D.1`, `E.1`–`E.5`, `F.1`–`F.30`, `G.1`–`G.4` |

Topik lingkup `group` diisi sebagai submission milik site Head Office, sehingga memakai alur form, review, dan approval yang sama dengan site lain.

### 4.3 Tipe isian

Tipe `InputType` GriForms yang ada (`text`, `number`, `select`, `boolean`, `monthlyTable`, `rowsTable`) sudah menampung hampir semua isian prototipe. Dua tambahan diusulkan:

1. **`evidence`**: daftar tautan/berkas bukti ("Link Referensi & Evidence") per item. Disimpan terpisah dari angka agar tidak tercampur di konsolidasi.
2. **Baris dari tabel referensi**: untuk 302-1/302-2, baris tabel (jenis BBM, sumber listrik) dipilih dari master `EnergySource`, bukan diketik bebas. Saat ini pengguna mengetik "Diesel B40" dan rumus `XLOOKUP` ke `tblEnergy` menghasilkan 0 bila ejaannya berbeda. Dengan baris dari referensi, `RowKey` stabil dan faktor konversinya pasti ditemukan.

### 4.4 Kamus satuan baku

| Satuan baku | Variasi di prototipe | Dipakai di |
| --- | --- | --- |
| `L` | `Liter`, `liter` | 302-1/302-2 BBM |
| `kWh` | `Kwh` | 302-1/302-2 listrik |
| `GJ` | `Gigajoule`, `GJ` | 302 hasil konversi, 302-4/302-5 |
| `ML` | `ML` | 303 air |
| `m³` | `m³` | 303-5-A.2 (konversi dari ML; disarankan tetap ML) |
| `t` | `Metrik Ton (Ton)`, `Ton`, `ton` | 306 limbah, material movement |
| `kg` | `kg` | 301-1 |
| `tCO2e` | `Ton CO2 Eq`, ` Ton Co2 eq`, `TonCo2 Eq` | 305-1/2/3 |
| `oz` | `oz`, `Oz` | produksi emas |
| `USD` | `USD` | 201, 204, 413, penjualan |
| `orang`, `pemasok`, `desa`, `kecamatan`, `kegiatan`, `pohon`, `ha`, `%` | sama | 403, 204, 413, 304 |
| rasio | `GJ/USD`, `GJ/oz`, `GJ/Ton`, `Ton CO2 Eq/USD`, `m³/Oz` ... | intensitas, selalu `<pembilang>/<penyebut>` dari satuan baku |

## 5. Katalog formulasi baku

Rumus yang sekarang disalin di tiap file dipindah menjadi tabel referensi berversi dan kalkulasi `CalcRef` berversi di server (pola `ICalculation` + `CalcRegistry` yang sudah ada di GriForms; contoh yang sudah jalan: `row_total_v1`). Nama kalkulasi memuat versi; mengubah rumus berarti kalkulasi baru.

### 5.1 Tabel referensi

| Referensi | Kolom | Isi prototipe | Catatan pembakuan |
| --- | --- | --- | --- |
| `EnergySource` | kode, nama, kategori (Renewable/Non-Renewable/Mixed), satuan, faktor ke GJ, EF tCO2e/GJ, sumber, berlaku sejak | sheet `Ref` / `tblEnergy` di 302: 32 sumber, mis. Diesel B40 (L, 0,0367 GJ/L, 0,03946 tCO2e/GJ), PLN (kWh, 0,0036 GJ/kWh, 0,18688 tCO2e/GJ), LPG, batu bara, biodiesel B100 | Satu tabel untuk semua site, berversi per tahun; tidak disalin ke tiap file |
| `GridFactor` | negara/jaringan, tahun, EF listrik | sekarang satu nilai "PLN" untuk semua site | Penjom berada di Malaysia; faktor jaringan harus per negara/site |
| `GwpSet` | gas, GWP, edisi IPCC | `Factor Calculation`: CO2 1, CH4 28, N2O 265 (AR5) | Edisi GWP dicatat per periode pelaporan |
| `ConversionFactor` | dari, ke, faktor | `Factor Calculation`: kWh→GJ 0,0036, liter→kJ 36.700, kJ→GJ 10⁻⁶, liter→MMBTU 0,03242 | Dipakai validasi satuan dan konversi telemetry |

### 5.2 Kalkulasi

| `CalcRef` usulan | Rumus | Input → output | Asal di prototipe |
| --- | --- | --- | --- |
| `row_total_v1` (sudah ada) | Σ bulan per baris | baris bulanan → total tahunan | `SUM(F11:Q11)` di semua tabel bulanan |
| `energy_gj_v1` | kuantitas × faktor GJ sumber energi | 302-1-A.1/A.3/A.5, B.x, 302-2-A.x (L/kWh) → A.2/A.4/A.6 ... (GJ) | `=F11*$E21`, `XLOOKUP(..., tblEnergy[Faktor ke GJ])` |
| `energy_total_v1` | Σ GJ cakupan 1, 2, 3 tak terbarukan + terbarukan | → `302-1-E`, `302-2-A.13` | `E143 = G92+G26+G114+G48+G136+G70` |
| `ghg_scope1_v1` | GJ BBM × EF tCO2e/GJ sumber | 302 GJ → `305-1-A` | 305-1 menyalin GJ dan EF dari 302 |
| `ghg_scope2_location_v1` | kWh × EF jaringan site | 302 kWh → `305-2-A` | `I15 = F15*(FE×GWP ...)/1000` |
| `ghg_scope3_v1` | GJ cakupan 3 × EF | → `305-3-A` | 305-3 |
| `intensity_v1` | pembilang / penyebut, 0 bila penyebut 0 | energi, emisi, air, limbah ÷ penjualan (USD), produksi emas (oz), material movement (t) | `IFERROR(E11/E14, 0)` di 302-3, 305-4, 303-5-A.2, 306 |
| `water_consumption_v1` | total pengambilan − total pembuangan | 303-3 total, 303-4 total → `303-5-A.1` | `='303-3'!E16-'303-4'!E22` |
| `waste_totals_v1` | Σ limbah B3 dan non-B3; dialihkan vs dibuang per metode | 306-3 → total, 306-4, 306-5 | 306-3 dan isian manual 306-4/306-5 |
| `k3_rates_v1` | TRIFR, LTIFR, ISR, fatality rate, high-consequence rate = jumlah kejadian × basis jam (200.000 atau 1.000.000, dicatat di definisi) / jam kerja | 403-9 jumlah kejadian + jam kerja → baris rasio | sekarang diketik manual |
| `local_procurement_share_v1` | transaksi lokal / (lokal + internasional) | `204-1-A` → porsi lokal | `Laporan 204-1 Tahunan` |
| `csr_absorption_v1` | realisasi / anggaran per bidang | `413-1-D` + `413-1-C.budget` → serapan | `413 Anggaran Vs Realisasi` |
| `economic_value_retained_v1` | pendapatan − Σ nilai didistribusikan | `201-1-A` → nilai ditahan | `E20 = E12-E19` di 201-1 (dengan `E12` sekarang bernilai 0, temuan 16) |

Hasil kalkulasi disimpan sebagai `Answer` bertanda asal `calculated` dan dihitung ulang setiap kali inputnya berubah. Dengan begitu konsolidasi dan dashboard membaca angka yang sama dengan yang dilihat reviewer.

## 6. Dependensi lintas topik

Link file diganti relasi antar `QuestionKey`. Relasi dibaca dari submission dengan site dan periode yang sama; untuk topik lingkup `group`, dari submission Head Office pada periode yang sama.

| Dari (input) | Ke (dipakai) | Prototipe sekarang |
| --- | --- | --- |
| 302-1/302-2 (GJ, kWh, EF) | 305-1/305-2/305-3 emisi | `305.xlsx` → `[1]302-1`, `[1]302-2` (link file) |
| 302-3 penyebut (penjualan, produksi, material movement) | 303-5-A.2, 305-4, 306 intensitas | link `[1]302-3` |
| 303-3, 303-4 | 303-5 konsumsi | lintas sheet |
| 405-1 (Head Office) | 403-5 | `403.xlsx` → `405.xlsx` |
| 302-Konsolidasi (energi grup) | B.2 (konsumsi energi, "penggunaan biodiesel"); 201-1 pendapatan | link ke `Helper` file konsolidasi; tautan 201-1 menunjuk sel kosong (temuan 16) |
| 304/302/305-Konsolidasi | B.2 Aspek Lingkungan Hidup | link ke file konsolidasi |
| 204-1 | B.1 jumlah pemasok lokal → F.2 | `B.1` → `204.xlsx`, `F.2` → `B.1.xlsx` |
| 408-1, 409-1 | F.19 | link file |
| GRI 2-1, 2-5, 2-6, 2-7, 2-28 | C.2, C.3, C.4, C.5, G.1, 205 | link ke workbook GRI 2 |

Indeks POJK (B.x, C.x, F.x) sebaiknya tidak menyimpan angka sendiri bila angkanya sudah ada di item GRI: definisi POJK cukup merujuk `QuestionKey` sumber, sehingga satu angka hanya diisi sekali.

## 7. Konsolidasi dan dashboard

### 7.1 Konsolidasi

Konsolidasi menjadi query atas `Answer` dari submission berstatus Approved, dikelompokkan per `QuestionKey`/`RowKey`/`PeriodKey` dan site. Aturan agregasi lintas site ditentukan per item di definisi:

| Jenis item | Agregasi grup | Contoh |
| --- | --- | --- |
| Volume/jumlah (L, kWh, GJ, ML, t, tCO2e, USD, orang, kejadian) | jumlah | energi, emisi, air, limbah, kecelakaan |
| Rasio/intensitas | dihitung ulang dari pembilang dan penyebut grup, **tidak** dirata-rata | intensitas energi grup = Σ GJ / Σ penjualan |
| Rasio K3 | dihitung ulang dari Σ kejadian dan Σ jam kerja | LTIFR grup |
| Teks naratif | ditampilkan per site | kartu naratif per site (sudah ada di GriForms: `NarrativeCardFactory`) |
| Lingkup `group` | tidak diagregasi, satu nilai Head Office | 201, 204, GRI 2 |

### 7.2 KPI dashboard

Setiap KPI didefinisikan sebagai pasangan kunci baku, bukan posisi baris:

| KPI (sheet Dashboard) | Sumber prototipe (`Kategori` → `No_Qz`) | Sumber baku |
| --- | --- | --- |
| Progres pengisian (`Data Status`) | `302-1-E`, bulan terakhir terisi / 12 | status submission per site × topik × periode |
| Energi bulanan dan tahunan, target vs realisasi | `302-1-E` → 1 realisasi, 2 target | `302-1-E` (calculated) dan `302-1-E.target` |
| Emisi bulanan dan tahunan | `305-1-A`, `305-2-A`, `305-3-A` → 21 target, sisanya realisasi | `305-1-A`, `305-2-A`, `305-3-A` total + `.target` |
| K3 per site dan tahunan | `403-9-A` (karyawan), `403-9-B` (mitra) → 1–22 dan rentang 1–5, 6–16, 17–19, 21–22 | `403-9-A/<rowKey>`, `403-9-B/<rowKey>` dengan grup baris dari definisi (jenis cedera, tingkat keparahan, rasio) |
| Konsumsi air | `303-5-A.1` → 1; `303-5-A.2` → 1–7 | `303-5-A.1` (calculated), intensitas via `intensity_v1`, `303-5-A.2.target` |
| Pembuangan air | `303-4-A.2` → semua baris | `303-4-A.2/total-dibuang`, `/total-daur-ulang` |
| Nilai ekonomi 201-1 | `201-1-A` → 1–6 | `201-1-A/<komponen>` + nilai ditahan (calculated) |
| Pengadaan lokal 204-1 | `204-1-A` → 1–4 | `204-1-A/<lokal|internasional>/<pemasok|transaksi>` + porsi lokal (calculated) |
| Realisasi dan anggaran PPM 413 | `413-1-C` (Period > 0 dan Period 0), `413-1-D` → 1–8 | `413-1-D/<bidang>` (bulanan), `413-1-C.budget/<bidang>`, serapan (calculated) |

Dashboard membaca langsung dari database, sehingga langkah "Refresh All", masalah path `Cover.xlsx`, dan tabel tempelan hilang.

## 8. Kesiapan otomasi telemetry

### 8.1 Variabel kelayakan telemetry (dibakukan di definisi form)

Setiap pertanyaan numerik di definisi form wajib membawa klasifikasi berikut. Definisi yang belum mengklasifikasikan semua pertanyaan numeriknya tidak bisa diterbitkan, sehingga keputusan otomasi tidak terlupa saat topik baru dibuat. "Numerik" berarti `number`, `monthlyTable`, atau `rowsTable` yang punya minimal satu kolom `valueType: "number"`; `text`, `select`, `boolean`, dan tabel teks murni boleh tanpa klasifikasi (dianggap manual).

Aturan ini sudah berjalan di GriForms: model `TelemetryDef` di `src/GriForms.Application/Forms/FormDefinition.cs`, validasi di `FormDefinitionValidator` (dipanggil saat Draft disimpan dan saat diterbitkan), dan ringkasan "Kesiapan telemetry" di editor Definisi form. Versi yang sudah terbit tidak divalidasi ulang.

```json
"telemetry": {
  "feasibility": "measured",
  "metric": "diesel_l",
  "unit": "L",
  "aggregation": "sum",
  "note": "flow meter tangki utama"
}
```

| Field | Nilai | Arti |
| --- | --- | --- |
| `feasibility` | `manual` | diisi orang (naratif, angka administrasi/keuangan, data dari sistem lain) |
| | `measured` | bisa diukur langsung oleh sensor/meter |
| | `derived` | dihitung dari item lain (wajib `calcRef` atau `note`) |
| `metric` | snake_case, mis. `energy_kwh` | nama metrik telemetry; wajib untuk `measured` |
| `unit` | satuan baku bagian 4.4 | satuan metrik; wajib untuk `measured` |
| `aggregation` | `sum`, `avg`, `min`, `max`, `last` | cara bacaan sensor diringkas per bulan; wajib untuk `measured` |
| `note` | teks bebas | sumber, alat ukur, atau asal perhitungan |

Field yang sama boleh dipasang per baris (`RowDef.telemetry`) bila sumber tiap baris berbeda, misalnya baris Diesel diukur flow meter sedangkan baris PLN dari meter listrik. Tabel dengan baris bebas (`allowCustomRows`) hanya boleh `measured` bila metriknya ditetapkan di tingkat pertanyaan.

### 8.2 Hasil klasifikasi prototipe

Dari 233 item ber-`Data-Konsolidasi` (212 item template site + 21 item Head Office): **176 manual, 29 derived, 28 measured**. Daftar per item ada di kolom *Telemetry* lampiran.

Kandidat `measured`, berurutan menurut prioritas:

| Prioritas | Item | Baris | Satuan | Metrik usulan | Agregasi | Alat ukur |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 302-1-A.1, B.1, A.5, B.5; 302-2-A.1/A.5/A.7/A.11 | per jenis BBM (Diesel B40, B100, bensin ...) | L | `fuel_<sumber>_l`, mis. `fuel_diesel_b40_l` | sum | flow meter dispenser/tangki, catatan pengisian elektronik |
| 1 | 302-1-A.3, B.3; 302-2-A.3/A.9 | per sumber listrik (PLN, surya ...) | kWh | `electricity_<sumber>_kwh` | sum (selisih register kumulatif) | kWh meter / smart meter |
| 2 | 303-3-A, 303-3-B, 303-3-C-1…5 | per sumber air (permukaan, tanah, PDAM ...) dan kualitas | ML | `water_withdrawal_<sumber>_ml` | sum | flow meter |
| 2 | 303-4-A.1, 303-4-B, 303-4-C | per badan penerima | ML | `water_discharge_<tujuan>_ml` | sum | flow meter outlet |
| 3 | 306-3-A.1 | 12 jenis limbah B3 | t | `waste_b3_<jenis>_t` | sum | timbangan TPS |
| 3 | 306-3-A.2 (organik, anorganik, daur ulang, inert, residu) | per jenis | t | `waste_nonb3_<jenis>_t` | sum | timbangan |

Turunan yang otomatis ikut terisi begitu input terukur tersedia: 302-1-A.2/A.4/A.6 (GJ), 302-1-E, 302-2-A.13, 305-1/2/3-A (tCO2e), 303-5-A.1 (konsumsi air), total 306, dan semua intensitas. Item 403-9 baris "Number of Hours Worked" tetap `manual` dengan catatan bisa diintegrasikan dari sistem absensi; itu integrasi sistem, bukan sensor.

### 8.3 Lapisan data

```
TelemetryDevice (per site, API key)  ── sudah ada
        │ POST /api/telemetry
        v
TelemetryReading (Metric, Value, Unit, RecordedAt)  ── sudah ada, bacaan mentah
        │ agregasi per site × metric × bulan (aggregation dari definisi)
        v
TelemetryMonthlyAggregate (nilai, jumlah bacaan, cakupan jam, celah)  ── baru
        │ pemetaan metric → QuestionKey/RowKey/PeriodKey (dari definisi form terbit)
        v
Saran nilai di form pengisian (ditandai "dari sensor")  ── baru
        │ contributor menerima / mengubah
        v
Answer (resmi) + asal nilai  →  review  →  approve  →  konsolidasi & dashboard
```

Prinsip yang sudah tertulis di kode GriForms tetap berlaku: *telemetry tidak menulis ke `Answer`; nilai sensor hanya saran yang diterima contributor*. Dengan begitu tanggung jawab angka tetap pada contributor dan alur review/approve tidak berubah.

### 8.4 Asal nilai (provenance)

Setiap `Answer` mencatat asalnya:

| Asal | Diisi saat | Data pendukung |
| --- | --- | --- |
| `manual` | contributor mengetik | pengguna, waktu (sudah ada: `UpdatedBy`, `UpdatedAt`) |
| `telemetry` | contributor menerima saran sensor | perangkat, rentang waktu, jumlah bacaan, nilai saran asli |
| `telemetry_edited` | contributor mengubah saran sensor | nilai saran asli + nilai akhir + alasan (wajib) |
| `calculated` | kalkulasi `CalcRef` | nama dan versi kalkulasi, versi tabel referensi |
| `import` | migrasi dari Excel | nama file, sheet, sel asal |

### 8.5 Kelengkapan dan anomali

- **Cakupan data**: persentase jam/hari dengan bacaan dalam bulan itu. Saran ditampilkan dengan peringatan bila cakupan di bawah ambang (misalnya 95%), dan tidak ditawarkan bila di bawah ambang minimum.
- **Celah pengiriman**: perangkat yang `LastSeenAt` lebih lama dari interval wajar ditandai di halaman Telemetry dan di form.
- **Rentang wajar**: `MetricSpec` (sudah ada untuk suhu/kelembapan) diperluas untuk metrik baru, termasuk batas lonjakan antar bacaan untuk meter kumulatif.
- **Perubahan saran**: bila contributor mengganti nilai sensor, alasan wajib diisi dan reviewer melihat perbandingan nilai sensor vs nilai akhir.

## 9. Urutan implementasi usulan

1. **Variabel kelayakan telemetry di definisi form** (bagian 8.1) beserta validasinya, agar semua definisi baru langsung terklasifikasi. *Sudah diimplementasikan.*
2. **Master dan referensi**: site (lengkap dengan negara), topik dengan kategori dan lingkup, `EnergySource`, `GridFactor`, `GwpSet`, kamus satuan.
3. **Definisi form per topik** dari lampiran: mulai dari topik yang dipakai dashboard (302, 305, 303, 403, 201, 204, 413), lalu topik site lain, lalu topik Head Office dan indeks POJK. Setiap tabel diberi `RowKey` stabil, target/anggaran dipisah, dan `telemetry` diisi sesuai lampiran.
4. **Kalkulasi** bagian 5.2 sebagai `CalcRef` berversi, diuji dengan angka prototipe 2025 sebagai pembanding.
5. **Migrasi data 2025** dari `Data-Konsolidasi` per site: `Kategori` + `No_Qz` → `QuestionKey`/`RowKey` lewat tabel pemetaan; laporan baris yang tidak terpetakan ditinjau manual.
6. **Konsolidasi dan dashboard** berbasis kunci baku; bandingkan hasilnya dengan `Dashboard.xlsx` untuk data 2025.
7. **Otomasi telemetry bertahap per metrik**: listrik dan BBM (302) → air (303) → limbah (306), masing-masing dengan pendaftaran metrik di `TelemetryMetrics`, agregat bulanan, saran di form, dan asal nilai.

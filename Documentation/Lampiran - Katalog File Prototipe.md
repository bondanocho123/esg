# Lampiran - Katalog File Prototipe Pelaporan GRI

Dihasilkan dari pembacaan langsung 146 workbook di `kertas - coba deadlock/2025/` dan `Dashboard/Dashboard.xlsx` (skrip read-only, Oktober 2026), lalu ditinjau. Dokumen induk: *Desain Struktur Data Pelaporan GRI - End to End*.

**Cara membaca tabel item**

- **Kategori**: kode item di kolom `Kategori` sheet `Data-Konsolidasi` (`<disclosure>-<item>`).
- **Jenis isian**: usulan `InputType` GriForms (`text`, `number`, `monthlyTable`, `rowsTable`).
- **Baris**: jumlah baris (`No_Qz`) dan contoh labelnya. `No_Qz` adalah urutan baris di template, bukan kunci stabil.
- **Sumber nilai**: jumlah sel yang berasal dari isian langsung, rumus, lintas sheet, lookup tabel faktor, atau file lain.
- **Telemetry**: kelayakan otomasi. `measured` = bisa diukur sensor, `derived` = dihitung dari item lain, `manual` = diisi orang. Variabel yang sama dibakukan di definisi form GriForms.
- **Dashboard**: sheet `Dashboard.xlsx` yang membaca item ini.

Jumlah file: 146 workbook + 1 Dashboard. Site Bakan 23, site Penjom 23, konsolidasi 23, General (Head Office) 77.

## 1. File site (template per topik)

Bakan Mine dan Penjom Mine memakai template identik: sheet, kode `Kategori`, dan jumlah baris `Data-Konsolidasi` sama per topik. Tabel item ditulis sekali di bagian Bakan; baris Penjom hanya mencatat perbedaannya.

### 1.1 Lingkungan (Env)

#### 101 Keanekaragaman Hayati

- File: `2. Bakan Mine/1. Env/101.xlsx` dan `3. Penjom Mine/1. Env/101.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `101-1 - 101-8`, `Data-Konsolidasi`; tersembunyi: `master`
- Baris `Data-Konsolidasi`: 8 per site; 8 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `101-1` | Penetapan kebijakan atau komitmen untuk menghentikan dan membalikkan kehilangan keanekarag | text | 1 | - | - | isian 1 | manual | - |
| `101-2` | Penerapan hierarki mitigasi untuk mengelola dampak keanekaragaman hayati, termasuk tindaka | text | 1 | - | - | isian 1 | manual | - |
| `101-3` | Memastikan kepatuhan terhadap regulasi dan langkah akses serta pembagian manfaat, serta ti | text | 1 | - | - | isian 1 | manual | - |
| `101-4` | Jelaskan bagaimana organisasi menentukan lokasi-lokasi dan produk serta layanan dalam rant | text | 1 | - | - | isian 1 | manual | - |
| `101-5` | Lokasi dan luas lahan dari situs dengan dampak paling signifikan terhadap keanekaragaman h | text | 1 | - | - | isian 1 | manual | - |
| `101-6` | Perubahan ekosistem, eksploitasi sumber daya alam, polusi, dan introduksi spesies invasif  | text | 1 | - | - | isian 1 | manual | - |
| `101-7` | Jenis, luas, dan kondisi ekosistem pada tahun dasar serta periode pelaporan saat ini di se | text | 1 | - | - | isian 1 | manual | - |
| `101-8` | Jasa ekosistem dan penerima manfaat yang terdampak atau berpotensi terdampak oleh kegiatan | text | 1 | - | - | isian 1 | manual | - |

#### 102 Perubahan Iklim

- File: `2. Bakan Mine/1. Env/102.xlsx` dan `3. Penjom Mine/1. Env/102.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `102-1`, `102-2`, `102-3`, `102-4`, `102-5`, `102-6`, `102-7`, `102-8`, `102-9`, `102-10`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 38 per site; 10 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `102-1` | Jelaskan bagaimana rencana transisi perusahaan untuk mitigasi perubahan iklim, termasuk la | text | 1 | - | - | isian 1 | manual | - |
| `102-2` | 102-2 Rencana adaptasi perubahan iklim | rowsTable (teks) | 10: Dampak risiko & peluang iklim — Apa dampak risiko/peluang iklim terhadap manusia dan lingk; Rencana adaptasi -... | - | - | isian 10 | manual | - |
| `102-3` | 102-3 Transisi yang adil (Just transition) | rowsTable (teks) | 10: A. Perekrutan karyawan baru — Berapa jumlah karyawan baru direkrut, dengan rincian gender ; B. Pemutusan hubun... | - | - | isian 10 | manual | - |
| `102-4` | 102-4 Target dan progres pengurangan emisi GRK | rowsTable (teks) | 11: Target jangka pendek, menengah, panjang — Apa target pengurangan emisi Scope 1, 2, 3 (dala; Biogenic CO2 — Apa... | - | - | isian 11 | manual | - |
| `102-5` | Berapa total emisi Scope 1 (CO2, CH4, N2O, HFCs, PFCs, SF6, NF3) dalam ton CO2e? Sertakan  | text | 1 | - | - | isian 1 | manual | - |
| `102-6` | Berapa total emisi Scope 2 (location-based & market-based)? Sertakan CO2, CH4, N2O, emisi  | text | 1 | - | - | isian 1 | manual | - |
| `102-7` | Berapa total emisi Scope 3 (15 kategori)? Sertakan CO2, CH4, N2O, HFCs, PFCs, SF6, NF3, em | text | 1 | - | - | isian 1 | manual | - |
| `102-8` | Apa rasio intensitas emisi (ton CO2e per unit output, pendapatan atau penjualan, atau kary | text | 1 | - | - | isian 1 | manual | - |
| `102-9` | Berapa total penghilangan emisi Scope 1 (ton CO2e)? Sertakan jenis storage pool, kualitas  | text | 1 | - | - | isian 1 | manual | - |
| `102-10` | Berapa total kredit karbon yang dibatalkan (ton CO2e)? Sertakan proyek, ID, tipe, negara,  | text | 1 | - | - | isian 1 | manual | - |

#### 103 Energi

- File: `2. Bakan Mine/1. Env/103.xlsx` dan `3. Penjom Mine/1. Env/103.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `103-1 - 103-5`; tersembunyi: `103-1 - 103-51`, `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 5 per site; 5 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `103-1` | Bagaimana kebijakan energi mendukung efisiensi, pengurangan konsumsi, dan transisi ke ener | text | 1 | - | - | isian 1 | manual | - |
| `103-2` | Berapa total konsumsi bahan bakar, listrik, panas, pendingin, dan uap (dari sumber terbaru | text | 1 | - | - | isian 1 | manual | - |
| `103-3` | Berapa konsumsi energi hulu (pemasok) dan hilir (penggunaan produk, end-of-life)? | text | 1 | - | - | isian 1 | manual | - |
| `103-4` | Apa rasio intensitas energi (total konsumsi energi ÷ metrik organisasi)? Scope apa yang di | text | 1 | - | - | isian 1 | manual | - |
| `103-5` | Berapa pengurangan konsumsi energi dibanding tahun dasar? Apa langkah yang diambil? | text | 1 | - | - | isian 1 | manual | - |

#### 301 Material

- File: `2. Bakan Mine/1. Env/301.xlsx` dan `3. Penjom Mine/1. Env/301.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `301-1`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 6 per site; 1 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `301-1-A` | 301-1 Material yang digunakan berdasarkan berat atau volume | rowsTable (campuran) | 6: Bahan Bakar Minyak; Minyak Pelumas | - | kg | isian 12 | manual | - |

#### 302 Energi

- File: `2. Bakan Mine/1. Env/302.xlsx` dan `3. Penjom Mine/1. Env/302.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `302-1`, `302-2`, `302-3`, `302-4`, `302-5`, `Ref`, `Factor Calculation`; tersembunyi: `Data-Konsolidasi`, `master`
- Baris `Data-Konsolidasi`: 1788 per site; 41 kode Kategori
- Bakan: sel error tersimpan: `COVER` 1
- Penjom: sheet tersembunyi: `Data-Konsolidasi`, `Factor Calculation`, `Ref`, `master`; sel error tersimpan: `COVER` 1

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `302-1-A.1` | Konsumsi energi cakupan 1 - (BBM) dalam organisasi, dari sumber daya tak terbarukan dalam  | monthlyTable | 5: Diesel B40 | bulanan | L | isian 60 | measured | - |
| `302-1-A.2` | Konsumsi energi cakupan 1 - (BBM) dalam organisasi, dari sumber daya tak terbarukan dalam  | monthlyTable | 6: Diesel B40; Target | bulanan | GJ | isian 60, rumus 12 | derived | - |
| `302-1-A.3` | Konsumsi energi cakupan 2 - (Listrik) dalam organisasi, dari sumber daya tak terbarukan da | monthlyTable | 5: PLN | bulanan | kWh | isian 60 | measured | - |
| `302-1-A.4` | Konsumsi energi cakupan 2 - (Listrik) dalam organisasi, dari sumber daya tak terbarukan da | monthlyTable | 6: PLN; Target | bulanan | GJ | isian 60, rumus 12 | derived | - |
| `302-1-A.5` | Konsumsi energi cakupan 3 - (BBM) dalam organisasi, dari sumber daya tak terbarukan dalam  | monthlyTable | 5 | bulanan | L | isian 60 | measured | - |
| `302-1-A.6` | Konsumsi energi cakupan 3 - (BBM) dalam organisasi, dari sumber daya tak terbarukan dalam  | monthlyTable | 6: Target | bulanan | GJ | isian 72 | derived | - |
| `302-1-B.1` | Konsumsi energi cakupan 1 - (BBM) dalam organisasi, dari sumber daya terbarukan dalam (Lit | monthlyTable | 5: Diesel B40 | bulanan | L | isian 60 | measured | - |
| `302-1-B.2` | Konsumsi energi cakupan 1 - (BBM) dalam organisasi, dari sumber daya terbarukan dalam (Gig | monthlyTable | 6: Diesel B40; Target | bulanan | GJ | isian 60, rumus 12 | derived | - |
| `302-1-B.3` | Konsumsi energi cakupan 2 - (Listrik) dalam organisasi, dari sumber daya terbarukan dalam  | monthlyTable | 5 | bulanan | kWh | isian 60 | measured | - |
| `302-1-B.4` | Konsumsi energi cakupan 2 - (Listrik) dalam organisasi, dari sumber daya terbarukan dalam  | monthlyTable | 6: Target | bulanan | GJ | isian 72 | derived | - |
| `302-1-B.5` | Konsumsi energi cakupan 3 - (BBM) dalam organisasi, dari sumber daya terbarukan dalam (Lit | monthlyTable | 5 | bulanan | L | isian 60 | measured | - |
| `302-1-B.6` | Konsumsi energi cakupan 3 - (BBM) dalam organisasi, dari sumber daya terbarukan dalam (Gig | monthlyTable | 6: Target | bulanan | GJ | isian 72 | derived | - |
| `302-1-E` | Konsumsi energi total dalam organisasi cakupan 1, 2, dan 3 (sumber daya terbarukan & tak t | monthlyTable | 2: Calculate; Target | bulanan | GJ | rumus 24 | derived | Dashboard: Data Status, Energi, Energi Tahunan; baris 1 = realisasi (Calculate), baris 2 = target |
| `302-2-A.1` | Konsumsi energi cakupan 1 - (BBM) di luar organisasi, dari sumber daya tak terbarukan dala | monthlyTable | 5 | bulanan | L | isian 60 | measured | - |
| `302-2-A.2` | Konsumsi energi cakupan 1 - (BBM) di luar organisasi, dari sumber daya tak terbarukan dala | monthlyTable | 6: Target | bulanan | GJ | isian 72 | derived | - |
| `302-2-A.3` | Konsumsi energi cakupan 2 - (Listrik) di luar organisasi, dari sumber daya tak terbarukan  | monthlyTable | 5 | bulanan | kWh | isian 60 | measured | - |
| `302-2-A.4` | Konsumsi energi cakupan 2 - (Listrik) di luar organisasi, dari sumber daya tak terbarukan  | monthlyTable | 6: Target | bulanan | GJ | isian 72 | derived | - |
| `302-2-A.5` | Konsumsi energi cakupan 3 - (BBM) luar organisasi, dari sumber daya tak terbarukan dalam ( | monthlyTable | 5 | bulanan | L | isian 60 | measured | - |
| `302-2-A.6` | Konsumsi energi cakupan 3 - (BBM) luar organisasi, dari sumber daya tak terbarukan dalam ( | monthlyTable | 6: Target | bulanan | GJ | isian 72 | derived | - |
| `302-2-A.7` | Konsumsi energi cakupan 1 - (BBM) di luar organisasi, dari sumber daya terbarukan dalam (L | monthlyTable | 5 | bulanan | L | isian 60 | measured | - |
| `302-2-A.8` | Konsumsi energi cakupan 1 - (BBM) di luar organisasi, dari sumber daya terbarukan dalam (G | monthlyTable | 6: Target | bulanan | GJ | isian 72 | derived | - |
| `302-2-A.9` | Konsumsi energi cakupan 2 - (Listrik) di luar organisasi, dari sumber daya terbarukan dala | monthlyTable | 5 | bulanan | kWh | isian 60 | measured | - |
| `302-2-A.10` | Konsumsi energi cakupan 2 - (Listrik) di luar organisasi, dari sumber daya terbarukan dala | monthlyTable | 6: Target | bulanan | GJ | isian 72 | derived | - |
| `302-2-A.11` | Konsumsi energi cakupan 3 - (BBM) luar organisasi, dari sumber daya terbarukan dalam (Lite | monthlyTable | 5 | bulanan | L | isian 60 | measured | - |
| `302-2-A.12` | Konsumsi energi cakupan 3 - (BBM) luar organisasi, dari sumber daya terbarukan dalam (Giga | monthlyTable | 6: Target | bulanan | GJ | isian 72 | derived | - |
| `302-2-A.13` | Konsumsi energi total luar organisasi cakupan 1, 2, dan 3 (sumber daya terbarukan & tak te | monthlyTable | 2: Calculate; Target | bulanan | GJ | rumus 24 | derived | baris 1 = realisasi, baris 2 = target |
| `302-1-F` | Standar, metodologi, asumsi, dan/atau alat penghitungan yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `302-1-G` | Sumber faktor konversi yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `302-2-B` | Standar, metodologi, asumsi, dan/atau alat penghitungan yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `302-2-C` | Sumber faktor konversi yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `302-3-B` | Metrik khusus organisasi (penyebut) yang dipilih untuk menghitung rasio | text | 1 | - | - | isian 1 | manual | - |
| `302-3-C` | Jenis-jenis energi yang termasuk dalam rasio intensitas; apakah bahan bakar, listrik, pema | text | 1 | - | - | isian 1 | manual | - |
| `302-3-D` | Apakah rasio menggunakan konsumsi energi dalam organisasi, di luarnya, atau keduanya | text | 1 | - | - | isian 1 | manual | - |
| `302-4-B` | Jenis-jenis energi yang termasuk dalam pengurangan; baik itu bahan bakar, listrik, pemanas | text | 1 | - | - | isian 1 | manual | - |
| `302-4-C` | Dasar perhitungan pengurangan konsumsi energi, seperti tahun dasar atau kondisi awal, term | text | 1 | - | - | isian 1 | manual | - |
| `302-4-D` | Standar, metodologi, asumsi, dan/atau alat penghitungan yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `302-5-B` | Dasar perhitungan pengurangan konsumsi energi, seperti tahun dasar atau kondisi awal, term | text | 1 | - | - | isian 1 | manual | - |
| `302-5-C` | Standar, metodologi, asumsi, dan/atau alat penghitungan yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `302-4-A` | Jumlah pengurangan konsumsi energi yang dicapai sebagai akibat langsung dari inisiatif efi | monthlyTable | 1 | bulanan | GJ | isian 12 | derived | - |
| `302-5-A` | Pengurangan pada energi yang dibutuhkan produk dan jasa terjual yang dicapai pada site pro | monthlyTable | 1: Pengurangan pada energi yang dibutuhkan produk dan jasa terjual yang dicapai pada site pro | bulanan | GJ | isian 12 | derived | - |
| `302-3-A` | Rasio intensitas energi untuk organisasi | monthlyTable | 10: Jumlah Konsumsi Energi Cakupan 1, 2, & 3, Dalam dan Luar Organisasi; Target Produksi Emas; Target Material Mov... | bulanan | GJ, GJ/USD, GJ/oz, GJ/t, USD, oz, t | isian 72, rumus 36, lintas sheet 12 | derived | - |

#### 303 Air dan Efluen

- File: `2. Bakan Mine/1. Env/303.xlsx` dan `3. Penjom Mine/1. Env/303.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `303-1`, `303-2`, `303-3`, `303-4`, `303-5`, `Data-Konsolidasi`; tersembunyi: `master`
- Baris `Data-Konsolidasi`: 390 per site; 25 kode Kategori
- Bakan: link ke file lain: `302.xlsx`
- Penjom: sheet tersembunyi: `Data-Konsolidasi`, `master`; link ke file lain: `302.xlsx` (URL OneDrive pribadi)

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `303-1-A` | Deskripsi bagaimana organisasi berinteraksi dengan air, termasuk bagaimana dan di mana air | text | 1 | - | - | isian 1 | manual | - |
| `303-1-B` | Deskripsi pendekatan yang digunakan untuk mengidentifikasi dampak-dampak terkait air, term | text | 1 | - | - | isian 1 | manual | - |
| `303-1-C` | Deskripsi bagaimana dampak terkait air ditangani, termasuk bagaimana organisasi bekerja de | text | 1 | - | - | isian 1 | manual | - |
| `303-1-D` | Penjelasan tentang proses menetapkan tujuan dan target yang berkaitan dengan air yang menj | text | 1 | - | - | isian 1 | manual | - |
| `303-2-A` | Deskripsi standar minimum yang ditetapkan untuk kualitas pembuangan efluen, dan cara stand | text | 1 | - | - | isian 1 | manual | - |
| `303-3-A` | Total pengambilan dari semua wilayah dalam megaliter, dan perincian jumlah total ini berda | monthlyTable | 5: Air Hujan; Air Permukaan; Air Tanah; Air Laut... | bulanan | ML | isian 60 | measured | - |
| `303-3-B` | Total pengambilan air dari semua wilayah yang mengalami langka air dalam megaliter, dan pe | monthlyTable | 4: Air Permukaan; Air Tanah; Air Laut; Air yang berasal dari pihak ketiga | bulanan | ML | isian 48 | measured | - |
| `303-3-C-1` | Perincian atas total pengambilan air dari setiap sumber yang tertera di Pengungkapan 303-3 | monthlyTable | 2: Air tawar (≤1.000 mg/L Total Padatan Terlarut); Air lainnya (>1.000 mg/L Total Padatan Terlarut) | bulanan | ML | isian 24 | measured | - |
| `303-3-C-2` | Perincian atas total pengambilan air dari setiap sumber yang tertera di Pengungkapan 303-3 | monthlyTable | 2: Air tawar (≤1.000 mg/L Total Padatan Terlarut); Air lainnya (>1.000 mg/L Total Padatan Terlarut) | bulanan | ML | isian 24 | measured | - |
| `303-3-C-3` | Perincian atas total pengambilan air dari setiap sumber yang tertera di Pengungkapan 303-3 | monthlyTable | 2: Air tawar (≤1.000 mg/L Total Padatan Terlarut); Air lainnya (>1.000 mg/L Total Padatan Terlarut) | bulanan | ML | isian 24 | measured | - |
| `303-3-C-4` | Perincian atas total pengambilan air dari setiap sumber yang tertera di Pengungkapan 303-3 | monthlyTable | 2: Air tawar (≤1.000 mg/L Total Padatan Terlarut); Air lainnya (>1.000 mg/L Total Padatan Terlarut) | bulanan | ML | isian 24 | measured | - |
| `303-3-C-5` | Perincian atas total pengambilan air dari setiap sumber yang tertera di Pengungkapan 303-3 | monthlyTable | 2: Air tawar (≤1.000 mg/L Total Padatan Terlarut); Air lainnya (>1.000 mg/L Total Padatan Terlarut) | bulanan | ML | isian 24 | measured | - |
| `303-3-D` | Semua informasi kontekstual yang diperlukan untuk memahami bagaimana data telah dikumpulka | text | 1 | - | - | isian 1 | manual | - |
| `303-4-A.1` | Total pembuangan air ke seluruh wilayah dalam megaliter, dan perincian jumlah total ini be | monthlyTable | 4: Air permukaan; Air tanah; Air laut; Air pihak ketiga | bulanan | ML | isian 48 | measured | - |
| `303-4-B` | Perincian total pembuangan air ke seluruh wilayah dalam megaliter berdasarkan kategori ber | monthlyTable | 2: Air tawar (≤1.000 mg/L Total Padatan Terlarut); Air lainnya (>1.000 mg/L Total Padatan Terlarut) | bulanan | ML | isian 24 | measured | - |
| `303-4-C` | Total pembuangan air ke seluruh wilayah yang mengalami langka air dalam megaliter, dan per | monthlyTable | 2: Air tawar (≤1.000 mg/L Total Padatan Terlarut); Air lainnya (>1.000 mg/L Total Padatan Terlarut) | bulanan | ML | isian 24 | measured | - |
| `303-4-D` | Zat-zat prioritas yang patut diperhatikan yang pembuangannya diolah, termasuk: I. bagaiman | text | 1 | - | - | isian 1 | manual | - |
| `303-4-E` | Semua informasi kontekstual yang diperlukan untuk memahami bagaimana data telah dikumpulka | text | 1 | - | - | isian 1 | manual | - |
| `303-4-A.2` | Efluen | monthlyTable | 2: Total Air yang Dibuang; Total Air yang di Daur Ulang | bulanan | ML | rumus 12, isian 12 | derived | Dashboard: Konsolidasi Pembuangan Air, Pembuangan Air Tahunan |
| `303-5-A.1` | Total konsumsi air* dari semua wilayah dalam megaliter | monthlyTable | 1: Total Konsumsi Air | bulanan | ML | lintas sheet 12 | derived | Dashboard: Konsumsi Air Tahunan |
| `303-5-A.2` | Intensitas Konsumsi Air | rowsTable | 7: Total Konsumsi Air; Total Penjualan; Total Produksi Emas ; Intensitas Konsumsi Air terhadap Penjualan... | tahunan | %, USD, m³, m³/USD, m³/oz, oz | rumus 3, file lain 2, isian 1, lintas sheet 1 | derived | Dashboard: Konsumsi Air, Konsumsi Air Tahunan |
| `303-5-B.1` | Total konsumsi air dari semua wilayah yang mengalami langka air dalam megaliter | monthlyTable | 1: Total Konsumsi Air | bulanan | ML | lintas sheet 12 | derived | - |
| `303-5-B.2` | Total konsumsi air dari semua wilayah yang mengalami langka air dalam megaliter | text | 1 | - | - | isian 1 | manual | - |
| `303-5-C` | Perubahan dalam penyimpanan air dalam megaliter, jika penyimpanan air telah diidentifikasi | text | 1 | - | - | isian 1 | manual | - |
| `303-5-D` | Semua informasi kontekstual yang diperlukan untuk memahami bagaimana data disusun, seperti | text | 1 | - | - | isian 1 | manual | - |

#### 304 Keanekaragaman Hayati

- File: `2. Bakan Mine/1. Env/304.xlsx` dan `3. Penjom Mine/1. Env/304.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `304-1`, `304-2`, `304-3`, `304-4`; tersembunyi: `Data-Konsolidasi`, `master`
- Baris `Data-Konsolidasi`: 40 per site; 9 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `304-1` | Untuk setiap lokasi operasional yang dimiliki, disewakan, dikelola dalam, atau berdekatan  | text | 1 | - | - | isian 1 | manual | - |
| `304-2-A` | Sifat dampak langsung dan tidak langsung yang signifikan pada keanekaragaman hayati dengan | rowsTable (teks) | 6: Konstruksi atau penggunaan pabrik produksi, tambang, dan infrastruktur transportasi; Polusi (masuknya zat-zat ... | - | - | isian 6 | manual | - |
| `304-2-B` | Dampak positif dan negatif signifikan yang langsung dan tidak langsung dengan merujuk ke h | rowsTable (teks) | 5: Spesies yang terpengaruh; Jangkauan wilayah yang terkena dampak; Masuknya spesies, hama, dan patogen yang inva... | - | - | isian 5 | manual | - |
| `304-3-A` | Ukuran dan lokasi dari seluruh habitat kawasan yang dilindungi atau yang direstorasi, dan  | rowsTable (campuran) | 9: Lahan Terganggu - Tahun 2025; Lahan Terganggu - Akumulasi 2025; Lahan Reklamasi - Tahun 2025; Lahan Reklamasi ... | - | ha, pohon | isian 9 | manual | - |
| `304-3-B` | Apakah dilakukan kemitraan dengan pihak ketiga untuk melindungi atau merestorasi wilayah h | rowsTable (campuran) | 7: Kewajiban Penanaman DAS hingga 2025; Lokasi DAS yang Telah Ditanam per 31 Desember 2025; Penanaman (P0); Pemel... | - | - | isian 7 | manual | - |
| `304-3-C` | Status dari setiap wilayah berdasarkan kondisinya pada saat terakhir periode pelaporan | text | 1 | - | - | isian 1 | manual | - |
| `304-3-D` | Standar, metodologi, dan asumsi yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `304-4-A.1` | Jumlah Species Flora berdasarkan daftar merah IUCN (International Union for Conservation o | rowsTable | 5: Kritis atau sangat terancam punah; Terancam punah; Rentan; Hampir terancam... | - | - | isian 5 | manual | - |
| `304-4-A.2` | Jumlah Species Fauna berdasarkan daftar merah IUCN (International Union for Conservation o | rowsTable | 5: Kritis atau sangat terancam punah; Terancam punah; Rentan; Hampir terancam... | - | - | isian 5 | manual | - |

#### 305 Emisi

- File: `2. Bakan Mine/1. Env/305.xlsx` dan `3. Penjom Mine/1. Env/305.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `305-1`, `305-2`, `305-3`, `305-4`, `305-5`, `305-6`, `305-7`; tersembunyi: `Sheet1`, `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 945 per site; 37 kode Kategori
- Bakan: link ke file lain: `302.xlsx` (URL OneDrive pribadi)
- Penjom: link ke file lain: `302.xlsx`

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `305-1-A` | Emisi GRK (Cakupan 1) langsung kotor dalam metrik ton setara CO2 | monthlyTable | 21: Diesel B40; Diesel B40; Target | bulanan | tCO2e | rumus 240, isian 12 | derived | Dashboard: Emisi, Emisi Tahunan; baris 1-20 = GJ x EF dari 302; baris 21 = target |
| `305-1-B` | Gas yang termasuk dalam penghitungan; apakah berupa CO₂, CH₄, N₂O, HFC, PFC, SF₆, NF₃, ata | text | 1 | - | - | isian 1 | manual | - |
| `305-1-C` | Emisi CO2 biogenik dalam metrik ton setara CO2 | text | 1 | - | - | isian 1 | manual | - |
| `305-1-D` | Tahun dasar untuk penghitungan, jika ada, meliputi: i. alasan untuk memilihnya; ii. emisi  | text | 1 | - | - | isian 1 | manual | - |
| `305-1-E` | Sumber faktor emisi dan nilai potensi pemanasan global (GWP) yang digunakan atau rujukan k | text | 1 | - | - | isian 1 | manual | - |
| `305-1-F` | Pendekatan konsolidasi untuk emisi; apakah porsi ekuitas, kontrol finansial, atau kontrol  | text | 1 | - | - | isian 1 | manual | - |
| `305-1-G` | Standar, metodologi, asumsi, dan/atau alat penghitungan yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `305-2-A` | Emisi energi GRK (Cakupan 2) tidak langsung kotor berdasarkan lokasi dalam metrik ton seta | monthlyTable | 21: PLN; Target | bulanan | tCO2e | rumus 240, isian 12 | derived | Dashboard: Emisi, Emisi Tahunan; baris 1-20 = kWh x EF jaringan; baris 21 = target |
| `305-2-B` | Jika ada, emisi energi GRK (Cakupan 2) tidak langsung kotor berdasarkan pasar dalam metrik | text | 1 | - | - | isian 1 | manual | - |
| `305-2-C` | Jika ada, gas-gas yang termasuk dalam penghitungan; apakah berupa CO₂, CH₄, N₂O, HFC, PFC, | text | 1 | - | - | isian 1 | manual | - |
| `305-2-D` | Tahun dasar untuk penghitungan, jika ada, meliputi: i. alasan untuk memilihnya; ii. emisi  | text | 1 | - | - | isian 1 | manual | - |
| `305-2-E` | Sumber faktor emisi dan nilai potensi pemanasan global (GWP) yang digunakan atau rujukan k | text | 1 | - | - | isian 1 | manual | - |
| `305-2-F` | Pendekatan konsolidasi untuk emisi; apakah porsi ekuitas, kontrol finansial, atau kontrol  | text | 1 | - | - | isian 1 | manual | - |
| `305-2-G` | Standar, metodologi, asumsi, dan/atau alat penghitungan yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `305-3-A` | Total emisi GRK (Cakupan 3) tidak langsung lainnya dalam metrik ton setara CO2 | monthlyTable | 21: Target | bulanan | tCO2e | rumus 240, isian 12 | derived | Dashboard: Emisi, Emisi Tahunan; baris 21 = target |
| `305-3-B` | Jika ada, gas-gas yang termasuk dalam penghitungan; apakah berupa CO₂, CH₄, N₂O, HFC, PFC, | text | 1 | - | - | isian 1 | manual | - |
| `305-3-C` | Emisi CO2 biogenik dalam metrik ton setara CO2 | text | 1 | - | - | isian 1 | manual | - |
| `305-3-D` | Aktivitas dan kategori emisi GRK tidak langsung (Cakupan 3) lainnya yang dimasukkan dalam  | text | 1 | - | - | isian 1 | manual | - |
| `305-3-E` | Tahun dasar untuk penghitungan, jika ada, meliputi: i. alasan untuk memilihnya; ii. emisi  | text | 1 | - | - | isian 1 | manual | - |
| `305-3-F` | Sumber faktor emisi dan nilai potensi pemanasan global (GWP) yang digunakan atau rujukan k | text | 1 | - | - | isian 1 | manual | - |
| `305-3-G` | Standar, metodologi, asumsi, dan/atau alat penghitungan yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `305-4-A` | Rasio intensitas emisi GRK untuk organisasi | monthlyTable | 5: Jumlah Konsumsi Energi Cakupan 1, 2, & 3, Dalam dan Luar Organisasi; Total Penjualan; Produksi Emas Actual; In... | bulanan | USD, oz, tCO2e, tCO2e/USD, tCO2e/oz | file lain 36, lintas sheet 12, rumus 12 | derived | - |
| `305-4-B` | Metrik khusus organisasi (penyebut) yang dipilih untuk menghitung rasio | text | 1 | - | - | isian 1 | manual | - |
| `305-4-C` | Jenis emisi GRK yang dimasukkan dalam rasio intensitas; apakah energi langsung (Cakupan 1) | text | 1 | - | - | isian 1 | manual | - |
| `305-4-D` | Gas yang termasuk dalam penghitungan; apakah berupa CO₂, CH₄, N₂O, HFC, PFC, SF₆, NF₃, ata | text | 1 | - | - | isian 1 | manual | - |
| `305-5-A` | Emisi GRK berkurang sebagai akibat langsung inisiatif pengurangan, dalam metrik ton setara | monthlyTable | 1: Pengurangan emisi GRK dalam metrik ton | bulanan | - | isian 12 | manual | - |
| `305-5-B` | Gas yang termasuk dalam penghitungan; apakah berupa CO₂, CH₄, N₂O, HFC, PFC, SF₆, NF₃, ata | text | 1 | - | - | isian 1 | manual | - |
| `305-5-C` | Tahun dasar atau kondisi awal, termasuk alasan untuk memilihnya | text | 1 | - | - | isian 1 | manual | - |
| `305-5-D` | Ruang Lingkup di mana pengurangan terjadi; apakah (Cakupan 1) langsung, (Cakupan 2) energi | text | 1 | - | - | isian 1 | manual | - |
| `305-5-E` | Standar, metodologi, asumsi, dan/atau alat penghitungan yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `305-6-A` | Produksi, impor, dan ekspor ODS dalam metrik ton setara triklorofluorometana (CFC-11) | rowsTable | 3: ODS yang diproduksi; ODS yang dihancurkan oleh teknologi yang disetujui; ODS secara keseluruhan digunakan seba | tahunan | - | isian 3 | manual | - |
| `305-6-B` | Zat-zat yang dimasukkan dalam penghitungan | text | 1 | - | - | isian 1 | manual | - |
| `305-6-C` | Sumber faktor emisi yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `305-6-D` | Standar, metodologi, asumsi, dan/atau alat penghitungan yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `305-7-A` | Emisi udara yang signifikan, dalam kilogram atau kelipatannya, untuk masing-masing hal ber | monthlyTable | 7: Nox; Sox; Polutan organik yang persisten (POP); Senyawa organik yang mudah menguap (VOC)... | bulanan | tCO2e | isian 84 | derived | satuan tercatat tCO2e padahal NOx/SOx/POP seharusnya ton atau kg |
| `305-7-B` | Sumber faktor emisi yang digunakan | text | 1 | - | - | isian 1 | manual | - |
| `305-7-C` | Standar, metodologi, asumsi, dan/atau alat penghitungan yang digunakan | text | 1 | - | - | isian 1 | manual | - |

#### 306 Limbah

- File: `2. Bakan Mine/1. Env/306.xlsx` dan `3. Penjom Mine/1. Env/306.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `306-1`, `306-2`, `306-3`, `306-4`, `306-5`; tersembunyi: `Data-Konsolidasi`, `master`
- Baris `Data-Konsolidasi`: 703 per site; 16 kode Kategori
- Bakan: link ke file lain: `302.xlsx`
- Penjom: link ke file lain: `302.xlsx`

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `306-2-A` | Tindakan yang diambil, termasuk upaya pendekatan sirkular, untuk mencegah timbulan limbah  | text | 1 | - | - | isian 1 | manual | - |
| `306-2-B` | Jika limbah yang timbul dari aktivitas organisasi dikelola oleh pihak ketiga, maka jelaska | text | 1 | - | - | isian 1 | manual | - |
| `306-2-C` | Proses yang digunakan untuk mengumpulkan dan memantau data terkait limbah | text | 1 | - | - | isian 1 | manual | - |
| `306-3-A.1` | Berat total limbah B3 yang ditimbulkan dalam metrik ton, dan perincian jumlah tersebut men | monthlyTable | 12: Minyak pelumas bekas; Aki / baterai bekas; Limbah fire assay (ceramic, flux, cuppel); Kain majun bekas (used r... | bulanan | t | isian 144 | measured | - |
| `306-3-A.2-Organik` | Berat total limbah non-B3 yang ditimbulkan dalam metrik ton, dan perincian jumlah tersebut | monthlyTable | 8: Sisa Makanan; Daun | bulanan | t | isian 96 | measured | kode bersufiks nama kategori limbah (tidak baku) |
| `306-3-A.2-Anorganik` | Berat total limbah non-B3 yang ditimbulkan dalam metrik ton, dan perincian jumlah tersebut | monthlyTable | 10: Plastik; Bungkus Makanan; Kertas; Karton | bulanan | t | isian 120 | measured | kode bersufiks nama kategori limbah (tidak baku) |
| `306-3-A.2-Recycle` | Berat total limbah non-B3 yang ditimbulkan dalam metrik ton, dan perincian jumlah tersebut | monthlyTable | 7: Botol Air Mineral | bulanan | t | isian 84 | measured | kode bersufiks nama kategori limbah (tidak baku) |
| `306-3-A.2-Inert` | Berat total limbah non-B3 yang ditimbulkan dalam metrik ton, dan perincian jumlah tersebut | monthlyTable | 8: Kayu; Besi | bulanan | t | isian 96 | measured | kode bersufiks nama kategori limbah (tidak baku) |
| `306-3-A.2-Residu` | Berat total limbah non-B3 yang ditimbulkan dalam metrik ton, dan perincian jumlah tersebut | monthlyTable | 7: Residu | bulanan | t | isian 84 | measured | kode bersufiks nama kategori limbah (tidak baku) |
| `306-3-B` | Informasi kontekstual yang diperlukan untuk memahami data dan cara penghimpunan data | text | 1 | - | - | isian 1 | manual | - |
| `306-4` | 306-4 Limbah yang dialihkan dari pembuangan akhir | rowsTable | 4: A. Berat total limbah yang dialihkan dari pembuangan akhir dalam metrik ton, dan perincian; B. Berat total lim | - | t | isian 4 | derived | dapat dihitung dari 306-3 per metode pengelolaan |
| `306-4-E` | Informasi kontekstual yang diperlukan untuk memahami data dan cara penghimpunan data | text | 1 | - | - | isian 1 | manual | - |
| `306-5` | 306-5 Limbah yang dikirimkan ke pembuangan akhir | rowsTable | 4: A. Berat total limbah yang dikirimkan ke pembuangan akhir dalam metrik ton, dan perincian ; B. Berat total lim | - | t | isian 4 | derived | dapat dihitung dari 306-3 per metode pengelolaan |
| `306-5-E` | Informasi kontekstual yang diperlukan untuk memahami data dan cara penghimpunan data | text | 1 | - | - | isian 1 | manual | - |
| `306-1-A` | Untuk dampak terkait limbah yang aktual dan potensial yang signifikan dari organisasi, pen | rowsTable (teks) | 5: Jenis Limbah — Aktivitas* — Lokasi (On Site/Off Site) — Dampak (Potensial/Actual) — Upaya ; Jenis Limbah — Akt... | - | - | isian 5 | manual | - |
| `306-3-A.2-Intensitas Limbah` | Berat total limbah non-B3 yang ditimbulkan dalam metrik ton, dan perincian jumlah tersebut | monthlyTable | 5: Total B3; Total Non-B3; Jumlah Produksi; Intensitas Limbah B3... | bulanan | - | rumus 48, file lain 12 | derived | - |

### 1.2 Sosial (Soc)

#### 203 Dampak Ekonomi Tidak Langsung

- File: `2. Bakan Mine/2. Soc/203.xlsx` dan `3. Penjom Mine/2. Soc/203.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `203-1`, `203-2`; tersembunyi: `Data-Konsolidasi`, `master`
- Baris `Data-Konsolidasi`: 5 per site; 5 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `203-1-A` | Tingkat pengembangan dari investasi infrastruktur yang signifikan dan dukungan layanan | text | 1 | - | - | isian 1 | manual | - |
| `203-1-B` | Dampak kini atau yang diperkirakan akan terjadi pada masyarakat dan perekonomian lokal, te | text | 1 | - | - | isian 1 | manual | - |
| `203-1-C` | Apakah investasi dan layanan ini berifat komersial, natura, atau keterlibatan pro bono | text | 1 | - | - | isian 1 | manual | - |
| `203-2-A` | Contoh dampak ekonomi tidak langsung yang sudah teridentifikasi yang signifikan dari organ | text | 1 | - | - | isian 1 | manual | - |
| `203-2-B` | Signifikansi dari dampak ekonomi tidak langsung dilihat dalam konteks tolok ukur eksternal | text | 1 | - | - | isian 1 | manual | - |

#### 403 Keselamatan dan Kesehatan Kerja

- File: `2. Bakan Mine/2. Soc/403.xlsx` dan `3. Penjom Mine/2. Soc/403.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `403-1`, `403-2`, `403-3`, `403-4`, `403-5`, `403-6`, `403-7`, `403-8`, `403-9`, `403-10`, `Data-Konsolidasi`; tersembunyi: `master`
- Baris `Data-Konsolidasi`: 241 per site; 30 kode Kategori
- Bakan: link ke file lain: `405.xlsx`
- Penjom: sheet tersembunyi: `Data-Konsolidasi`, `master`; link ke file lain: `405.xlsx`

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `403-1-A` | Pernyataan apakah sistem manajemen kesehatan dan keselamatan kerja telah diterapkan, terma | text | 1 | - | - | isian 1 | manual | - |
| `403-1-B` | Deskripsi mengenai ruang lingkup pekerja, aktivitas, dan tempat kerja yang tercakup dalam  | text | 1 | - | - | isian 1 | manual | - |
| `403-2-A` | Deskripsi proses yang digunakan untuk mengidentifikasi bahaya terkait pekerjaan dan menila | text | 1 | - | - | isian 1 | manual | - |
| `403-2-B` | Deskripsi proses bagi pekerja untuk melaporkan bahaya terkait pekerjaan dan situasi berbah | text | 1 | - | - | isian 1 | manual | - |
| `403-2-C` | Deskripsi tentang kebijakan dan proses bagi pekerja untuk meninggalkan situasi kerja yang  | text | 1 | - | - | isian 1 | manual | - |
| `403-2-D` | Deskripsi proses yang digunakan untuk menginvestigasi insiden terkait pekerjaan, termasuk  | text | 1 | - | - | isian 1 | manual | - |
| `403-3-A` | Deskripsi mengenai fungsi layanan kesehatan kerja yang berkontribusi pada proses mengident | text | 1 | - | - | isian 1 | manual | - |
| `403-4-A` | Deskripsi mengenai proses partisipasi dan konsultasi dengan pekerja dalam pengembangan, pe | text | 1 | - | - | isian 1 | manual | - |
| `403-4-B` | Jika ada komite formal gabungan manajemen dan pekerja untuk kesehatan dan keselamatan, des | text | 1 | - | - | isian 1 | manual | - |
| `403-5-A` | Deskripsi tentang pelatihan kesehatan dan keselamatan yang diberikan kepada pekerja, terma | monthlyTable | 12: Training Water Rescue; Training First Aid; Training PHBS; Auditor Internal SMKP... | bulanan | - | isian 133, file lain 11 | manual | - |
| `403-6-A` | Penjelasan tentang cara organisasi memfasilitasi akses pekerja terhadap layanan obat dan p | text | 1 | - | - | isian 1 | manual | - |
| `403-6-B` | Deskripsi layanan dan program peningkatan kualitas kesehatan sukarela yang ditawarkan kepa | text | 1 | - | - | isian 1 | manual | - |
| `403-7-A` | Deskripsi pendekatan organisasi untuk mencegah atau memitigasi dampak kesehatan dan kesela | text | 1 | - | - | isian 1 | manual | - |
| `403-8-A.1` | Jumlah dan persentase semua karyawan dan pekerja yang bukan karyawan tetapi pekerjaannya d | rowsTable | 3: Karyawan; Non Karyawan (Outsourcing/Vendor); Kontraktor | - | orang | isian 3 | manual | - |
| `403-8-A.2` | jumlah dan persentase semua karyawan dan pekerja yang bukan karyawan tetapi pekerjaannya d | rowsTable | 3: Karyawan*; Non Karyawan (Outsourcing/Vendor)*; Kontraktor* | - | orang | isian 3 | manual | - |
| `403-8-A.3` | Jumlah dan persentase semua karyawan dan pekerja yang bukan karyawan tetapi pekerjaannya d | rowsTable | 3: Karyawan*; Non Karyawan (Outsourcing/Vendor)*; Kontraktor* | - | orang | isian 3 | manual | - |
| `403-8-B` | Jika ada pekerja yang tidak disertakan dalam pengungkapan ini, termasuk jenis pekerjaan ya | text | 1 | - | - | isian 1 | manual | - |
| `403-8-C` | Semua informasi kontekstual yang diperlukan untuk memahami bagaimana data telah dikumpulka | text | 1 | - | - | isian 1 | manual | - |
| `403-9-A` | A. Tingkat dan Jumlah Kecelakaan Kerja Karyawan B. Tingkat dan Jumlah Kecelakaan Kerja Mit | rowsTable | 22: Luka Dalam; Patah Tulang; Luka Gores; Luka Memar... | - | - | isian 21, rumus 1 | manual | Dashboard: K3, K3 Tahunan; baris 12-16 dan 21-22 adalah rasio (derived per baris); baris 11 jam kerja bisa dari sistem absensi |
| `403-9-B` | A. Tingkat dan Jumlah Kecelakaan Kerja Karyawan B. Tingkat dan Jumlah Kecelakaan Kerja Mit | rowsTable | 16: Luka Dalam; Patah Tulang; Luka Gores; Luka Memar... | - | - | isian 16 | manual | Dashboard: K3, K3 Tahunan; baris 12-16 adalah rasio (derived per baris) |
| `403-9-C` | Bahaya terkait pekerjaan yang memiliki risiko kecelakaan kerja dengan konsekuensi tinggi,  | rowsTable (teks) | 11: Kontak dengan benda tajam; Tersengat listrik; Jatuh dari ketinggian ke tingkat lebih rendah; Jatuh dari keting... | - | - | isian 11 | manual | - |
| `403-9-D` | Tindakan apa pun yang diambil atau sedang berlangsung untuk menghilangkan bahaya-bahaya la | text | 1 | - | - | isian 1 | manual | - |
| `403-9-E` | Jika perhitungan berdasarkan 200.000 atau 1.000.000 jam kerja | text | 1 | - | - | isian 1 | manual | - |
| `403-9-F` | Jika ada pekerja yang tidak disertakan dalam pengungkapan ini, termasuk jenis pekerja yang | text | 1 | - | - | isian 1 | manual | - |
| `403-9-G` | Semua informasi kontekstual yang diperlukan untuk memahami bagaimana data kecelakaan kerja | text | 1 | - | - | isian 1 | manual | - |
| `403-10-A` | A. Tingkat Kesehatan Kerja Karyawan B. Tingkat Kesehatan Kerja Mitra Kerja | rowsTable | 4: Rasio Kelayakan Kerja; Penyakit Akibat Kerja; Total Recordable Occupational Ill-health; Occupational Ill-healt | - | - | isian 4 | manual | - |
| `403-10-B` | A. Tingkat Kesehatan Kerja Karyawan B. Tingkat Kesehatan Kerja Mitra Kerja | rowsTable | 4: Rasio Kelayakan Kerja; Penyakit Akibat Kerja; Total Recordable Occupational Ill-health; Occupational Ill-healt | - | - | isian 4 | manual | - |
| `403-10-C` | Bahaya terkait pekerjaan yang memiliki risiko penyakit, termasuk: i. bagaimana bahaya ters | rowsTable (teks) | 11: Kontak dengan zat berbahaya; Terpapar radiasi; Potensi ceceran hidrokarbon dari proses refueling dan maintenan | - | - | isian 11 | manual | - |
| `403-10-D` | Jika ada pekerja yang tidak disertakan dalam pengungkapan ini, termasuk jenis pekerja yang | text | 1 | - | - | isian 1 | manual | - |
| `403-10-E` | Semua informasi kontekstual yang diperlukan untuk memahami bagaimana data telah dikumpulka | text | 1 | - | - | isian 1 | manual | - |

### 1.3 Standar Sektor GRI 14

#### Sektor Tambahan 14.10 Komunitas lokal

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.10.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.10.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.10`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 1 per site; 1 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.10.4` | Untuk setiap lokasi tambang, laporkan: i. jumlah dan jenis pengaduan dari masyarakat lokal | text | 1 | - | - | isian 1 | manual | - |

#### Sektor Tambahan 14.11 Hak-Hak Masyarakat Adat

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.11.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.11.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.11`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 2 per site; 2 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.11.3` | Mencantumkan lokasi operasi dan cadangan terbukti di mana Masyarakat Adat ada dan terpenga | text | 1 | - | - | isian 1 | manual | - |
| `14.11.4` | Melaporkan apakah organisasi telah terlibat dalam proses untuk mencari persetujuan atas da | text | 1 | - | - | isian 1 | manual | - |

#### Sektor Tambahan 14.12 Hak atas tanah dan sumber daya

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.12.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.12.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.12`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 2 per site; 2 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.12.2` | Sebutkan lokasi tambang di mana pemukiman kembali secara paksa direncanakan, berlangsung,  | text | 1 | - | - | isian 1 | manual | - |
| `14.12.3` | Sebutkan lokasi operasi tempat terjadinya konflik atau pelanggaran hak atas tanah dan sumb | text | 1 | - | - | isian 1 | manual | - |

#### Sektor Tambahan 14.13 Pertambangan rakyat dan pertambangan skala kecil

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.13.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.13.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.13`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 2 per site; 2 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.13.2` | Sebutkan lokasi tambang tempat PRPSK (Pertambangan Rakyat dan Pertambangan Skala Kecil) be | text | 1 | - | - | isian 1 | manual | - |
| `14.13.3` | Laporkan jumlah total dan sifat insiden yang melibatkan PRPSK (Pertambangan Rakyat dan Per | text | 1 | - | - | isian 1 | manual | - |

#### Sektor Tambahan 14.15 Manajemen krisis

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.15.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.15.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.15`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 2 per site; 2 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.15.3` | Laporkan jumlah insiden kritis dalam periode pelaporan, jelaskan dampak, dan tindakan untu | text | 1 | - | - | isian 1 | manual | - |
| `14.15.4` | Laporkan persentase lokasi tambang yang memiliki program kesiapan dan tanggap darurat, dan | text | 1 | - | - | isian 1 | manual | - |

#### Sektor Tambahan 14.20 Kebebasan berserikat dan perundingan bersama

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.20.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.20.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.20`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 1 per site; 1 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.20.3` | Laporkan jumlah mogok kerja dan larangan bekerja yang melibatkan 1.000 pekerja atau lebih  | text | 1 | - | - | isian 1 | manual | - |

#### Sektor Tambahan 14.22 Antikorupsi

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.22.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.22.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.22`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 2 per site; 2 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.22.5` | Menjelaskan pendekatan terhadap transparansi kontrak, termasuk: i. apakah kontrak dan izin | text | 1 | - | - | isian 1 | manual | - |
| `14.22.6` | Laporkan informasi berikut tentang pihak yang berhak atas manfaat organisasi, termasuk usa | text | 1 | - | - | isian 1 | manual | - |

#### Sektor Tambahan 14.23 Pembayaran kepada pemerintah

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.23.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.23.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.23`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 1 per site; 1 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.23.8` | Untuk mineral yang dibeli dari negara, atau pihak ketiga yang ditunjuk oleh negara untuk m | text | 1 | - | - | isian 1 | manual | - |

#### Sektor Tambahan 14.25 Kawasan terdampak konflik dan berisiko tinggi

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.25.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.25.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.25`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 3 per site; 3 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.25.2` | Sebutkan lokasi operasi di kawasan terdampak konflik atau berisiko tinggi dan cara mengide | text | 1 | - | - | isian 1 | manual | - |
| `14.25.3` | Jelaskan uji tuntas yang diterapkan ketika beroperasi di, atau ketika melakukan pengadaan  | text | 1 | - | - | isian 1 | manual | - |
| `14.25.4` | Untuk operasi di kawasan terdampak konflik atau berisiko tinggi, laporkan potensi dampak n | text | 1 | - | - | isian 1 | manual | - |

#### Sektor Tambahan 14.6 Endapan

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.6.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.6.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.6`; tersembunyi: `Data-Konsolidasi`, `master`
- Baris `Data-Konsolidasi`: 2 per site; 2 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.6.2` | Laporkan metode pembuangan endapan yang digunakan oleh organisasi | text | 1 | - | - | isian 1 | manual | - |
| `14.6.3` | Untuk setiap fasilitas endapan yang belum dipastikan dalam keadaan penutupan yang aman: i. | text | 1 | - | - | isian 1 | manual | - |

#### Sektor Tambahan 14.8 Penutupan dan rehabilitasi

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.8.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.8.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.8`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 6 per site; 6 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.8.4` | Untuk setiap lokasi tambang, laporkan apakah lokasi tambang: i. memiliki rencana penutupan | text | 1 | - | - | isian 1 | manual | - |
| `14.8.5` | Untuk setiap rencana penutupan dan rehabilitasi: i. laporkan apakah rencana telah disetuju | text | 1 | - | - | isian 1 | manual | - |
| `14.8.6` | Untuk setiap lokasi tambang, laporkan dalam satuan hektare: i. total lahan yang terganggu  | text | 1 | - | - | isian 1 | manual | - |
| `14.8.7` | Untuk setiap lokasi tambang, laporkan perkiraan umur tambang (LOM) | text | 1 | - | - | isian 1 | manual | - |
| `14.8.8` | Untuk provisi keuangan yang dibuat oleh organisasi untuk penutupan dan rehabilitasi, terma | text | 1 | - | - | isian 1 | manual | - |
| `14.8.9` | Jelaskan provisi nonkeuangan yang dibuat oleh organisasi untuk mengelola transisi sosial-e | text | 1 | - | - | isian 1 | manual | - |

#### Sektor Tambahan 14.9 Dampak ekonomi

- File: `2. Bakan Mine/3. GRI 14/Sektor Tambahan 14.9.xlsx` dan `3. Penjom Mine/3. GRI 14/Sektor Tambahan 14.9.xlsx` (konsolidasi di bagian 2)
- Sheet: `COVER`, `SUMMARY`, `14.9`; tersembunyi: `master`, `Data-Konsolidasi`
- Baris `Data-Konsolidasi`: 1 per site; 1 kode Kategori
- Penjom: identik dengan Bakan

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `14.9.6` | Laporkan persentase pekerja yang direkrut dari masyarakat lokal di tingkat lokasi tambang, | text | 1 | - | - | isian 1 | manual | - |

## 2. File konsolidasi (`0. Konsolidasi/2. All Site`)

Setiap file berisi `Data-Konsolidasi` gabungan berupa nilai (tanpa rumus) untuk 6 site: Head Office, Bakan, Penjom, Seruyung, North Lanut, dan Doup, sehingga jumlah barisnya 6 x baris per site. Kartu `Konsolidasi ...` membaca tabel itu dengan `SUMIFS(tblKonsolidasi[Number_Value], Site_Name, Kategori, No_Qz, Period)` dan pemilih site di sel `E5`.

| File | Baris Data-Konsolidasi | Kode Kategori | Sheet kartu | Catatan |
|---|---|---|---|---|
| `101-Konsolidasi.xlsx` | 48 | 8 | Konsolidasi 101 | - |
| `102-Konsolidasi.xlsx` | 228 | 10 | Konsolidasi 102-1, Konsolidasi 102-2 s.d 102-4 | - |
| `103-Konsolidasi.xlsx` | 30 | 5 | Konsolidasi 103 | - |
| `301-Konsolidasi.xlsx` | 36 | 1 | Konsolidasi 301-1, Konsolidasi 301-1- | sel error tersimpan: `Konsolidasi 301-1-` 1; sheet sisa/tersembunyi: `Konsolidasi 301-1-` |
| `302-Konsolidasi.xlsx` | 10728 | 41 | Konsolidasi 302-1, Helper, Konsolidasi 302-2, Konsolidasi 302-3, Konsolidasi 302-4, Konsolidasi 302-5, Sheet2 | sheet sisa/tersembunyi: `Sheet2`; `Konsolidasi 302-2` baris 18-24 memakai SUMIFS dengan kriteria kolom tertukar (String_Value/Site_Location/Unit) |
| `303-Konsolidasi.xlsx` | 2340 | 25 | Konsolidasi 303-1, Konsolidasi 303-2, Konsolidasi 303-3-A-B-C, Konsolidasi 303-3-D, Konsolidasi 303-4, Konsolidasi 303-5 | - |
| `304-Konsolidasi.xlsx` | 240 | 9 | Konsolidasi 304-backup, Konsolidasi 304, Helper | sheet sisa/tersembunyi: `Konsolidasi 304-backup`, `Helper` |
| `305-Konsolidasi.xlsx` | 5670 | 37 | Konsolidasi 305-1, Konsolidasi 305-2, Konsolidasi 305-3, Konsolidasi 305-4, Konsolidasi 305-5, Konsolidasi 305-6, Konsolidasi 305-7 | - |
| `306-Konsolidasi.xlsx` | 4218 | 16 | Konsolidasi 306-1, Konsolidasi 306-2, Konsolidasi 306-3, Konsolidasi 306-4, Konsolidasi 306-5 | sel error tersimpan: `Data-Konsolidasi` 5; kode `306-3-A.2-<nama>` bersufiks nama, tidak baku |
| `203-Konsolidasi.xlsx` | 30 | 5 | Konsolidasi 203 | - |
| `403-Konsolidasi.xlsx` | 1446 | 30 | Konsolidasi 403-1 s.d 403-7, Konsolidasi 403-5, Konsolidasi 403-8, Konsolidasi 403-9 s.d 403-10 | - |
| `Sektor Tambahan 14.10-Konsolidasi.xlsx` | 6 | 1 | Konsolidasi 14.10-, Konsolidasi 14.10 | sel error tersimpan: `Konsolidasi 14.10-` 1; sheet sisa/tersembunyi: `Konsolidasi 14.10-` |
| `Sektor Tambahan 14.11-Konsolidasi.xlsx` | 12 | 2 | Konsolidasi 14.11 | - |
| `Sektor Tambahan 14.12-Konsolidasi.xlsx` | 12 | 2 | Konsolidasi 14.12 | - |
| `Sektor Tambahan 14.13-Konsolidasi.xlsx` | 12 | 2 | Konsolidasi 14.13 | - |
| `Sektor Tambahan 14.15-Konsolidasi.xlsx` | 12 | 2 | Konsolidasi 14.15 | - |
| `Sektor Tambahan 14.20-Konsolidasi.xlsx` | 6 | 1 | Konsolidasi 14.20 | - |
| `Sektor Tambahan 14.22-Konsolidasi.xlsx` | 12 | 2 | Konsolidasi 14.22 | - |
| `Sektor Tambahan 14.23-Konsolidasi.xlsx` | 6 | 1 | Konsolidasi 14.23 | - |
| `Sektor Tambahan 14.25-Konsolidasi.xlsx` | 18 | 3 | Konsolidasi 14.25 | - |
| `Sektor Tambahan 14.6-Konsolidasi.xlsx` | 12 | 2 | Konsolidasi 14.6 | - |
| `Sektor Tambahan 14.8-Konsolidasi.xlsx` | 36 | 6 | Konsolidasi 14.8 | - |
| `Sektor Tambahan 14.9-Konsolidasi.xlsx` | 6 | 1 | Konsolidasi 14.9 | - |

## 3. File General / Head Office (`0. Konsolidasi/1. General`)

Topik tingkat perusahaan: GRI 2, GRI 3, GRI 201-415, dan indeks laporan keberlanjutan POJK 51/2017 (A.1-G.4, Sertifikasi & Apresiasi). COVER hanya berisi Tahun (F11) dan Topik (F12), tanpa Site. Hanya 3 dari 77 file yang punya `Data-Konsolidasi` (201, 204, 413); sisanya belum pernah sampai ke konsolidasi maupun dashboard.

### 1. Eco

#### 201 Kinerja Ekonomi

- File: `1. Eco/201.xlsx`
- Sheet: `COVER`, `Konsolidasi 201`, `SUMMARY`, `201-1`, `201-2`, `201-3`, `Data-Konsolidasi`, `201-4`; tersembunyi: `master`
- link ke file lain: `302-Konsolidasi.xlsx`
- Baris `Data-Konsolidasi`: 16; 11 kode Kategori

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `201-1-A` | Nilai ekonomi langsung yang dihasilkan dan didistribusikan (EVG&D) dengan basis akrual, te | rowsTable | 6: Pendapatan; Biaya Operasional; Gaji dan Tunjangan Karyawan; Pembayaran kepada Penyedia Modal... | - | USD | isian 5, file lain 1 | manual | Dashboard: Laporan 201-1 Tahunan |
| `201-1-B` | Jika signifikan, laporkan EVG&D (nilai ekonomi yang dihasilkan dan didistribusikan, termas | text | 1 | - | - | isian 1 | manual | - |
| `201-2-A` | Risiko dan kesempatan yang diakibatkan oleh perubahan iklim yang berpotensi menghasilkan p | text | 1 | - | - | isian 1 | manual | - |
| `201-3-A` | Nilai perkiraan liabilitas, jika liabilitas program tersebut dipenuhi oleh sumber daya umu | text | 1 | - | - | isian 1 | manual | - |
| `201-3-B` | Jika ada dana terpisah untuk membayar liabilitas program pensiun: i. Sejauh mana liabilita | text | 1 | - | - | isian 1 | manual | - |
| `201-3-C` | Jika dana yang dibentuk untuk membayar liabilitas program pensiun tidak sepenuhnya dicakup | text | 1 | - | - | isian 1 | manual | - |
| `201-3-D` | Persentase gaji yang dikontribusikan oleh karyawan atau pemberi kerja | text | 1 | - | - | isian 1 | manual | - |
| `201-3-E` | Tingkat partisipasi dalam program pensiun, seperti partisipasi dalam skema wajib atau suka | text | 1 | - | - | isian 1 | manual | - |
| `201-4-A` | Nilai moneter total dari bantuan finansial yang diterima oleh organisasi dari pemerintah m | text | 1 | - | - | isian 1 | manual | - |
| `201-4-B` | Informasi dalam 201-4-a berdasarkan negara | text | 1 | - | - | isian 1 | manual | - |
| `201-4-C` | Apakah, dan sejauh mana, pemerintah ikut dalam struktur kepemilikan saham | text | 1 | - | - | isian 1 | manual | - |

#### 204 Praktik Pengadaan

- File: `1. Eco/204.xlsx`
- Sheet: `COVER`, `SUMMARY`, `204-1`, `Data-Konsolidasi`; tersembunyi: `master`
- Baris `Data-Konsolidasi`: 22; 3 kode Kategori

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `204-1-A` | Persentase anggaran pengadaan pada lokasi operasi yang signifikan yang dikeluarkan untuk p | rowsTable | 4: Lokal (Nasional) - Jumlah Pemasok Lokal; Lokal (Nasional) - Transaksi (USD); Internasional - Jumlah Pemasok Lo | - | USD, pemasok | isian 20 | manual | Dashboard: Laporan 204-1 Tahunan |
| `204-1-B` | Definisi geografis organisasi tentang 'lokal' | text | 1 | - | - | isian 1 | manual | - |
| `204-1-C` | Definisi yang digunakan untuk 'lokasi operasi yang signifikan' | text | 1 | - | - | isian 1 | manual | - |

#### 207 Pajak

- File: `1. Eco/207.xlsx`
- Sheet: `COVER`, `SUMMARY`, `207-1`, `207-2`, `207-3`, `207-4`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `207-1` | 207-1 Pendekatan terhadap pajak | Uraian -> Pemenuhan kewajiban perpajakan merupakan wujud tanggung jawab Perusahaan dalam mendukung a | evidence link, text | manual |
| `207-2` | 207-2 Tata kelola, pengontrolan, dan manajemen risiko pajak | Uraian -> Pengelolaan administrasi dan kepatuhan perpajakan berada di bawah tanggung jawab Departeme // Uraian // Uraian | text | manual |
| `207-3` | 207-3 Keterlibatan pemangku kepentingan dan pengelolaan kepedulian yang berkaita | Uraian -> Nilai ekonomi yang dihasilkan dari kegiatan usaha sebagian dikontribusikan kepada negara m | evidence link, text | manual |
| `207-4` | 207-4 Laporan per negara | Uraian -> Pada SR 2025 tidak ada statement mengenai indikator ini. Catatan untuk SR 2026: perlu rinc // Uraian -> Pada SR 2025 tidak ada statement mengenai indikator ini. Catatan untuk SR 2026: perlu rinc // Uraian | text | manual |

#### B.1 Aspek Ekonomi

- File: `1. Eco/B.1.xlsx`
- Sheet: `COVER`, `SUMMARY`, `B.1`; tersembunyi: `master`
- link ke file lain: `204.xlsx`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `B.1` | B.1 Aspek Ekonomi | Deskripsi/Nilai/Satuan -> Jumlah Produksi Emas; Pendapatan atau Penjualan; Laba atau Rugi Bersih; Jumlah Pemasok Lok | evidence link, rowsTable | manual |

#### F.2 Perbandingan Target dan Kinerja Produksi, Portofolio, Target Pembiayaan, atau Investasi, Pendapatan dan Laba Rugi

- File: `1. Eco/F.2.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.2`; tersembunyi: `master`
- link ke file lain: `B.1.xlsx`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.2` | F.2 Perbandingan Target dan Kinerja Produksi, Portofolio, Target Pembiayaan, ata | Deskripsi/Target/Realisasi/Satuan -> Produksi; Pendapatan | evidence link, rowsTable | manual |

#### F.3 Perbandingan Target dan Kinerja Portofolio, Target Pembiayaan, atau Investasi pada Instrumen Keuangan atau Proyek yang Sejalan dengan Keuangan Berkelanjutan

- File: `1. Eco/F.3.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.3`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.3` | F.3 Perbandingan Target dan Kinerja Portofolio, Target Pembiayaan, atau Investas | Uraian -> Perusahaan belum memiliki daftar proyek (stated as) yang mendukung keberlanjutan. Catatan | evidence link, text | manual |

### 2. Env

#### B.2 Aspek Lingkungan Hidup

- File: `2. Env/B.2.xlsx`
- Sheet: `COVER`, `Summary`, `B.2`; tersembunyi: `master`
- link ke file lain: `304-Konsolidasi.xlsx`, `302-Konsolidasi.xlsx`, `305-Konsolidasi.xlsx`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `B.2` | B.2 Aspek Lingkungan Hidup | Deskripsi/Nilai (Tahunan)/Satuan -> Luas Lahan Reklamasi; Penggunaan Biodiesel; Konsumsi Energi; Pengurangan Emisi yang Dihasi | evidence link, rowsTable | manual |

#### F.10 Usaha Konservasi Keanekaragaman Hayati

- File: `2. Env/F.10.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.10`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.10` | F.10 Usaha Konservasi Keanekaragaman Hayati | Uraian -> Pengelolaan keanekaragaman hayati dilakukan melalui penerapan kebijakan lingkungan, standa | evidence link, text | manual |

#### F.11 Jumlah dan Intensitas Emisi yang Dihasilkan Berdasarkan Jenisnya

- File: `2. Env/F.11.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.11`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.11` | F.11 Jumlah dan Intensitas Emisi yang Dihasilkan Berdasarkan Jenisnya | Uraian -> Jumlah dan intensitas emisi yang dihasilkan semua cakupan bisa dilihat pada konsolidasi 30 | evidence link, text | manual |

#### F.12 Upaya dan Pencapaian Pengurangan Emisi yang Dilakukan

- File: `2. Env/F.12.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.12`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.12` | F.12 Upaya dan Pencapaian Pengurangan Emisi yang Dilakukan | Uraian -> Di Bakan, inisiatif optimalisasi desain tambang dan rencana jalur angkut berhasil menurunk | text | manual |

#### F.13 Jumlah Limbah dan Efluen yang Dihasilkan Berdasarkan Jenis

- File: `2. Env/F.13.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.13`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.13` | F.13 Jumlah Limbah dan Efluen yang Dihasilkan Berdasarkan Jenis | Uraian -> Jumlah dan jenis limbah bisa dilihat pada konsolidasi 306-3.; Konsolidasi Indikator 306-3 | evidence link, text | manual |

#### F.14 Mekanisme Pengelolaan Limbah dan Efluen

- File: `2. Env/F.14.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.14`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.14` | F.14 Mekanisme Pengelolaan Limbah dan Efluen | Uraian -> Pengelolaan air limbah atau efluen telah dirancang sejak tahap awal sebelum aktivitas pena | text | manual |

#### F.15 Tumpahan yang Terjadi (jika ada)

- File: `2. Env/F.15.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.15`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.15` | F.15 Tumpahan yang Terjadi (jika ada) | Uraian -> Di Tambang Bakan, tidak dihasilkan tailing karena pengolahan batuan menjadi bijih emas dil | evidence link, text | manual |

#### F.16 Jumlah dan Materi Pengaduan Lingkungan Hidup yang Diterima dan Diselesaikan

- File: `2. Env/F.16.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.16`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.16` | F.16 Jumlah dan Materi Pengaduan Lingkungan Hidup yang Diterima dan Diselesaikan | Uraian -> J Resources memastikan seluruh kegiatan pengelolaan lingkungan selaras dengan ketentuan pe | evidence link, text | manual |

#### F.4 Biaya Lingkungan Hidup

- File: `2. Env/F.4.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.4`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.4` | F.4 Biaya Lingkungan Hidup | - | text | manual |

#### F.5 Penggunaan Material yang Ramah Lingkungan

- File: `2. Env/F.5.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.5`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.5` | F.5 Penggunaan Material yang Ramah Lingkungan | Uraian -> Untuk mengurangi ketergantungan terhadap energi fosil, J Resources mendorong pemanfaatan e | evidence link, text | manual |

#### F.6 Jumlah dan Intensitas Energi yang Digunakan

- File: `2. Env/F.6.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.6`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.6` | F.6 Jumlah dan Intensitas Energi yang Digunakan | Uraian | text | manual |

#### F.7 Upaya dan Pencapaian Efisiensi Energi dan Penggunaan Energi Terbarukan

- File: `2. Env/F.7.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.7`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.7` | F.7 Upaya dan Pencapaian Efisiensi Energi dan Penggunaan Energi Terbarukan | Uraian | text | manual |

#### F.8 Penggunaan Air

- File: `2. Env/F.8.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.8`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.8` | F.8 Penggunaan Air | Uraian | text | manual |

#### F.9 Dampak dari Wilayah Operasional yang Dekat atau Berada di Daerah Konservasi atau Memiliki Keanekaragaman Hayati

- File: `2. Env/F.9.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.9`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.9` | F.9 Dampak dari Wilayah Operasional yang Dekat atau Berada di Daerah Konservasi  | Uraian -> Pengelolaan keanekaragaman hayati dilakukan melalui penerapan kebijakan lingkungan, standa | evidence link, text | manual |

### 3. Soc

#### 202 Keberadaan Pasar

- File: `3. Soc/202.xlsx`
- Sheet: `COVER`, `SUMMARY`, `202-1`, `202-2`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `202-1` | 202-1 Rasio standar upah karyawan pemula berdasarkan jenis kelamin terhadap upah | Deskripsi/Uraian -> A. Ketika proporsi yang signifikan dari karyawan mendapatkan kompensasi yang berdasarkan p | evidence link, text | manual |
| `202-2` | 202-2 Proporsi manajemen senior yang berasal dari masyarakat setempat | Deskripsi/Nilai -> Manajemen Senior; Manajemen Senior yang Berasal dari Masyarakat Setempat; Persentase Manaj // Uraian // Uraian // Uraian | rowsTable, text | manual |

#### 401 Kepegawaian

- File: `3. Soc/401.xlsx`
- Sheet: `COVER`, `SUMMARY`, `401-1`, `401-2`, `401-3`; tersembunyi: `401-01-old`, `master`
- sel error tersimpan: `401-01-old` 1
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `401-1` | 401-1 Perekrutan karyawan baru dan pergantian karyawan | Deskripsi/Nilai -> Jumlah karyawan keluar; Tingkat perputaran/Turnover | evidence link, rowsTable | manual |
| `401-2` | 401-2 Tunjangan yang diberikan kepada karyawan purnawaktu yang tidak diberikan k | Uraian -> Di SR diungkap mengenai tunjangan yang didapatkan pada hal. 61 - 69, antara lain: - Remune // Uraian | text | manual |
| `401-3` | 401-3 Cuti melahirkan | Uraian -> Catatan untuk 2026:  1. dibuat tata cara atau sistem untuk meng-capture karyawan perempuan | evidence link, text | manual |

#### 402 Hubungan Tenaga Kerja/Manajemen

- File: `3. Soc/402.xlsx`
- Sheet: `COVER`, `SUMMARY`, `402-1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `402-1` | 402-1 Periode pemberitahuan minimum terkait perubahan operasional | Uraian -> Catatan: belum ada kebijakan khusus mengenai minimum waktu pemberitahuan untuk implementas // Uraian -> Catatan: belum ada kebijakan khusus mengenai minimum periode pemberitahuan untuk implement | evidence link, text | manual |

#### 404 Pelatihan dan Pendidikan

- File: `3. Soc/404.xlsx`
- Sheet: `COVER`, `SUMMARY`, `404-1`, `404-2`, `404-3`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `404-1` | 404-1 Rata-rata jam pelatihan per tahun per karyawan | - | text | manual |
| `404-2` | 404-2 Program untuk meningkatkan keterampilan karyawan dan program bantuan peral | Uraian // Uraian | text | manual |
| `404-3` | 404-3 Persentase karyawan yang menerima tinjauan rutin terhadap kinerja dan peng | - | text | manual |

#### 407 Kebebasan Berserikat dan Perundingan Kolektif

- File: `3. Soc/407.xlsx`
- Sheet: `COVER`, `SUMMARY`, `407-1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `407-1` | 407-1 Operasi dan pemasok di mana hak atas kebebasan berserikat dan perundingan  | Uraian // Uraian | text | manual |

#### 408 Pekerja Anak

- File: `3. Soc/408.xlsx`
- Sheet: `COVER`, `SUMMARY`, `408-1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `408-1` | 408-1 Operasi dan pemasok yang berisiko signifikan terhadap insiden pekerja anak | Uraian -> Perusahaan menerapkan kebijakan yang melarang praktik pekerja anak, dengan menetapkan bata // Uraian // Uraian | text | manual |

#### 409 Kerja Paksa atau Wajib Kerja

- File: `3. Soc/409.xlsx`
- Sheet: `COVER`, `SUMMARY`, `409-1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `409-1` | 409-1 Operasi dan pemasok yang berisiko signifikan terhadap insiden kerja paksa  | Uraian -> Perusahaan telah mengintegrasikan ketentuan perlindungan hak karyawan ke dalam Perjanjian  // Uraian | text | manual |

#### 410 Praktik Keamanan

- File: `3. Soc/410.xlsx`
- Sheet: `COVER`, `SUMMARY`, `410-1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `410-1` | 410-1 Petugas keamanan yang dilatih mengenai kebijakan atau prosedur hak asasi m | Uraian | text | manual |

#### 411 Hak-Hak Masyarakat Adat

- File: `3. Soc/411.xlsx`
- Sheet: `COVER`, `SUMMARY`, `411-1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `411-1` | 411-1 Insiden pelanggaran yang melibatkan hak-hak masyarakat adat | Uraian -> Tidak ada masyarakat adat di sekitar daerah operasi utama dan operasi yang lainnya.  Catat // Uraian -> Tidak ada masyarakat adat di sekitar daerah operasi utama dan operasi yang lainnya.  Catat | evidence link, text | manual |

#### 413 Masyarakat Lokal

- File: `3. Soc/413.xlsx`
- Sheet: `COVER`, `SUMMARY`, `413-1`, `413-2`; tersembunyi: `master`, `Data-Konsolidasi`
- tahun di COVER = 2028 (data uji)
- Baris `Data-Konsolidasi`: 249; 7 kode Kategori

| Kategori | Judul item | Jenis isian | Baris | Periode | Satuan | Sumber nilai | Telemetry | Dashboard / catatan |
|---|---|---|---|---|---|---|---|---|
| `413-1-A` | Persentase operasi dengan keterlibatan masyarakat setempat yang sudah diimplementasikan, p | text | 1: Uraian* | - | - | isian 1 | manual | - |
| `413-1-B` | Lokasi Pelibatan dan Pengembangan Masyarakat | rowsTable | 3: Jumlah Orang; Jumlah Desa; Jumlah Kecamatan | - | desa, kecamatan, orang | isian 18 | manual | - |
| `413-1-C` | Realisasi Anggaran PPM (USD) | monthlyTable | 8: Pendidikan; Kesehatan; Tingkat Pendapatan Riil atau Pekerjaan; Kemandirian Ekonomi... | bulanan + 0 (anggaran) | USD, kegiatan | isian 104 | manual | Dashboard: 413 Anggaran Vs Realisasi, Laporan 413 Realisasi |
| `413-1-D` | Realisasi Anggaran PPM (USD) | monthlyTable | 8: Pendidikan; Kesehatan; Tingkat Pendapatan Riil atau Pekerjaan; Kemandirian Ekonomi... | bulanan | USD | isian 96 | manual | Dashboard: 413 Anggaran Vs Realisasi, Laporan 413 Realisasi |
| `413-1-E` | Program Unggulan Pemberdayaan Masyarakat | rowsTable (teks) | 6: Ekonomi - Aktivitas; Ekonomi - Latar Belakang/Justifikasi Kegiatan; Ekonomi - Input*; Ekonomi - Output**... | - | - | isian 18 | manual | - |
| `413-2-A` | Operasi yang secara aktual dan yang berpotensi memiliki dampak negatif signifikan terhadap | text | 1: Uraian | - | - | isian 1 | manual | - |
| `413-2-B` |  | rowsTable (teks) | 11: Lokasi — Potensial — Aktual — Inovasi Sosial; Lokasi — Potensial — Aktual — Inovasi Sosial; Lokasi — Potensial... | - | - | isian 11 | manual | - |

#### B.3 Ikhtisar Kinerja Keberlanjutan (Aspek Sosial)

- File: `3. Soc/B.3.xlsx`
- Sheet: `COVER`, `SUMMARY`, `B.3`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `B.3` | B.3 Aspek Sosial | Uraian | text | manual |

#### F.17 Komitmen untuk Memberikan Layanan atas Produk dan/atau Jasa yang Setara kepada Konsumen

- File: `3. Soc/F.17.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.17`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.17` | F.17 Komitmen untuk Memberikan Layanan atas Produk dan/atau Jasa yang Setara kep | Uraian -> Catatan untuk 2026: perlu diungkap dengan informasi yang diperoleh dari bagian produksi da | evidence link, text | manual |

#### F.18 Kesetaraan Kesempatan Bekerja

- File: `3. Soc/F.18.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.18`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.18` | F.18 Kesetaraan Kesempatan Bekerja | Uraian -> Penerapan prinsip kesetaraan menjadi dasar dalam pengelolaan sumber daya manusia di J Reso | evidence link, text | manual |

#### F.19 Tenaga Kerja Anak dan Tenaga Kerja Paksa

- File: `3. Soc/F.19.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.19`; tersembunyi: `master`
- link ke file lain: `408.xlsx`, `409.xlsx`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.19` | F.19 Tenaga Kerja Anak dan Tenaga Kerja Paksa | Uraian | evidence link, text | manual |

#### F.20 Upah Minimum Regional

- File: `3. Soc/F.20.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.20`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.20` | F.20 Upah Minimum Regional | Uraian | text | manual |

#### F.21 Lingkungan Bekerja yang Layak dan Aman

- File: `3. Soc/F.21.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.21`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.21` | F.21 Lingkungan Bekerja yang Layak dan Aman | Uraian -> Risiko keselamatan dan kesehatan kerja (K3) pada sektor pertambangan memerlukan pengendali | evidence link, text | manual |

#### F.22 Pelatihan dan Pengembangan Kemampuan Pegawai

- File: `3. Soc/F.22.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.22`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.22` | F.22  Pelatihan dan Pengembangan Kemampuan Pegawai | Uraian | text | manual |

#### F.23 Dampak Operasi terhadap Masyarakat Sekitar

- File: `3. Soc/F.23.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.23`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.23` | F.23 Dampak Operasi terhadap Masyarakat Sekitar | Uraian -> Bersama pemerintah daerah, J Resources menginisiasi berbagai program untuk memperkuat kema | evidence link, text | manual |

#### F.24 Pengaduan Masyarakat

- File: `3. Soc/F.24.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.24`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.24` | F.24 Pengaduan masyarakat | Uraian -> Perusahaan belum memiliki saluran pengaduan anonim khusus bagi masyarakat di sekitar area | evidence link, text | manual |

#### F.25 Kegiatan Tanggung Jawab Sosial Lingkungan (TJSL)

- File: `3. Soc/F.25.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.25`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.25` | F.25 Kegiatan Tanggung Jawab Sosial Lingkungan (TJSL) | Uraian -> Program TJSL melakukan berbagai program unggulan di bidang pendidikan, kesehatan, sosial b | evidence link, text | manual |

#### F.26 Inovasi dan Pengembangan Produk/Jasa Keuangan Berkelanjutan

- File: `3. Soc/F.26.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.26`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.26` | F.26 Inovasi dan Pengembangan Produk/Jasa Berkelanjutan | Uraian -> Perusahaan melakukan pengendalian kualitas dan keamanan produk secara menyeluruh untuk mem | evidence link, text | manual |

#### F.27 Produk/Jasa yang Sudah Dievaluasi Keamanannya bagi Pelanggan

- File: `3. Soc/F.27.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.27`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.27` | F.27 Produk/Jasa yang Sudah Dievaluasi Keamanannya bagi Pelanggan | Uraian -> Perusahaan melakukan pengendalian kualitas dan keamanan produk secara menyeluruh untuk mem | evidence link, text | manual |

#### F.28 Dampak Produk/Jasa

- File: `3. Soc/F.28.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.28`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.28` | F.28 Dampak Produk/Jasa | Uraian -> Catatan untuk 2026: perlu dilakukan survei kepuasan pelanggan yang mencakup dampak positif | evidence link, text | manual |

#### F.29 Jumlah Produk yang Ditarik Kembali

- File: `3. Soc/F.29.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.29`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.29` | F.29 Jumlah Produk yang Ditarik Kembali | Uraian -> Tidak ada produk yang ditarik kembali, karena produk telah mengikuti standar kualitas sert | evidence link, text | manual |

#### F.30 Survei Kepuasan Pelanggan terhadap Produk dan/atau Jasa Keuangan Berkelanjutan

- File: `3. Soc/F.30.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.30`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.30` | F.30 Survei Kepuasan Pelanggan terhadap Produk dan/atau Jasa Keuangan Berkelanju | Uraian -> Di 2025 Perusahaan belum melakukan evaluasi kepuasan pelanggan secara resmi.  Catatanuntuk // Deskripsi/Nilai -> Hasil Survei Kepuasan Pelanggan (Skala 1-5); Pencapaian Hasil Survei terhadap Target (Skal | evidence link, text | manual |

### 4. Gov

#### 205 Antikorupsi

- File: `4. Gov/205.xlsx`
- Sheet: `COVER`, `SUMMARY`, `205-1`, `205-2`, `205-3`; tersembunyi: `master`
- link ke file lain: `2. GRI 2 - Aktivitas dan Pekerja.xlsx`
- sel error tersimpan: `205-2` 48
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `205-1` | 205-1 Operasi-operasi yang dinilai memiliki risiko terkait korupsi | Uraian -> Catatan untuk 2026: pengungkapan ini dapat dikalkulasi dari informasi di dalam risk regist // Uraian -> Upaya pencegahan praktik korupsi diterapkan melalui penciptaan lingkungan kerja yang berin | text | manual |
| `205-2` | 205-2 Komunikasi dan pelatihan tentang kebijakan dan prosedur antikorupsi | Uraian // Uraian // Uraian // Uraian // Uraian | text | manual |
| `205-3` | 205-3 Insiden korupsi yang terbukti dan tindakan yang diambil | Deskripsi/Jenis Insiden*/Jumlah/Keterangan -> A. Jumlah total dan sifat insiden korupsi yang terbukti; B. Jumlah total insiden yang terb | evidence link, text | manual |

#### 206 Perilaku Antipersaingan

- File: `4. Gov/206.xlsx`
- Sheet: `COVER`, `SUMMARY`, `206-1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `206-1` | 206-1 Langkah-langkah hukum untuk perilaku antipersaingan, praktik antipakat dan | Uraian // Uraian | text | manual |

#### 308 Penilaian Lingkungan Pemasok

- File: `4. Gov/308.xlsx`
- Sheet: `COVER`, `SUMMARY`, `308-1`, `308-2`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `308-1` | 308-1 Seleksi pemasok baru dengan menggunakan kriteria lingkungan | No/Deskripsi/Jumlah/Satuan -> A | evidence link, rowsTable | manual |
| `308-2` | 308-2 Dampak lingkungan negatif dalam rantai pasokan dan tindakan yang telah dia | No/Deskripsi/Jumlah/Satuan/Jenis Dampak Negatif -> A; B; C; D | evidence link, rowsTable | manual |

#### 405 Keanekaragaman dan Peluang Setara

- File: `4. Gov/405.xlsx`
- Sheet: `COVER`, `SUMMARY`, `405-1`, `405-2`; tersembunyi: `405-02`, `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `405-1` | 405-1 Keanekaragaman badan tata kelola dan karyawan | - | text | manual |
| `405-2` | 405-2 Rasio gaji pokok dan remunerasi perempuan dibandingkan laki-laki | Uraian -> Kebijakan pengupahan diterapkan secara adil tanpa diskriminasi gender, dengan tingkat remu // Uraian -> Lokasi operasi yang signifikan adalah lokasi yang berkontribusi dan/atau lokasi di mana Pe | evidence link, text | manual |

#### 406 Nondiskriminasi

- File: `4. Gov/406.xlsx`
- Sheet: `COVER`, `SUMMARY`, `406-1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `406-1` | 406-1 Insiden diskriminasi dan tindakan perbaikan yang dilakukan | Uraian // Uraian | text | manual |

#### 414 Penilaian Sosial Pemasok

- File: `4. Gov/414.xlsx`
- Sheet: `COVER`, `SUMMARY`, `414-1`, `414-2`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `414-1` | 414-1 Seleksi pemasok baru dengan menggunakan kriteria sosial | No/Deskripsi/Jumlah/Satuan -> A | evidence link, text | manual |
| `414-2` | 414-2 Dampak sosial negatif dalam rantai pasokan dan tindakan yang telah diambil | Deskripsi/Jumlah/jenis dampak sosial negatif/Mitigasi -> A. Jumlah pemasok yang dinilai untuk dampak sosial; B. Jumlah pemasok yang diidentifikasi | evidence link, rowsTable | manual |

#### 415 Kebijakan Publik

- File: `4. Gov/415.xlsx`
- Sheet: `COVER`, `SUMMARY`, `415-1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `415-1` | 415-1 Kontribusi politik | Uraian -> Pada tahun 2025 Perusahaan tidak memiliki kontribusi politik.; B // Uraian -> Pada tahun 2025 Perusahaan tidak memiliki kontribusi politik. | evidence link, text | manual |

#### A.1 Penjelasan Strategi Keberlanjutan

- File: `4. Gov/A.1.xlsx`
- Sheet: `COVER`, `SUMMARY`, `A.1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `A.1` | A.1 Penjelasan Strategi Keberlanjutan | Uraian -> Strategi keberlanjut tercakup dalam SR 2025 hal. 15-17.  Catatan untuk 2026: diupayakan su | evidence link, text | manual |

#### D.1 Penjelasan Direksi

- File: `4. Gov/D.1.xlsx`
- Sheet: `COVER`, `SUMMARY`, `D.1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `D.1` | D.1 Penjelasan Direksi | - | text | manual |

#### E.1 Penanggung Jawab Penerapan Keuangan Berkelanjutan

- File: `4. Gov/E.1.xlsx`
- Sheet: `COVER`, `SUMMARY`, `E.1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `E.1` | E.1 Penanggung Jawab Penerapan Keuangan Berkelanjutan | Uraian | text | manual |

#### E.2 Pengembangan Kompetensi Terkait Keuangan Berkelanjutan

- File: `4. Gov/E.2.xlsx`
- Sheet: `COVER`, `SUMMARY`, `E.2`, `data_source`; tersembunyi: `master`
- sel error tersimpan: `data_source` 8
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `E.2` | E.2 Pengembangan Kompetensi Terkait Keuangan Berkelanjutan | Uraian | text | manual |

#### E.3 Penilaian Risiko atas Penerapan Keuangan Keberlanjutan

- File: `4. Gov/E.3.xlsx`
- Sheet: `COVER`, `SUMMARY`, `E.3`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `E.3` | E.3 Penilaian Risiko atas Penerapan Keuangan Keberlanjutan | Uraian -> Perseroan belum memiliki unit atau fungsi manajemen risiko yang terpusat. Namun demikian,  // Uraian -> Perseroan belum memiliki unit atau fungsi manajemen risiko yang terpusat. Namun demikian, | evidence link, text | manual |

#### E.4 Hubungan Dengan Pemangku Kepentingan

- File: `4. Gov/E.4.xlsx`
- Sheet: `COVER`, `SUMMARY`, `E.4`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `E.4` | E.4 Hubungan Dengan Pemangku Kepentingan | Uraian // Uraian | text | manual |

#### E.5 Permasalahan terhadap Penerapan Keuangan Berkelanjutan

- File: `4. Gov/E.5.xlsx`
- Sheet: `COVER`, `SUMMARY`, `E.5`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `E.5` | E.5 Permasalahan terhadap Penerapan Keuangan Berkelanjutan | Uraian -> Dinamika operasi dan aktivitas Perseroan terus meningkat, mencakup eksplorasi untuk memper | evidence link, text | manual |

#### F.1 Kegiatan Membangun Budaya Keberlanjutan

- File: `4. Gov/F.1.xlsx`
- Sheet: `COVER`, `SUMMARY`, `F.1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `F.1` | F.1 Kegiatan Membangun Budaya Keberlanjutan | Uraian -> Komitmen keberlanjutan Perusahaan diawali dengan penandatanganan Pernyataan Komitmen ESG o | text | manual |

#### G.1 Verifikasi Tertulis dari Pihak Independen (jika ada)

- File: `4. Gov/G.1.xlsx`
- Sheet: `COVER`, `SUMMARY`, `G.1`; tersembunyi: `master`
- link ke file lain: `1. GRI 2 - Organisasi dan Praktik Pelaporan.xlsx`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `G.1` | G.1 Verifikasi Tertulis dari Pihak Independen (jika ada) | Uraian | evidence link, text | manual |

### 5. Pu

#### C.1 Visi, Misi, dan Nilai Keberlanjutan

- File: `5. Pu/C.1.xlsx`
- Sheet: `COVER`, `SUMMARY`, `C.1`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `C.1` | C.1 Visi, Misi, dan Nilai Keberlanjutan | Uraian | text | manual |

#### C.2 Alamat Perusahaan

- File: `5. Pu/C.2.xlsx`
- Sheet: `COVER`, `SUMMARY`, `C.2`; tersembunyi: `master`
- link ke file lain: `1. GRI 2 - Organisasi dan Praktik Pelaporan.xlsx`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `C.2` | C.2 Alamat Perusahaan | - | text | manual |

#### C.3 Skala Usaha

- File: `5. Pu/C.3.xlsx`
- Sheet: `COVER`, `SUMMARY`, `C.3`, `data_source`; tersembunyi: `401-01-old`, `master`
- link ke file lain: `1. GRI 2 - Organisasi dan Praktik Pelaporan.xlsx`
- sel error tersimpan: `401-01-old` 1
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `C.3` | C.3 Skala Usaha | Deskripsi/Jumlah/Satuan -> Total Kapitalisasi; Liabilitas; Ekuitas; Total Aset // Uraian -> D // Uraian -> - Kantor Pusat Jakarta (HO) - Bakan - Penjom - Seruyung - Lanut - Doup | evidence link, rowsTable, text | manual |

#### C.4 Produk, Layanan, dan Kegiatan Usaha yang Dijalankan

- File: `5. Pu/C.4.xlsx`
- Sheet: `COVER`, `SUMMARY`, `C.4`; tersembunyi: `master`
- link ke file lain: `2. GRI 2 - Aktivitas dan Pekerja.xlsx`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `C.4` | C.4 Produk, Layanan, dan Kegiatan Usaha yang Dijalankan | Uraian | evidence link, text | manual |

#### C.5 Keanggotaan pada Asosiasi

- File: `5. Pu/C.5.xlsx`
- Sheet: `COVER`, `SUMMARY`, `C.5`; tersembunyi: `master`
- link ke file lain: `4. GRI 2 - Strategi, Kebijakan, dan Praktik.xlsx`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `C.5` | C.5 Produk, Layanan, dan Kegiatan Usaha yang Dijalankan | - | text | manual |

#### C.6 Perubahan Emiten dan Perusahaan Publik yang Bersifat Signifikan

- File: `5. Pu/C.6.xlsx`
- Sheet: `COVER`, `SUMMARY`, `C.6`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `C.6` | C.6 Perubahan Emiten dan Perusahaan Publik yang Bersifat Signifikan | Uraian -> Tahun 2025 tidak ada perubahan Emiten | evidence link, text | manual |

#### G.2 Lembar Umpan Balik

- File: `5. Pu/G.2.xlsx`
- Sheet: `COVER`, `SUMMARY`, `G.2`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `G.2` | G.2 Lembar Umpan Balik | Uraian -> Lembar atau form umpan balik tercakup dalam SR 2025 hal. 161. | evidence link, text | manual |

#### G.3 Tanggapan terhadap Umpan Balik Laporan Keberlanjutan Tahun Sebelumnya

- File: `5. Pu/G.3.xlsx`
- Sheet: `COVER`, `SUMMARY`, `G.3`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `G.3` | G.3 Tanggapan terhadap Umpan Balik Laporan Keberlanjutan Tahun Sebelumnya | Uraian -> Sepanjang 2025, J Resources menerima tanggapan atas Laporan Keberlanjutan 2024 dari pemang | evidence link, text | manual |

#### G.4 Daftar Pengungkapan Sesuai Peraturan Otoritas Jasa Keuangan Nomor 51/POJK.03/2017 tentang Penerapan Keuangan Berkelanjutan bagi Lembaga Jasa Keuangan, Emiten dan Perusahaan Publik

- File: `5. Pu/G.4.xlsx`
- Sheet: `COVER`, `SUMMARY`, `G.4`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `G.4` | G.4 Daftar Pengungkapan Sesuai Peraturan Otoritas Jasa Keuangan Nomor 51/POJK.03 | - | text | manual |

#### Sertifikasi & Apresiasi (Penghargaan)

- File: `5. Pu/Sertifikasi & Apresiasi.xlsx`
- Sheet: `COVER`, `Sertifikasi`, `Apresiasi`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `Sertifikasi` |  | - | text | manual |
| `Apresiasi` |  | No/Judul Penghargaan/Penyelengara/Tanggal Penyerahan Penghargaan/Keterangan -> Indonesia’s SDGs Action Awards (SAA) 2025; Penghargaan Kinerja Keselamatan Pertambangan at | rowsTable | manual |

### 6. GRI 2

#### 2 Pengungkapan Umum - Organisasi dan Praktik Pelaporan

- File: `6. GRI 2/1. GRI 2 - Organisasi dan Praktik Pelaporan.xlsx`
- Sheet: `COVER`, `SUMMARY`, `2-1`, `2-2`, `2-3`, `2-4`, `2-5`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `2-1` | 2-1 Rincian organisasi | Deskripsi/Uraian -> A. Melaporkan nama resmi Perusahaan; B. Melaporkan jenis kepemilikan dan bentuk hukum Peru | evidence link, text | manual |
| `2-2` | 2-2 Entitas yang dimasukkan dalam pelaporan keberlanjutan organisasi | - | text | manual |
| `2-3` | 2-3 Periode, frekuensi, dan titik kontak pelaporan | - | text | manual |
| `2-4` | 2-4 Penyajian kembali informasi | Uraian -> Ya, terdapat pernyataan kembali (restatement) untuk data energi, emisi, konsumsi air, laha | text | manual |
| `2-5` | 2-5 Penjaminan eksternal | Uraian -> Proses penjaminan (assurance) atas laporan ini dilakukan oleh pihak independen, yaitu PT S // Uraian -> Tercakup dalam Independent Assurance Statement (SR 2025, hal. 146). | evidence link, text | manual |

#### 2 Pengungkapan Umum - Aktivitas dan Pekerja

- File: `6. GRI 2/2. GRI 2 - Aktivitas dan Pekerja.xlsx`
- Sheet: `COVER`, `SUMMARY`, `2-6`, `2-7`, `2-8`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `2-6` | 2-6 Aktivitas, rantai nilai, dan hubungan bisnis lainnya | Uraian -> PT J Resources Asia Pasifik Tbk menjalankan kegiatan usaha di bidang pertambangan emas yan // Uraian -> Perusahaan mengelola lima proyek pertambangan utama yang berlokasi di berbagai wilayah. Ta // Uraian -> Catatan untuk 2026: disampaikan hubungan d | evidence link, text | manual |
| `2-7` | 2-7 Entitas anak yang dilaporkan, entitas anak yang datanya dikonsolidasi dalam  | Uraian // Uraian | text | manual |
| `2-8` | 2-8 Pekerja yang bukan pekerja langsung | Jenis pekerja paling umum*/Jumlah Karyawan/Hubungan kontrak dengan Perusahaan**/Jenis Pekerjaan yang dilakukan // Uraian // Uraian | text | manual |

#### 2 Pengungkapan Umum - Tata Kelola

- File: `6. GRI 2/3. GRI 2 - Tata Kelola.xlsx`
- Sheet: `COVER`, `SUMMARY`, `2-9`, `2-10`, `2-11`, `2-12`, `2-13`, `2-14`, `2-15`, `2-16`, `2-17`, `2-18`, `2-19`, `2-20`, `2-21`; tersembunyi: `master`
- sel error tersimpan: `2-21` 2
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `2-9` | 2-9 Struktur dan komposisi tata kelola | Uraian -> Struktur tata kelola Perusahaan ditetapkan sesuai dengan Undang-Undang Nomor 40 Tahun 2007 // Uraian -> Dalam pelaksanaan tugas dan tanggung jawabnya, masing-masing organ didukung oleh komite-ko | evidence link, text | manual |
| `2-10` | 2-10 Pencalonan dan pemilihan badan tata kelola tertinggi | Uraian -> Dalam pelaksanaan tugas dan tanggung jawabnya, masing-masing organ didukung oleh komite-ko // Uraian -> Contoh: Proses pencalonan dan pemilihan anggota Dewan Komisaris dan Direksi dilakukan mela | evidence link, text | manual |
| `2-11` | 2-11 Ketua badan tata kelola tertinggi | Uraian -> Di SR 2025 tidak dijelaskan.  Catatan contoh untuk 2026: Ketua badan tata kelola tertinggi // Uraian -> Di SR 2025 tidak dijelaskan. | evidence link, text | manual |
| `2-12` | 2-12 Peran badan tata kelola tertinggi dalam mengawasi manajemen dampak | Uraian -> Di SR 2025 tidak dijelaskan.; B // Uraian -> Di SR 2025 tidak dijelaskan.; C // Uraian -> Di SR 2025 tidak dijelaskan. | evidence link, text | manual |
| `2-13` | 2-13 Delegasi tanggung jawab untuk mengelola dampak | Uraian -> Penerapan tata kelola keberlanjutan dilaksanakan secara terintegrasi dan menjadi tanggung  // Uraian -> Pelaporan dilakukan secara berkala sesuai dengan Sustainability Blueprint J Resources. | evidence link, text | manual |
| `2-14` | 2-14 Peran badan tata kelola tertinggi dalam pelaporan keberlanjutan | Uraian -> Ya, badan tata kelola bertanggung jawab untuk meninjau dan menyetujui informasi yang dilap // Uraian | text | manual |
| `2-15` | 2-15 Konflik kepentingan | Uraian -> Perusahaan menerapkan mekanisme pengendalian untuk memastikan tidak terjadinya benturan ke // Uraian -> Perusahaan menerapkan mekanisme pengendalian untuk memastikan tidak terjadinya benturan ke | evidence link, text | manual |
| `2-16` | 2-16 Komunikasi masalah penting | Uraian -> Perusahaan memperkuat keterlibatan seluruh unit kerja melalui dialog, peningkatan komunika // Uraian -> Di SR 2025 tidak diungkap.  Catatan untuk 2026: harus menjadi bagian dari perbaikan tata k | evidence link, text | manual |
| `2-17` | 2-17 Pengetahuan kolektif badan tata kelola tertinggi | - | text | manual |
| `2-18` | 2-18 Evaluasi kinerja badan tata kelola tertinggi | Uraian -> DI SR 2025 belum diungkap.  Catatan untuk 2026: perlu dilakukan peninjauan dengan laporan  // Uraian -> DI SR 2025 belum diungkap.  Catatan untuk 2026: perlu dilakukan peninjauan dengan laporan  // Uraian -> DI SR 2025 belum diungkap.  Catatan untuk  | evidence link, text | manual |
| `2-19` | 2-19 Kebijakan remunerasi | Uraian -> Di SR 2025 tidak dijelaskan mengenai kebijakan remunerasi untuk anggota badan tata kelola  // Uraian -> Di SR 2025 tidak dijelaskan mengenai kebijakan remunerasi untuk anggota badan tata kelola | evidence link, text | manual |
| `2-20` | 2-20 Proses untuk menentukan remunerasi | Uraian -> Di SR 2025 tidak dijelaskan mengenai kebijakan remunerasi untuk anggota badan tata kelola  // Uraian -> Di SR 2025 tidak dijelaskan mengenai kebijakan remunerasi untuk anggota badan tata kelola | evidence link, text | manual |
| `2-21` | 2-21 Rasio kompensasi total tahunan | Deskripsi/Nilai -> Kompensasi total tahunan untuk individu dengan bayaran tertinggi di organisasi; Nilai teng // Uraian -> Di SR 2025 tidak dijelaskan mengenai kebijakan remunerasi untuk anggota badan tata kelola | evidence link, rowsTable, text | manual |

#### 2 Pengungkapan Umum - Strategi, Kebijakan, dan Praktik

- File: `6. GRI 2/4. GRI 2 - Strategi, Kebijakan, dan Praktik.xlsx`
- Sheet: `COVER`, `SUMMARY`, `2-22`, `2-23`, `2-24`, `2-25`, `2-26`, `2-27`, `2-28`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `2-22` | 2-22 Penjelasan Strategi Keberlanjutan | Uraian -> J Resources telah menetapkan topik material dan merumuskan arah strategis yang terintegras | text | manual |
| `2-23` | 2-23 Komitmen kebijakan | Uraian -> J Resources telah menetapkan topik material dan merumuskan arah strategis yang terintegras // Uraian // Uraian // Uraian // Uraian // Uraian | text | manual |
| `2-24` | 2-24 Komitmen kebijakan | Uraian | text | manual |
| `2-25` | 2-25 Proses untuk memperbaiki dampak negatif | Uraian -> Catatan untuk 2026: pengisian pengungkapan ini dilakukan item by item, yang nantinya akan  // Uraian -> Catatan untuk 2026: pengisian pengungkapan ini dilakukan item by item, yang nantinya akan  // Uraian -> Catatan untuk 2026: pengisian pengungkapan | evidence link, text | manual |
| `2-26` | 2-26 Mekanisme untuk mencari nasihat dan mengemukakan masalah | Uraian -> Catatan untuk 2026: pengisian pengungkapan ini dilakukan item by item, yang nantinya akan | evidence link, text | manual |
| `2-27` | 2-27 Kepatuhan terhadap hukum dan peraturan | Uraian -> Catatan untuk 2026: pengisian pengungkapan ini dilakukan item by item, yang nantinya akan  // Uraian -> Catatan untuk 2026: pengisian pengungkapan ini dilakukan item by item, yang nantinya akan  // Uraian -> Catatan untuk 2026: pengisian pengungkapan | evidence link, text | manual |
| `2-28` | 2-28 Asosiasi keanggotaan | - | text | manual |

#### 2 Pengungkapan Umum - Keterlibatan Pemangku Kepentingan

- File: `6. GRI 2/5. GRI 2 - Keterlibatan Pemangku Kepentingan.xlsx`
- Sheet: `COVER`, `SUMMARY`, `2-29`, `2-30`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `2-29` | 2-29 Pendekatan untuk keterlibatan pemangku kepentingan | Uraian -> Catatan untuk 2026: pengisian pengungkapan ini dilakukan item by item, yang nantinya akan | text | manual |
| `2-30` | 2-30  Perjanjian perundingan kolektif | Uraian -> Di tahun 2025, jumlah karyawan yang masuk ke dalam Perjanjian Kerja Kolektif (PKB) tidak d // Uraian -> Di 2025 hal ini tidak diungkap. Namun, karyawan yang tidak dicakup dalam PKB akan mengikut | evidence link, text | manual |

### 7. GRI 3

#### 3 Topik Material

- File: `7. GRI 3/3 - Topik Material.xlsx`
- Sheet: `COVER`, `SUMMARY`, `3-1`, `3-2`, `3-3`; tersembunyi: `master`
- Belum punya `Data-Konsolidasi`. Isian dari tata letak sheet:

| Sheet | Judul | Blok isian (header -> baris) | Usulan jenis isian | Telemetry |
|---|---|---|---|---|
| `3-1` | 3-1 Proses untuk menentukan topik material | Uraian -> J Resources melakukan penelaahan dan diskusi internal atas laporan keberlanjutan sebelumny // Uraian -> Dalam menentukan topik material sudah dilakukan Focus Group Discussion (FGD) dengan mengha | evidence link, text | manual |
| `3-2` | 3-2 Daftar topik material | Uraian -> Aspek Lingkungan: - Perubahan Iklim - Reklamasi Pascatambang - Air dan Efluen - Limbah  As // Uraian -> Tidak ada perubahan dari tahun 2024. | evidence link, text | manual |
| `3-3` | 3-3 Manajemen topik material | Uraian // Uraian // Uraian // Uraian // Uraian // Uraian | text | manual |

## 4. Dashboard (`Dashboard/Dashboard.xlsx`)

Tabel `Data` (35.164 baris, tersembunyi) berskema sama dengan `Data-Konsolidasi`. Query Power Query di file ini kosong (`Section1.m` tanpa query), jadi data saat ini berupa nilai tempelan, bukan hasil Refresh seperti yang diuraikan sheet `Panduan Refresh`. Tahun di data: 2025, 2026, 2028, 2030, 2031, 2032 (2028-2032 data uji).

| Sheet | Item sumber (Kategori -> baris `No_Qz`) | Rumus | Catatan |
|---|---|---|---|
| `Panduan Refresh` | seluruh tabel | ROWS, COUNTIF ke Master, ISNUMBER(Tahun), cek duplikat | pemeriksaan kesehatan data; cek duplikat mengacu kode `ECO`/`B.1` yang tidak ada di data |
| `Data Status` | `302-1-E` (semua baris) | MAXIFS(Period, Number_Value<>0)/12 per site x 3 tahun | progres pengisian dihitung hanya dari energi |
| `Energi` | `302-1-E` -> 1 realisasi, 2 target | SUMPRODUCT per bulan, pemilih site (All Sites) | GJ |
| `Energi Tahunan` | `302-1-E` -> 1, 2 | SUMPRODUCT per site x 3 tahun; capaian = realisasi/target | GJ |
| `Emisi` | `305-1-A`, `305-2-A`, `305-3-A` -> 21 target, selain 21 realisasi | SUMPRODUCT per bulan | tCO2e; target dikenali dari posisi baris 21 |
| `Emisi Tahunan` | sama dengan Emisi | SUMPRODUCT per site x 3 tahun | tCO2e |
| `K3` | `403-9-A` (karyawan), `403-9-B` (mitra kerja) -> 1-22 | COUNTIFS/SUMIFS per site | jumlah kejadian dan rasio dalam satu tabel |
| `K3 Tahunan` | `403-9-A/B` -> kelompok 1-5, 6-16, 17-19, 21-22 | SUMIFS dengan rentang `No_Qz` | rentang posisi baris, rapuh bila baris disisipkan |
| `Konsumsi Air` | `303-5-A.2` -> 1-7 | SUMIFS per site | satuan campur m3 dan ML |
| `Konsumsi Air Tahunan` | `303-5-A.1` -> 1; `303-5-A.2` -> 2-7 | SUMIFS per site x 3 tahun | - |
| `Konsolidasi Pembuangan Air` | `303-4-A.2` -> semua baris | SUMIFS per bulan x site | tahun diambil dari baris pertama tabel |
| `Pembuangan Air Tahunan` | `303-4-A.2` | SUMIFS per site x 3 tahun | teks satuan m3 diganti ML lewat SUBSTITUTE |
| `Laporan 201-1 Tahunan` | `201-1-A` -> 1-6 | SUMIFS x 3 tahun; nilai ditahan = pendapatan - distribusi | USD, tingkat grup |
| `Laporan 204-1 Tahunan` | `204-1-A` -> 1-4 | SUMIFS per site x 3 tahun; porsi lokal = lokal/(lokal+internasional) | Pemasok dan USD |
| `Chart 204-1` | dari Laporan 204-1 Tahunan | N() | grafik |
| `Laporan 413 Realisasi` | `413-1-C` (Period > 0), `413-1-D` -> 1-8 | SUMIFS x 3 tahun; porsi per bidang | USD |
| `413 Anggaran Vs Realisasi` | `413-1-C` Period 0 (anggaran), `413-1-D` (realisasi) -> 1-8 | selisih dan persentase serapan | anggaran disandikan sebagai Period = 0 |

Sheet lain: `Master` (6 site), `Data` (tabel sumber), `Sheet1` (kosong).


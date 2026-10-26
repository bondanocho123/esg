namespace GriForms.Tests.Support;

public static class SampleDefinitions
{
    // Contoh topik 403 yang memuat semua tipe input
    public const string Topic403 = """
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
              "validation": { "min": 0, "integerOnly": true },
              "telemetry": { "feasibility": "manual", "note": "daftar hadir pelatihan" }
            }
          ]
        },
        {
          "no": "403-9",
          "title": "Cedera terkait pekerjaan",
          "questions": [
            { "key": "403-9-A", "type": "number", "label": "Jam kerja", "unit": "jam",
              "telemetry": { "feasibility": "manual", "note": "bisa diintegrasikan dari sistem absensi" } },
            { "key": "403-9-B", "type": "boolean", "label": "Ada kecelakaan fatal?" },
            {
              "key": "403-9-C",
              "type": "select",
              "label": "Sumber data",
              "options": [ { "value": "hse", "label": "HSE" }, { "value": "hr", "label": "HR" } ],
              "visibleWhen": "403-9-B == 'true'"
            },
            {
              "key": "403-9-D",
              "type": "rowsTable",
              "label": "Rincian kecelakaan",
              "rows": [ { "key": "r1", "label": "Kejadian 1" } ],
              "columns": [ { "key": "jumlah", "label": "Jumlah", "valueType": "number" }, { "key": "ket", "label": "Keterangan" } ],
              "telemetry": { "feasibility": "manual" }
            }
          ]
        }
      ]
    }
    """;
}

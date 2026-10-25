using System.Text.Json.Nodes;
using GriForms.Application.Calculations;
using GriForms.Application.Forms;
using GriForms.Tests.Support;

namespace GriForms.Tests.Forms;

// Aturan definisi sebelum publish (bab 6). Setiap kasus mengubah satu bagian dari contoh topik 403 yang valid.
public class FormDefinitionValidatorTests
{
    private static readonly FormDefinitionValidator Validator =
        new(new JsonFormDefinitionReader(), new CalcRegistry([new RowTotalCalculation()]));

    private static DefinitionCheck Check(string json, string topicCode = "403", int versionNo = 1) =>
        Validator.Check(json, topicCode, versionNo);

    private static string Mutate(Action<JsonObject> change)
    {
        var root = JsonNode.Parse(SampleDefinitions.Topic403)!.AsObject();
        change(root);
        return root.ToJsonString();
    }

    private static JsonObject Question(JsonObject root, int disclosure, int question) =>
        root["disclosures"]![disclosure]!["questions"]![question]!.AsObject();

    private static void AssertIssue(DefinitionCheck check, string path)
    {
        Assert.False(check.IsValid);
        Assert.Contains(check.Issues, i => i.Path == path);
    }

    [Fact]
    public void Sample_definition_with_every_input_type_is_valid()
    {
        var check = Check(SampleDefinitions.Topic403);

        Assert.True(check.IsValid, string.Join("; ", check.Issues.Select(i => $"{i.Path}: {i.Message}")));
        Assert.NotNull(check.Definition);
    }

    [Fact]
    public void Header_must_match_topic_and_version()
    {
        var check = Check(SampleDefinitions.Topic403, topicCode: "302", versionNo: 2);

        AssertIssue(check, "formCode");
        AssertIssue(check, "versionNo");
    }

    [Fact]
    public void Effective_years_must_be_ordered()
    {
        AssertIssue(Check(Mutate(r => r["effectiveToYear"] = 2024)), "effectiveToYear");
        AssertIssue(Check(Mutate(r => r["effectiveFromYear"] = 0)), "effectiveFromYear");
    }

    [Fact]
    public void Unreadable_json_reports_line_without_definition()
    {
        var check = Check("{ \"formCode\": \"403\", ");

        Assert.Null(check.Definition);
        var issue = Assert.Single(check.Issues);
        Assert.Contains("baris", issue.Message);
    }

    [Fact]
    public void Unknown_input_type_is_reported_on_its_path()
    {
        var check = Check(Mutate(r => Question(r, 0, 0)["type"] = "slider"));

        Assert.Null(check.Definition);
        AssertIssue(check, "disclosures[0].questions[0].type");
    }

    [Fact]
    public void Question_keys_must_be_unique_across_disclosures()
    {
        var check = Check(Mutate(r => Question(r, 1, 0)["key"] = "403-5-A"));

        AssertIssue(check, "disclosures[1].questions[0].key");
    }

    [Theory]
    [InlineData("403/9")]
    [InlineData("403|9")]
    [InlineData("")]
    public void Question_key_must_not_contain_cell_separators(string key)
    {
        AssertIssue(Check(Mutate(r => Question(r, 1, 0)["key"] = key)), "disclosures[1].questions[0].key");
    }

    [Fact]
    public void Disclosure_numbers_must_be_unique()
    {
        AssertIssue(Check(Mutate(r => r["disclosures"]![1]!["no"] = "403-5")), "disclosures[1].no");
    }

    [Fact]
    public void Disclosure_needs_questions()
    {
        AssertIssue(Check(Mutate(r => r["disclosures"]![0]!["questions"] = new JsonArray())), "disclosures[0].questions");
        AssertIssue(Check(Mutate(r => r["disclosures"] = new JsonArray())), "disclosures");
    }

    [Fact]
    public void Select_needs_unique_options()
    {
        AssertIssue(Check(Mutate(r => Question(r, 1, 2)["options"] = new JsonArray())), "disclosures[1].questions[2].options");
        AssertIssue(Check(Mutate(r => Question(r, 1, 2)["options"]![1]!["value"] = "hse")),
            "disclosures[1].questions[2].options[1].value");
    }

    [Fact]
    public void Monthly_table_needs_rows_unless_custom_rows_are_allowed()
    {
        AssertIssue(Check(Mutate(r => Question(r, 0, 1)["rows"] = new JsonArray())), "disclosures[0].questions[1].rows");

        Assert.True(Check(Mutate(r =>
        {
            Question(r, 0, 1)["rows"] = new JsonArray();
            Question(r, 0, 1)["allowCustomRows"] = true;
        })).IsValid);
    }

    [Fact]
    public void Rows_table_needs_unique_columns()
    {
        AssertIssue(Check(Mutate(r => Question(r, 1, 3)["columns"] = new JsonArray())), "disclosures[1].questions[3].columns");
        AssertIssue(Check(Mutate(r => Question(r, 1, 3)["columns"]![1]!["key"] = "jumlah")),
            "disclosures[1].questions[3].columns[1].key");
        AssertIssue(Check(Mutate(r => Question(r, 1, 3)["columns"]![0]!["valueType"] = "angka")),
            "disclosures[1].questions[3].columns[0].valueType");
    }

    [Fact]
    public void Validation_bounds_must_be_consistent()
    {
        AssertIssue(Check(Mutate(r => Question(r, 1, 0)["validation"] = new JsonObject { ["min"] = 10, ["max"] = 1 })),
            "disclosures[1].questions[0].validation");
        AssertIssue(Check(Mutate(r => Question(r, 0, 0)["validation"] = new JsonObject { ["maxLength"] = 0 })),
            "disclosures[0].questions[0].validation.maxLength");
    }

    [Theory]
    [InlineData("403-9-B = 'true'")]       // operator tidak dikenal
    [InlineData("403-9-X == 'true'")]      // key tidak ada di form
    public void Visible_when_must_be_parseable_and_refer_to_existing_key(string expression)
    {
        AssertIssue(Check(Mutate(r => Question(r, 1, 2)["visibleWhen"] = expression)), "disclosures[1].questions[2].visibleWhen");
    }

    [Fact]
    public void Visible_when_may_refer_to_a_table_cell()
    {
        Assert.True(Check(Mutate(r => Question(r, 1, 2)["visibleWhen"] = "403-9-D/r1/ket != ''")).IsValid);
    }

    [Fact]
    public void Calc_ref_must_be_registered()
    {
        AssertIssue(Check(Mutate(r => Question(r, 0, 1)["calcRef"] = "ghg_total_v9")), "disclosures[0].questions[1].calcRef");
        Assert.True(Check(Mutate(r => Question(r, 0, 1)["calcRef"] = "row_total_v1")).IsValid);
    }
}

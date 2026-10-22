using GriForms.Application.Forms;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Submissions;
using GriForms.Tests.Support;

namespace GriForms.Tests.Integration;

public class AnswerWriterTests : IDisposable
{
    private readonly TestDb _db = new();

    private async Task SaveAsync(int submissionId, params CellChange[] changes)
    {
        await using var ctx = _db.CreateContext();
        var writer = new AnswerWriter(ctx, ctx, new JsonFormDefinitionReader(), _db.User, _db.Clock);
        await writer.SaveCellsAsync(submissionId, changes, CancellationToken.None);
    }

    [Fact]
    public async Task Saves_cells_with_unit_and_disclosure_from_definition_and_audits_them()
    {
        var id = _db.SeedSubmission(1, _db.SeedTopic());

        await SaveAsync(id,
            new CellChange(FormState.CellKey("403-5-B/k3-dasar", "2026-01"), new AnswerValue(null, 12)),
            new CellChange("403-5-A", new AnswerValue("Pelatihan rutin", null)));

        using var db = _db.CreateContext();
        var monthly = db.Answers.Single(a => a.QuestionKey == "403-5-B/k3-dasar");
        Assert.Equal("2026-01", monthly.PeriodKey);
        Assert.Equal("orang", monthly.Unit);
        Assert.Equal("403-5", monthly.DisclosureNo);
        Assert.Equal("user-1", monthly.UpdatedBy);
        Assert.Equal(2, db.AuditEntries.Count(e => e.EntityType == nameof(Answer) && e.Action == "Added"));
    }

    [Fact]
    public async Task Update_and_clear_are_audited_and_bump_row_version()
    {
        var id = _db.SeedSubmission(1, _db.SeedTopic());
        await SaveAsync(id, new CellChange("403-9-A", new AnswerValue(null, 100)));

        await SaveAsync(id, new CellChange("403-9-A", new AnswerValue(null, 120)));
        using (var db = _db.CreateContext())
        {
            var answer = db.Answers.Single(a => a.QuestionKey == "403-9-A");
            Assert.Equal(120m, answer.NumberValue);
            Assert.Equal(1, answer.RowVersion);
            var audit = db.AuditEntries.Single(e => e.Action == "Modified");
            Assert.Contains("100", audit.OldValue);
            Assert.Contains("120", audit.NewValue);
        }

        await SaveAsync(id, new CellChange("403-9-A", null));
        using (var db = _db.CreateContext())
        {
            Assert.Empty(db.Answers.Where(a => a.QuestionKey == "403-9-A"));
            Assert.Single(db.AuditEntries.Where(e => e.Action == "Deleted"));
        }
    }

    [Fact]
    public async Task Rejects_changes_to_submission_that_is_not_editable()
    {
        var id = _db.SeedSubmission(1, _db.SeedTopic(), SubmissionStatus.Approved);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SaveAsync(id, new CellChange("403-9-A", new AnswerValue(null, 1))));
    }

    [Fact]
    public async Task Rejects_question_key_outside_definition()
    {
        var id = _db.SeedSubmission(1, _db.SeedTopic());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SaveAsync(id, new CellChange("999-1-A", new AnswerValue(null, 1))));
    }

    [Fact]
    public async Task Rejects_oversized_text()
    {
        var id = _db.SeedSubmission(1, _db.SeedTopic());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SaveAsync(id, new CellChange("403-5-A", new AnswerValue(new string('x', 20_001), null))));
    }

    [Fact]
    public async Task Rejects_blank_cell_key_and_null_change()
    {
        var id = _db.SeedSubmission(1, _db.SeedTopic());

        await Assert.ThrowsAsync<ArgumentException>(() => SaveAsync(id, new CellChange(" ", null)));
        await Assert.ThrowsAsync<ArgumentException>(() => SaveAsync(id, (CellChange)null!));
    }

    [Fact]
    public async Task Empty_change_list_is_a_no_op_even_for_unknown_submission()
    {
        await SaveAsync(12345);

        using var db = _db.CreateContext();
        Assert.Empty(db.Answers);
    }

    [Fact]
    public async Task Rejects_unknown_submission()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SaveAsync(999, new CellChange("403-9-A", new AnswerValue(null, 1))));
    }

    public void Dispose() => _db.Dispose();
}

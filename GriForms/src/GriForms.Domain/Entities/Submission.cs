using GriForms.Domain.Enums;

namespace GriForms.Domain.Entities;

public class Submission : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }   // wajib sama dengan tenant Site dan Period
    public int SiteId { get; set; }
    public int PeriodId { get; set; }
    public int TopicId { get; set; }
    public int FormVersionId { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
    public int RowVersion { get; set; }

    public Site Site { get; set; } = null!;
    public ReportingPeriod Period { get; set; } = null!;
    public Topic Topic { get; set; } = null!;
    public FormVersion FormVersion { get; set; } = null!;
    public List<Answer> Answers { get; set; } = new();
    public List<Signoff> Signoffs { get; set; } = new();

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

    // Transisi yang mungkin dari status tertentu beserta peran yang berhak; dipakai UI untuk menampilkan tombol
    public static IEnumerable<(SubmissionStatus To, RoleName Role)> TransitionsFrom(SubmissionStatus from) =>
        Allowed.Where(x => x.Key.From == from).Select(x => (x.Key.To, x.Value));

    public static bool RequiresComment(SubmissionStatus to) => to is SubmissionStatus.Rejected or SubmissionStatus.Draft;

    public void Transition(SubmissionStatus to, RoleName actorRole, string? comment)
    {
        // Period bisa belum dimuat (null) pada entity yang diambil tanpa Include; pemeriksaan penuh ada di use case.
        if (Period is { Status: PeriodStatus.Closed })
            throw new InvalidOperationException("Periode pelaporan sudah ditutup, status submission tidak bisa diubah.");

        if (!Allowed.TryGetValue((Status, to), out var required) || required != actorRole)
            throw new InvalidOperationException($"Transisi {Status} ke {to} tidak diizinkan untuk peran {actorRole}.");

        if (RequiresComment(to) && string.IsNullOrWhiteSpace(comment))
            throw new InvalidOperationException("Komentar atau alasan wajib saat menolak atau membuka kembali.");

        Status = to;
    }

    public bool IsEditableBy(RoleName role) =>
        role == RoleName.Contributor && (Status is SubmissionStatus.Draft or SubmissionStatus.Rejected);
}

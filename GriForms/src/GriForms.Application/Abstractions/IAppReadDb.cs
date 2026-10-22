using GriForms.Domain.Entities;

namespace GriForms.Application.Abstractions;

// Akses data: dipisah antara membaca dan menulis (ISP)
public interface IAppReadDb
{
    IQueryable<Site> Sites { get; }
    IQueryable<Submission> Submissions { get; }
    IQueryable<Answer> Answers { get; }
    IQueryable<FormVersion> FormVersions { get; }
}

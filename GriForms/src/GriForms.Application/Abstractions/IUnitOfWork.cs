namespace GriForms.Application.Abstractions;

public interface IUnitOfWork
{
    void Add<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
    Task<int> SaveChangesAsync(CancellationToken ct);
}

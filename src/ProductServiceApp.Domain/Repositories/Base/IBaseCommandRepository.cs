namespace ProductServiceApp.Domain.Repositories.Base;

public interface IBaseCommandRepository<T> : IBaseRepository where T : class
{
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken);
    Task<T> UpdateAsync(T entity, long id, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
}

namespace ProductServiceApp.Domain.Repositories.Base;

public interface IBaseQueryRepository<T> : IBaseRepository where T : class
{
    Task<T> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
}

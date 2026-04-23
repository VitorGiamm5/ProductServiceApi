namespace ProductServiceApp.Domain.Repositories.Base;

public interface IBaseQueryRepository<T> : IBaseRepository where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
}

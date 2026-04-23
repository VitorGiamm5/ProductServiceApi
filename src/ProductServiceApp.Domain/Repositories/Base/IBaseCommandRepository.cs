namespace ProductServiceApp.Domain.Repositories.Base;

public interface IBaseCommandRepository<T> : IBaseRepository where T : class
{
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(long id);
}

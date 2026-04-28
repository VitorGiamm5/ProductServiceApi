namespace ProductServiceApp.Domain.Business.Base;

/// <summary>
/// Contrato da camada de negócio com pipeline Inbox/Outbox.
/// </summary>
public interface IBaseBusinessService<TInObject, TOutObject>
    where TInObject : class
    where TOutObject : class
{
    Task<TOutObject> ExecuteAsync(TInObject input, CancellationToken ct = default);
}

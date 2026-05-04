using ProductServiceApp.Domain.Services.Base;

namespace ProductServiceApp.Domain.Services.Products.Base;

/// <summary>
/// Base interface for product business services, providing common operations for handling product-related business logic.
/// </summary>
/// <typeparam name="TInObject"></typeparam>
/// <typeparam name="TOutObject"></typeparam>
public interface IBaseProductBusinessService<TInObject, TOutObject>
    : IBaseBusinessService<TInObject, TOutObject>
    where TInObject : class
    where TOutObject : class
{
}

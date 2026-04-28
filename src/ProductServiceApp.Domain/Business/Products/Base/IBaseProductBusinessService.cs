using ProductServiceApp.Domain.Business.Base;

namespace ProductServiceApp.Domain.Business.Products.Base;

public interface IBaseProductBusinessService<TInObject, TOutObject>
    : IBaseBusinessService<TInObject, TOutObject>
    where TInObject : class
    where TOutObject : class
{
}

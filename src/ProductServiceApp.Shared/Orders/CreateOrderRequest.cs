namespace ProductServiceApp.Shared.Orders;

public class CreateOrderRequest
{
    public long? Id { get; set; } = 0L;
    public List<long> ProductIds { get; set; } = [];
    public bool? IsActive { get; set; } = true;
    public bool? IsDeleted { get; set; } = false;
}

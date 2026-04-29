namespace ProductServiceApp.Domain.Business.Orders.Dtos;

public class CreateOrderRequest
{
    public long? Id { get; set; } = 0L;
    public List<long> ProductIds { get; set; } = [];
    public bool? IsActive { get; set; } = true;
    public bool? IsDeleted { get; set; } = false;
}

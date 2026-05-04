using System.Text.Json.Serialization;

namespace ProductServiceApp.Domain.Services.Orders.Dtos;

public class CreateOrderRequest
{
    public long? Id { get; set; } = 0L;

    public List<OrderProductRequest> Products { get; set; } = [];

    public bool? IsActive { get; set; } = true;

    [JsonIgnore]
    public bool? IsDeleted { get; set; } = false;
}

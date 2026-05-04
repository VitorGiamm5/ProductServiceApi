using System.Text.Json.Serialization;

namespace ProductServiceApp.Domain.Services.Orders.Dtos;

public class CreateOrderRequest
{
    public long? Id { get; set; } = 0L;

    public List<long> ProductIds { get; set; } = [];

    public bool? IsActive { get; set; } = true;

    [JsonIgnore]
    public bool? IsDeleted { get; set; } = false;
}

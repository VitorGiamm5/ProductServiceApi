using System.Text.Json.Serialization;

namespace ProductServiceApp.Shared.Orders;

public class CreateOrderRequest
{
    public long? Id { get; set; } = 0L;
    public List<OrderProductRequest> Products { get; set; } = [];
    public bool? IsActive { get; set; } = true;
    public bool? IsDeleted { get; set; } = false;
}

public class OrderProductRequest
{
    public long ProductId { get; set; }

    [JsonIgnore]
    public int Quantity { get; set; } = 1;

    [JsonPropertyName("quatity")]
    public int Quatity
    {
        get => Quantity;
        set => Quantity = value;
    }
}

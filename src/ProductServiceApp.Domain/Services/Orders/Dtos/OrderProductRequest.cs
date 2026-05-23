using System.Text.Json.Serialization;

namespace ProductServiceApp.Domain.Services.Orders.Dtos;

[Serializable]
public class OrderProductRequest
{
    public long ProductId { get; set; }
    public int Quantity { get; set; } = 1;

    [JsonPropertyName("quatity")]
    public int? Quatity
    {
        get => null;
        set
        {
            if (value.HasValue)
            {
                Quantity = value.Value;
            }
        }
    }
}

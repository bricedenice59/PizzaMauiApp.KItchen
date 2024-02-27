using PizzaMauiApp.RabbitMq.Messages;

namespace PizzaMauiApp.Kitchen;

public class KitchenOrderDto
{
    public Guid OrderId { get; set; }
    public string UserId { get; set; } = null!;
    public List<IOrderItem>? Items { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
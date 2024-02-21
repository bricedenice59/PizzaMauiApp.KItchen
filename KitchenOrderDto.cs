using PizzaMauiApp.RabbitMq.Messages;

namespace PizzaMauiApp.Kitchen;

public class KitchenOrderDto
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public List<IOrderItem>? Items { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
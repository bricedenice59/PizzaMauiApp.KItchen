using MassTransit;
using PizzaMauiApp.RabbitMq.Messages;

namespace PizzaMauiApp.Kitchen.Consumers
{
    public class KitchenAppOrderReceivedConsumer(IProcessStackOrder<KitchenOrderDto> processStackOrder) : IConsumer<IKitchenMessage>
    {
        public Task Consume(ConsumeContext<IKitchenMessage> context)
        {
            processStackOrder.Enqueue(new KitchenOrderDto()
            {
                OrderId = context.Message.OrderId,
                UserId = context.Message.UserId,
                Items = context.Message.Items,
                CreatedAt = context.Message.CreatedAt
            });
            return Task.CompletedTask;
        }
    }
}
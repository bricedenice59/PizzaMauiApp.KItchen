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
                Items = context.Message.Items,
                ReceivedAt = DateTime.Now
            });
            return Task.CompletedTask;
        }
    }
}
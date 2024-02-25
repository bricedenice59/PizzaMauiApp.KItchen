using MassTransit;
using PizzaMauiApp.RabbitMq.Messages;

namespace PizzaMauiApp.Kitchen;

public sealed class ScopedBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ScopedBackgroundService> logger,
    IProcessStackOrder<KitchenOrderDto> processStackOrder) : BackgroundService
{
    private const string ClassName = nameof(ScopedBackgroundService);
    private const int OrderExpiresTimeoutInMinutes = 5;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} is running.", ClassName);

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        await MonitorOrdersAndActAsync(publishEndpoint, stoppingToken);
    }

    private async Task ProcessOrderAsync(IPublishEndpoint publishEndpoint, KitchenOrderDto order, CancellationToken stoppingToken)
    {
        order.ExpiresAt = DateTime.Now.Add(TimeSpan.FromMinutes(OrderExpiresTimeoutInMinutes));

        await Console.Out.WriteLineAsync($"Order arrived: {order.OrderId}");

        bool orderProcessed = false;
        bool isOrderAccepted = false;
        
        //let's simulate a human who might be busy and can/cannot answer in time
        var rnd = new Random();
        TimeSpan interactionWaitingTimeInMinutes = rnd.NextDouble() * TimeSpan.FromMinutes(10);
        bool interactionOrderCompleted = rnd.Next() > Int32.MaxValue / 2;

        char response = interactionOrderCompleted  ? 'A' : 'R';

        while (DateTime.Now < order.ExpiresAt && !orderProcessed)
        {
            await Console.Out.WriteLineAsync("Accept(A) or Reject(R)?");

            //that is blocking the queue... not a good choice
            //var keyStroke = await Console.In.ReadLineAsync(stoppingToken);
            
            await Console.Out.WriteLineAsync($"Kitchen will answer your order {order.OrderId} in {interactionWaitingTimeInMinutes.TotalMinutes} minutes");
            await Task.Delay(interactionWaitingTimeInMinutes, stoppingToken);
            
            //it may have expired at that stage, too late...
            if (DateTime.Now > order.ExpiresAt)
            {
                await Console.Out.WriteLineAsync($"Order {order.OrderId} has expired, not accepted/rejected within the given time {OrderExpiresTimeoutInMinutes} minutes.");
                isOrderAccepted = false;
                break;
            }
            await Console.Out.WriteLineAsync(response);
            switch (response)
            {
                case 'A':
                    await Console.Out.WriteLineAsync($"Order: {order.OrderId} accepted");
                    isOrderAccepted = true;
                    orderProcessed = true;
                    break;
                case 'R':
                    await Console.Out.WriteLineAsync($"Order: {order.OrderId} rejected");
                    isOrderAccepted = false;
                    orderProcessed = true;
                    break;
            }
        }

        if (isOrderAccepted)
        {
            await publishEndpoint.Publish<IKitchenOrderAccepted>(new
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                Items = order.Items,
                CreatedAt = order.CreatedAt
            }, stoppingToken);
        }
        else
        {
            await publishEndpoint.Publish<IKitchenOrderRejected>(new
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                Items = order.Items,
                CreatedAt = order.CreatedAt
            }, stoppingToken);
        }
    }

    private async Task MonitorOrdersAndActAsync(IPublishEndpoint publishEndpoint, 
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var order = processStackOrder.Dequeue();
            if (order is null)
                continue;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => { ProcessOrderAsync(publishEndpoint, order, stoppingToken); }, stoppingToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await Task.Delay(1000, stoppingToken);
        }
    }
    
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} is stopping.", ClassName);

        await base.StopAsync(stoppingToken);
    }
}

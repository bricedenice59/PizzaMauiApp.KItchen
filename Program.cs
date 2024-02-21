using System.Diagnostics;
using System.Reflection;
using MassTransit;
using PizzaMauiApp.API.Shared.Environment;
using PizzaMauiApp.Kitchen;

var builder = Host.CreateApplicationBuilder(args);

//decode configuration environment variables;
var rabbitMqConnectionConfig = new DbConnectionConfig(builder.Configuration, "RabbitMq");
if (rabbitMqConnectionConfig.Host is null || rabbitMqConnectionConfig.Port is null)
{
    Console.WriteLine("There was an issue reading configuration from dotnet user-secrets");
    throw new ArgumentException();
}
builder.Services.AddMassTransit(x =>
{
    var entryAssembly = Assembly.GetEntryAssembly();
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumers(entryAssembly);
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host($"rabbitmq://{rabbitMqConnectionConfig.Host}:{rabbitMqConnectionConfig.Port}", hostconfig =>
        {
            hostconfig.Username(rabbitMqConnectionConfig.Username);
            hostconfig.Password(rabbitMqConnectionConfig.Password);
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHostedService<ScopedBackgroundService>();
builder.Services.AddSingleton(typeof(IProcessStackOrder<>),typeof(ProcessStackOrder<>));

var host = builder.Build();
host.Run();
using System.Diagnostics;
using System.Reflection;
using MassTransit;
using PizzaMauiApp.API.Core.Environment;
using PizzaMauiApp.Kitchen;

var builder = Host.CreateApplicationBuilder(args);

//decode configuration environment variables;
var rabbitMqConnectionConfig = new DbConnectionConfig(builder.Configuration, "RabbitMq");
//check if secrets data are correctly read and binded to object
ArgumentException.ThrowIfNullOrEmpty(rabbitMqConnectionConfig.Host);
ArgumentException.ThrowIfNullOrEmpty(rabbitMqConnectionConfig.Port);
ArgumentException.ThrowIfNullOrEmpty(rabbitMqConnectionConfig.Username);
ArgumentException.ThrowIfNullOrEmpty(rabbitMqConnectionConfig.Password);

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
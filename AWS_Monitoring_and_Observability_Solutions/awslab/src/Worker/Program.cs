using System;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using worker;
using VotingData.Db;
using worker.Consumers;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;
using OpenTelemetry.Trace;

var builder = Host.CreateDefaultBuilder(args);
var defaultResource = ResourceBuilder.CreateDefault();

builder.ConfigureLogging((hostBuilderContext,logging) =>
{
    logging.ClearProviders();
    logging.AddConsole();
    //Add code to configurate ILogger for OpenTelemtry here
    logging.AddOpenTelemetry((options) =>
    {
        options.SetResourceBuilder(defaultResource);       
        options.AddOtlpExporter();
    });
});
builder.ConfigureServices((hostBuilderContext, services) =>
{    
    //add code block to register opentelemetry for metrics and traces
    
    var connectionString = hostBuilderContext.Configuration.GetConnectionString("SqlDbConnection");
    services.AddDbContext<VotingDBContext>(options =>options.UseNpgsql(connectionString));

    services.AddHostedService<Worker>();
    
    //RabittMQ over Masstransit
    services.AddMassTransit(x =>
    {
        x.AddConsumer<MessageConsumer>();
        x.UsingRabbitMq((context, cfg) =>
            {
                    cfg.Host(hostBuilderContext.Configuration.GetValue<string>("MassTransit:RabbitMq:Host"));
                    cfg.ConfigureEndpoints(context);
                });
    });
    
    //Add code block to register opentelemetr metric provider here
    services.AddOpenTelemetry()            
        .WithMetrics((providerBuilder) => providerBuilder.AddMeter("VotingMeter")
                                                    .SetResourceBuilder(defaultResource)
                                                    .AddAspNetCoreInstrumentation()
                                                    .AddConsoleExporter()
                                                    .AddOtlpExporter())
        .WithTracing((providerBuilder) => providerBuilder.SetResourceBuilder(defaultResource)
                                                    .AddSource("Npgsql")
                                                    .AddSource("MassTransit")
                                                    .AddXRayTraceId()
                                                    .AddAWSInstrumentation() //when perform service call to aws services        
                                                    .AddAspNetCoreInstrumentation()
                                                    .AddSqlClientInstrumentation(options => options.SetDbStatementForText = true)
                                                    .AddMassTransitInstrumentation()
                                                    .AddConsoleExporter()
                                                    .AddOtlpExporter()
        );
    //Add code block to register opentelemetr trace provider here
    
});

await builder.Build().RunAsync();

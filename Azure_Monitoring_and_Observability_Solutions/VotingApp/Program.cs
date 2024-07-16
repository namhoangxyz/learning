using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Microsoft.ApplicationInsights.Extensibility;
using Azure.Monitor.OpenTelemetry.Exporter;
using VotingApp.Interfaces;
using VotingApp.Clients;
using System.Net.Mime;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging.ApplicationInsights;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IVoteQueueClient>(s =>new VoteQueueClient(builder.Configuration["ConnectionStrings:sbConnectionString"], builder.Configuration["ConnectionStrings:queueName"]));

builder.Services.AddHttpClient<IVoteDataClient, VoteDataClient>(c =>
                                                                    {
                                                                        c.BaseAddress = new Uri(builder.Configuration["ConnectionStrings:VotingDataAPIBaseUri"]);
                                                                        c.DefaultRequestHeaders.Add(
                                                                            Microsoft.Net.Http.Headers.HeaderNames.Accept,
                                                                            MediaTypeNames.Application.Json);
                                                                    });

 builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "VotingApp", Version = "v1" });
                
            });
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddOpenTelemetryTracing(providerBuilder =>
                                            {
                                                providerBuilder
                                                    .AddConsoleExporter()
                                                    .AddSource("VotingApp")
                                                    .SetResourceBuilder(
                                                        ResourceBuilder.CreateDefault()
                                                            .AddService(serviceName: "VotingApp", serviceVersion: "1.0.0"))
                                                    .AddAspNetCoreInstrumentation()
                                                    .AddHttpClientInstrumentation()        
                                                    .AddAzureMonitorTraceExporter(o =>
                                                    {
                                                        o.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];            
                                                    });
                                            });    

//This method gets called by the runtime. Use this method to add services to the container.
//var appInsightsTelemetryConfiguration = TelemetryConfiguration.CreateDefault();
//appInsightsTelemetryConfiguration.InstrumentationKey = builder.Configuration["ApplicationInsights:InstrumentationKey"]; 

builder.WebHost.ConfigureLogging((context, logBuilder) =>
{
    if (context.HostingEnvironment.IsDevelopment())
    {
        logBuilder.AddDebug();
        logBuilder.AddConsole();
    }

    logBuilder.AddApplicationInsights(
            (config) => config.ConnectionString = context.Configuration["ApplicationInsights:ConnectionString"],
            (options) => { }
        );

    //Send LogLevel.Debug and higher from all categories to Application Insights
    logBuilder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Debug);
});
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHttpClient();

builder.Services.AddApplicationInsightsTelemetryProcessor<FilterOutFastDependencyCallProcessor>(); 
builder.Services.AddSingleton<ITelemetryInitializer, MyCustomTelemetryInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "app v1");    
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

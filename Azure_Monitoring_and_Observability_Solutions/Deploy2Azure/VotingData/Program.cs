// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

using Microsoft.EntityFrameworkCore;

using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using VotingData.Models;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var configuration =  new ConfigurationBuilder().AddJsonFile($"appsettings.json").Build();

var appInsightConnectionString = configuration.GetValue<string>("ApplicationInsights:ConnectionString");
var appInsightInstrumentationKey = configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
var dbConnectionString = configuration.GetConnectionString("SqlDbConnection");

builder.Services.AddDbContext<VotingDBContext>(options => options.UseSqlServer(dbConnectionString));


builder.WebHost.ConfigureLogging((hostingContext, logBuilder) =>
                    {
                        if (hostingContext.HostingEnvironment.IsDevelopment())
                        {
                            logBuilder.AddDebug();
                        }
                        logBuilder.AddOpenTelemetry(config=>{
                            config.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                .AddService(serviceName: "VotingData", serviceVersion: "1.0.0").AddTelemetrySdk());
                            config.AddAzureMonitorLogExporter(opt=>{
                                opt.ConnectionString = appInsightConnectionString;
                            });
                        });
                        logBuilder.AddApplicationInsights(instrumentationKey: appInsightInstrumentationKey);
                        logBuilder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);
                    });

builder.Services.AddOpenTelemetryTracing(providerBuilder => {providerBuilder
                                .AddSource("VotingApp")
                                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                                .AddService(serviceName: "VotingApp", serviceVersion: "1.0.0").AddTelemetrySdk()) 
                                .AddHttpClientInstrumentation()
                                .AddAspNetCoreInstrumentation()
                                .AddSqlClientInstrumentation(options => options.SetDbStatementForText = true)                                
                                .AddAzureMonitorTraceExporter(o =>
                                {
                                    o.ConnectionString = appInsightConnectionString;
                                });
            });

builder.Services.AddOpenTelemetryMetrics(providerBuilder =>
{
                                        providerBuilder
                                            .AddMeter("VotingMeter")
                                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: "VotingApp", serviceVersion: "1.0.0").AddTelemetrySdk())                             
                                            .AddAzureMonitorMetricExporter(o =>
                                            {
                                                o.ConnectionString = appInsightConnectionString;
                                            });
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();



var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI(c => {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "app v1");
                        c.RoutePrefix = string.Empty;
    });

app.UseEndpoints(builder =>
{
    builder.MapControllers();
});

app.Run();

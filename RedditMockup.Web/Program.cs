﻿using Microsoft.EntityFrameworkCore;
using RedditMockup.DataAccess.Context;
using RedditMockup.Service.Grpc;
using RedditMockup.Web;
using Serilog;
using Serilog.Settings.Configuration;

// TODO: Use logging across the app

// TODO: Use redis

var builder = WebApplication.CreateBuilder(args);


#region [macOS Configuration for gRPC over HTTP 2.0 Without TLS]

/*

builder.WebHost.ConfigureKestrel(options =>
{
    // Setup a HTTP/2 endpoint without TLS.

    options.ListenLocalhost(6000, o => o.Protocols =
    HttpProtocols.Http2);
});

*/

#endregion

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders();

//Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();


builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom
            .Configuration(context.Configuration));

try
{
    builder.Services
        .AddEndpointsApiExplorer()
        .InjectApi()
        .InjectSwagger()
        .InjectUnitOfWork()
        .InjectSieve()
        .InjectAuthentication()
        .InjectContext(builder.Configuration, builder.Environment)
        .InjectBusinesses()
        .InjectFluentValidation()
        .InjectRabbitMq()
        .InjectAutoMapper()
        .InjectGrpc()
        .InjectSerilog(builder.Configuration)
        .AddHealthChecks();

    //.InjectRedis(builder.Configuration)

    var app = builder.Build();

    await using var scope = app.Services.CreateAsyncScope();

    using var context = scope.ServiceProvider.GetRequiredService<RedditMockupContext>();

    app.UseSwagger()
        .UseSwaggerUI();

    if (app.Environment.IsEnvironment("Testing"))
    {
        await context.Database.EnsureDeletedAsync();
    }

    if (!app.Environment.IsProduction())
    {
        await context.Database.EnsureCreatedAsync();
    }

    else
    {
        await context.Database.MigrateAsync();
        //app.UseHsts();
    }

    app
        //.UseHttpsRedirection()
        .UseRouting()
        .UseAuthentication()
        .UseAuthorization()
        .UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/healthcheck");
            endpoints.MapGrpcService<GrpcService>();
            endpoints.MapGet("/protos/redditmockup.proto", async context =>
            {
                await context.Response.WriteAsync(File.ReadAllText("../RedditMockup.Model/Protos/redditmockup.proto"));
            });
        });

    await app.RunAsync();
}
catch (Exception exception)
{
   Log.Error(exception, "Program stopped due to a {ExceptionType} exception", exception.GetType());
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program
{
}
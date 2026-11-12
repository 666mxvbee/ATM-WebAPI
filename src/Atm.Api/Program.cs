using Atm.Application;
using Atm.Infrastructure.Persistence;
using Atm.Presentation;
using FluentMigrator.Runner;
using SessionOptions = Atm.Application.Options.SessionOptions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentationHttp();

builder.Services.AddSwaggerGen().AddEndpointsApiExplorer();

builder.Services
    .AddOptions<SessionOptions>()
    .Bind(builder.Configuration.GetRequiredSection("Session"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UsePresentationHttp();

await app.RunAsync();
using Atm.Application;
using Atm.Infrastructure.Persistence;
using Atm.Presentation;
using SessionOptions = Atm.Application.Options.SessionOptions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure()
    .AddPresentationHttp();

builder.Services.AddSwaggerGen().AddEndpointsApiExplorer();

builder.Services
    .AddOptions<SessionOptions>()
    .Bind(builder.Configuration.GetRequiredSection("Session"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UsePresentationHttp();

await app.RunAsync();
using Atm.Application;
using Atm.Application.Abstractions.Repositories;
using Atm.Infrastructure;
using Atm.Presentation.Common;
using ApplicationSessionOptions = Atm.Application.Sessions.SessionOptions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
    options.JsonSerializerOptions.AllowTrailingCommas = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddOptions<ApplicationSessionOptions>()
    .Bind(builder.Configuration.GetSection(ApplicationSessionOptions.SectionName))
    .Validate(options => options.UserLifetimeMinutes > 0, "User session lifetime must be positive.")
    .Validate(options => options.AdminLifetimeMinutes > 0, "Admin session lifetime must be positive.")
    .ValidateOnStart();

builder.Services.AddSingleton<ISystemPasswordProvider, SystemPasswordProvider>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

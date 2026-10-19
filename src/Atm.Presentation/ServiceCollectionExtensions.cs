using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.Presentation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationHttp(
        this IServiceCollection collection)
    {
        collection.AddControllers();
        return collection;
    }

    public static WebApplication UsePresentationHttp(
        this WebApplication application)
    {
        application.MapControllers();
        return application;
    }
}
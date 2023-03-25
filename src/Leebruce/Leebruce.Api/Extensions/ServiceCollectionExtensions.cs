namespace Leebruce.Api.Extensions;

public static class ServiceCollectionExtensions
{
	public static T CreateWithHttp<T>( this IServiceProvider services, string clientName )
	{
		var factory = services.GetRequiredService<IHttpClientFactory>();
		var client = factory.CreateClient( clientName );
		return ActivatorUtilities.CreateInstance<T>( services, client );
	}
	public static IServiceCollection AddScopedWithHttp<TService, TImplementation>( this IServiceCollection services, string clientName )
		where TService : class
		where TImplementation : class, TService
	{
		return services.AddScoped<TService>( s => s.CreateWithHttp<TImplementation>( clientName ) );
	}
}

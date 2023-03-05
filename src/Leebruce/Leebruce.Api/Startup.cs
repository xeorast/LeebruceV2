using Leebruce.Api.Auth;
using Leebruce.Api.OpenApi;
using Leebruce.Domain.Converters;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Leebruce.Api;

public static class Startup
{
	public static void ConfigureServices( WebApplicationBuilder builder )
	{
		TimeTypesConverters.Register();

		_ = builder.Services.AddControllers()
			.AddJsonOptions( o =>
			{
				o.JsonSerializerOptions.Converters.RegisterTimeTypesConverters();
			} );

		// sagger
		_ = builder.Services.AddEndpointsApiExplorer();
		_ = builder.Services.AddSwaggerGen( o =>
		{
			o.MapType<DateOnly>( () => new OpenApiSchema() { Type = "string", Format = "date", Example = new OpenApiString( "2020-03-11" ) } );
			o.MapType<TimeOnly>( () => new OpenApiSchema() { Type = "string", Format = "partial-time", Example = new OpenApiString( "16:20:00" ) } );

			o.AddSecurityToken();
			o.SchemaFilter<RecordValidationSchemaFilter>();
			o.SchemaFilter<NullableReferenceSchemaFilter>();
		} );

		// authentication
		_ = builder.Services.AddAuthentication( options =>
		{
			options.DefaultScheme
			= options.DefaultAuthenticateScheme
			= options.DefaultChallengeScheme
			= options.DefaultForbidScheme
			= AuthConstants.Token.Scheme;
		} ).AddToken();

		// services
		_ = builder.Services.AddScoped<ILbLoginService, LbLoginService>();
		_ = builder.Services.AddScoped<ILbLogoutService, LbLogoutService>();
		_ = builder.Services.AddScoped<ILbHelperService, LbHelperService>();
		_ = builder.Services.AddScoped<ITimetableService, TimetableService>();
		_ = builder.Services.AddScoped<IAnnouncementsService, AnnouncementsService>();
		_ = builder.Services.AddScoped<IMessagesService, MessagesService>();
		_ = builder.Services.AddScoped<IScheduleService, ScheduleService>();
		_ = builder.Services.AddScoped<IGradesService, GradesService>();
		_ = builder.Services.AddScoped<JsonService>();
		_ = builder.Services.AddScoped<ILiblinkService>( s => new LiblinkService(
				  s.GetRequiredService<IHttpClientFactory>()
				  .CreateClient( nameof( LiblinkService ) ) ) );

		_ = builder.Services.AddHttpClient( nameof( LiblinkService ) )
			.ConfigurePrimaryHttpMessageHandler( () =>
			{
				return new HttpClientHandler() { AllowAutoRedirect = false };
			} );

	}

	public static void Configure( WebApplication app )
	{
		if ( app.Environment.IsDevelopment() )
		{
			_ = app.UseSwagger();
			_ = app.UseSwaggerUI();
		}

		_ = app.UseHttpsRedirection();

		_ = app.UseAuthentication();
		_ = app.UseAuthorization();

		_ = app.MapControllers();
	}
}

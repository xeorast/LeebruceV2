using Leebruce.Api.Auth;
using Leebruce.Api.Extensions;
using Leebruce.Api.OpenApi;
using Leebruce.Domain.Converters;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Leebruce.Api;

public static class Startup
{
	const string httpClientForAuthedCalls = "httpClientForAuthenticatedCalls";
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
		_ = builder.Services.AddHttpContextAccessor();
		_ = builder.Services.AddScoped<ILbUserService, LbUserService>();
		_ = builder.Services.AddScoped<ILbLoginService, LbLoginService>();
		_ = builder.Services.AddScopedWithHttp<ILbLogoutService, LbLogoutService>( httpClientForAuthedCalls );
		_ = builder.Services.AddScoped<ILbHelperService, LbHelperService>();
		_ = builder.Services.AddScopedWithHttp<ITimetableService, TimetableService>( httpClientForAuthedCalls );
		_ = builder.Services.AddScopedWithHttp<IAnnouncementsService, AnnouncementsService>( httpClientForAuthedCalls );
		_ = builder.Services.AddScopedWithHttp<IMessagesService, MessagesService>( httpClientForAuthedCalls );
		_ = builder.Services.AddScopedWithHttp<IScheduleService, ScheduleService>( httpClientForAuthedCalls );
		_ = builder.Services.AddScopedWithHttp<IGradesService, GradesService>( httpClientForAuthedCalls );
		_ = builder.Services.AddScoped<JsonService>();
		_ = builder.Services.AddScopedWithHttp<ILiblinkService, LiblinkService>( nameof( LiblinkService ) );

		_ = builder.Services.AddHttpClient( nameof( LiblinkService ) )
			.ConfigurePrimaryHttpMessageHandler( () =>
			{
				return new HttpClientHandler() { AllowAutoRedirect = false };
			} );

		_ = builder.Services.AddHttpClient( httpClientForAuthedCalls )
			.ConfigurePrimaryHttpMessageHandler( () =>
			{
				return new HttpClientHandler() { AllowAutoRedirect = false, UseCookies = false };
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

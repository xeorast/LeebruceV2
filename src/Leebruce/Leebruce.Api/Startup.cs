using Leebruce.Api.Auth;
using Leebruce.Api.Extensions;
using Leebruce.Api.OpenApi;
using Leebruce.Api.Options;
using Leebruce.Domain.Converters;
using Microsoft.Extensions.Options;
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

		// options
		_ = builder.Services.Configure<LbConfigOptions>( builder.Configuration.GetRequiredSection( "LbConfig" ) );

		// services
		_ = builder.Services.AddHttpContextAccessor();
		_ = builder.Services.AddScoped<ILbUserService, LbUserService>();
		_ = builder.Services.AddScoped<ILbLoginService, LbLoginService>();
		_ = builder.Services.AddScoped<ILbLogoutService, LbLogoutService>();
		_ = builder.Services.AddScoped<ILbHelperService, LbHelperService>();
		_ = builder.Services.AddScoped<ITimetableService, TimetableService>();
		_ = builder.Services.AddScoped<IAnnouncementsService, AnnouncementsService>();
		_ = builder.Services.AddScoped<IMessagesService, MessagesService>();
		_ = builder.Services.AddScoped<IScheduleService, ScheduleService>();
		_ = builder.Services.AddScoped<IGradesService, GradesService>();
		_ = builder.Services.AddScoped<ILbMetaService, LbMetaService>();
		_ = builder.Services.AddScoped<JsonService>();
		_ = builder.Services.AddScopedWithHttp<ILiblinkService, LiblinkService>( nameof( LiblinkService ) );
		_ = builder.Services.AddScopedWithHttp<ILbSiteClient, LbSiteClient>( httpClientForAuthedCalls );

		_ = builder.Services.AddHttpClient( nameof( LiblinkService ) )
			.ConfigurePrimaryHttpMessageHandler( () =>
			{
				return new HttpClientHandler() { AllowAutoRedirect = false, UseCookies = false };
			} );

		_ = builder.Services.AddHttpClient( httpClientForAuthedCalls )
			.ConfigureHttpClient( ( s, client ) =>
			{
				var o = s.GetRequiredService<IOptions<LbConfigOptions>>().Value;
				client.BaseAddress = new( o.WebsiteUrl );
			} )
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

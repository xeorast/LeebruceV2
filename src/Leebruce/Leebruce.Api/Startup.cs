using Leebruce.Api.Auth;
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
			o.MapType<DateOnly>( () => new OpenApiSchema() { Title = "Date", Type = "string", Example = new OpenApiString( "2020-03-11" ) } );
			o.MapType<TimeOnly>( () => new OpenApiSchema() { Title = "Hour", Type = "string", Example = new OpenApiString( "16:20:00" ) } );

			o.AddSecurityToken();
			o.SchemaFilter<RecordValidationSchemaFilter>();
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
		_ = builder.Services.AddScoped<JsonService>();

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

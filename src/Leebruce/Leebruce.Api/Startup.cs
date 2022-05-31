using Leebruce.Api.Auth;

namespace Leebruce.Api;

public static class Startup
{
	public static void ConfigureServices( WebApplicationBuilder builder )
	{
		_ = builder.Services.AddControllers();
		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		_ = builder.Services.AddEndpointsApiExplorer();
		_ = builder.Services.AddSwaggerGen( o =>
		{
			o.AddSecurityToken();
			o.SchemaFilter<RecordValidationSchemaFilter>();
		} );

		_ = builder.Services.AddAuthentication( options =>
		{
			options.DefaultScheme
			= options.DefaultAuthenticateScheme
			= options.DefaultChallengeScheme
			= options.DefaultForbidScheme
			= AuthConstants.Token.Scheme;
		} ).AddToken();

		_ = builder.Services.AddScoped<ILbLoginService, LbLoginService>();
		_ = builder.Services.AddScoped<ILbLogoutService, LbLogoutService>();
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

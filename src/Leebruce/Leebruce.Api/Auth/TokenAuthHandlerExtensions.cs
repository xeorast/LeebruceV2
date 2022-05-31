using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Leebruce.Api.Auth;

public static class TokenAuthHandlerExtensions
{
	public static AuthenticationBuilder AddToken( this AuthenticationBuilder builder )
	{
		return builder.AddToken( _ => { } );
	}
	public static AuthenticationBuilder AddToken( this AuthenticationBuilder builder, Action<TokenOptions> configureOptions )
	{
		return builder.AddScheme<TokenOptions, TokenAuthHandler>( AuthConstants.Token.Scheme, configureOptions );
	}

	public static void AddSecurityToken( this SwaggerGenOptions o )
	{
		var bearerSecurityScheme = new OpenApiSecurityScheme
		{
			Scheme = AuthConstants.Token.Scheme,
			Type = SecuritySchemeType.Http,
			Description = "Authorization header. Example: 'Authorization: Bearer {token}'",

			Reference = new OpenApiReference
			{
				Id = "Bearer",
				Type = ReferenceType.SecurityScheme
			}
		};

		var cookieSecurityScheme = new OpenApiSecurityScheme
		{
			Name = AuthConstants.Token.CookieName,
			In = ParameterLocation.Cookie,
			Type = SecuritySchemeType.ApiKey,
			Description = "Authorization cookie.",

			Reference = new OpenApiReference
			{
				Id = "Cookie",
				Type = ReferenceType.SecurityScheme
			}
		};

		o.AddSecurityDefinition( "Bearer", bearerSecurityScheme );
		o.AddSecurityDefinition( "Cookie", cookieSecurityScheme );

		o.AddSecurityRequirement( new OpenApiSecurityRequirement()
		{
			[bearerSecurityScheme] = Array.Empty<string>()
		} );
		o.AddSecurityRequirement( new OpenApiSecurityRequirement()
		{
			[cookieSecurityScheme] = Array.Empty<string>()
		} );

	}

}
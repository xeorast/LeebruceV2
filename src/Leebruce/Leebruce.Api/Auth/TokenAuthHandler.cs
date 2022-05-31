using Leebruce.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Leebruce.Api.Auth;

public class TokenAuthHandler : AuthenticationHandler<TokenOptions>
{
	private readonly JsonService _json;
	private readonly ProblemDetailsFactory _detailsFactory;
	private readonly IActionResultExecutor<ObjectResult> _executor;

	public TokenAuthHandler( IOptionsMonitor<TokenOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, JsonService json, ProblemDetailsFactory detailsFactory, IActionResultExecutor<ObjectResult> executor )
		: base( options, logger, encoder, clock )
	{
		_json = json;
		_detailsFactory = detailsFactory;
		_executor = executor;
	}

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		await Task.CompletedTask;

		if ( Scheme.Name != "Bearer" )
		{
			throw new InvalidOperationException( $"{nameof( TokenAuthHandler )} can only use Bearer scheme." );
		}

		// check if token is present
		var token = GetTokenFromHeader() ?? GetTokenFromCookie();
		if ( string.IsNullOrEmpty( token ) )
		{
			return AuthenticateResult.NoResult();
		}

		// check if token is valid
		if ( !_json.TryFromBase64Json<LbAuthData>( token, out var data ) )
		{
			return AuthenticateResult.Fail( "Token is invalid." );
		}
		if ( data is not { DziennikSid: not null, SdziennikSid: not null, OAuthToken: not null } )
		{
			return AuthenticateResult.Fail( "Token is incomplete." );
		}

		// generate claims
		Claim[] claims = new Claim[]
		{
			new( nameof( data.DziennikSid ), data.DziennikSid ),
			new( nameof( data.SdziennikSid ), data.SdziennikSid ),
			new( nameof( data.OAuthToken ), data.OAuthToken ),
		};

		ClaimsPrincipal principal = new( new ClaimsIdentity( claims, Scheme.Name ) );
		TokenValidatedContext ctx = new( Context, Scheme, Options ) { Principal = principal };
		ctx.Success();

		ctx.Properties.StoreTokens( new[]
		{
			new AuthenticationToken(){ Name = AuthConstants.Token.TokenName, Value = token }
		} );

		return ctx.Result;
	}

	private string? GetTokenFromHeader()
	{
		string authorization = Request.Headers.Authorization;

		if ( string.IsNullOrEmpty( authorization )
			|| !authorization.StartsWith( "Bearer ", StringComparison.OrdinalIgnoreCase ) )
		{
			return null;
		}

		return authorization["Bearer ".Length..].Trim();
	}
	private string? GetTokenFromCookie()
	{
		return Request.Cookies[AuthConstants.Token.CookieName];
	}

	protected override async Task HandleChallengeAsync( AuthenticationProperties properties )
	{
		AuthenticateResult authResult = await HandleAuthenticateOnceSafeAsync();

		Response.StatusCode = 401;

		// https://datatracker.ietf.org/doc/html/rfc6750#section-3.1
		StringBuilder wwwauth = new( $"Bearer realm=\"LeebruceDefault\"" );

		if ( authResult.None || authResult.Failure is null )
		{
			Response.Headers.WWWAuthenticate = wwwauth.ToString();
			return;
		}

		string? detail = authResult.Failure.Message;
		if ( detail is not null )
		{
			_ = wwwauth.Append( ", error=\"invalid_token\", "
					   + $"error_description=\"{detail}\"" );
		}

		Response.Headers.WWWAuthenticate = wwwauth.ToString();

		await FillResponse( Response.StatusCode, detail );

	}
	protected override async Task HandleForbiddenAsync( AuthenticationProperties properties )
	{
		Response.StatusCode = 403;

		// https://datatracker.ietf.org/doc/html/rfc6750#section-3.1
		Response.Headers.Append( HeaderNames.WWWAuthenticate, $"Bearer realm=\"LeebruceDefault\", error=\"insufficient_scope\"" );

		await FillResponse( Response.StatusCode, "Insufficient scope" );
	}

	private async Task FillResponse( int code, string? detail )
	{
		ActionContext actionContext = new( Context, Context.GetRouteData(), new ActionDescriptor() );

		ProblemDetails problemDetails = _detailsFactory.CreateProblemDetails( Context, code, detail: detail );
		ObjectResult result = new( problemDetails )
		{
			StatusCode = problemDetails.Status
		};

		await _executor.ExecuteAsync( actionContext, result );
	}

}

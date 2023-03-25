using Leebruce.Api.Models;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbHelperService
{
	HttpClientHandler CreateHandler( ClaimsPrincipal user );
	Task<string> GetUserNameAsync( ClaimsPrincipal user );
	public bool IsUnauthorized( string document );
}

public partial class LbHelperService : ILbHelperService
{
	//todo: automatically create one httpclient per call
	//private readonly IHttpContextAccessor _httpContext;
	//public HttpClient Http { get; }

	public HttpClientHandler CreateHandler( ClaimsPrincipal user )
	{
		return CreateHandler( LbAuthData.FromClaims( user.Claims ) );
	}
	public HttpClientHandler CreateHandler( LbAuthData parameters )
	{
		CookieContainer cookies = new();
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.dsidName, parameters.DziennikSid ) );
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.sdsidName, parameters.SdziennikSid ) );
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.oatokenName, parameters.OAuthToken ) );

		return new() { AllowAutoRedirect = false, CookieContainer = cookies };
	}

	public bool IsUnauthorized( string document )
	{
		return document.Contains( "<h2 class=\"inside\">Brak dostępu</h2>" )
			&& document.Contains( @"https:\/\/synergia.librus.pl\/loguj" );
	}

	public async Task<string> GetUserNameAsync( ClaimsPrincipal user )
	{
		using HttpClientHandler handler = CreateHandler( user );
		using HttpClient http = new( handler );

		using var resp = await http.GetAsync( "https://synergia.librus.pl/uczen/index" );
		var ctnt = await resp.Content.ReadAsStringAsync();
		if ( ctnt.Contains( "Brak dostępu" ) )
		{
			throw new NotAuthorizedException( "User not logged in." );
		}

		var match = userNameRx().Match( ctnt );
		if ( !match.Success )
		{
			throw new Exception( "Cannot get username." );
		}

		return match.Groups[1].Value;
	}
	[GeneratedRegex(  @"<div id=""user-section""[\s\S]*?jesteś zalogowany jako: <b>[\W\s]*([\w\s-.]*)"  )]
	private static partial Regex userNameRx();

}

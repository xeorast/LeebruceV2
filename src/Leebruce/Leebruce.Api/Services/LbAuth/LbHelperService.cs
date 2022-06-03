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

public class LbHelperService : ILbHelperService
{
	//todo: automatically create one httpclient per call
	//private readonly IHttpContextAccessor _httpContext;
	//public HttpClient Http { get; }

	public HttpClientHandler CreateHandler( ClaimsPrincipal user )
	{
		string? dziennikSid = user.Claims.FirstOrDefault( x => x.Type == "DziennikSid" )?.Value
			?? throw new ArgumentException( "Incomplete principal, no 'DziennikSid' claim found.", nameof( user ) );

		string? sdziennikSid = user.Claims.FirstOrDefault( x => x.Type == "SdziennikSid" )?.Value
			?? throw new ArgumentException( "Incomplete principal, no 'SdziennikSid' claim found.", nameof( user ) );

		string? oAuthToken = user.Claims.FirstOrDefault( x => x.Type == "OAuthToken" )?.Value
			?? throw new ArgumentException( "Incomplete principal, no 'OAuthToken' claim found.", nameof( user ) );

		return CreateHandler( dziennikSid, sdziennikSid, oAuthToken );
	}
	public HttpClientHandler CreateHandler( string dziennikSid, string sdziennikSid, string oAuthToken )
	{
		CookieContainer cookies = new();
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.dsidName, dziennikSid ) );
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.sdsidName, sdziennikSid ) );
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.oatokenName, oAuthToken ) );

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

		var match = userNameRx.Match( ctnt );
		if ( !match.Success )
		{
			throw new Exception( "Cannot get username." );
		}

		return match.Groups[1].Value;
	}
	static readonly Regex userNameRx = new( @"<div id=""user-section""[\s\S]*?jesteś zalogowany jako: <b>[\W\s]*([\w\s-.]*)" );

}

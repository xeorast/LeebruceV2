using Leebruce.Api.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbHelperService
{
	HttpClientHandler CreateHandler( string token );
	Task<string> GetUserNameAsync( string token );
}

public class LbHelperService : ILbHelperService
{
	private readonly JsonService _json;
	//private readonly IHttpContextAccessor _httpContext;
	//public HttpClient Http { get; }

	public LbHelperService( JsonService json )
	{
		_json = json;
	}

	public HttpClientHandler CreateHandler( string token )
	{
		var data = _json.FromBase64Json<LbAuthData>( token ) ?? throw new Exception();

		CookieContainer cookies = new();
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.dsidName, data.DziennikSid ) );
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.sdsidName, data.SdziennikSid ) );

		return new() { AllowAutoRedirect = false, CookieContainer = cookies };
	}

	public async Task<string> GetUserNameAsync( string token )
	{
		using HttpClientHandler handler = CreateHandler( token );
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

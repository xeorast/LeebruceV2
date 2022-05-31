using Leebruce.Api.Models;
using System.Net;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbLogoutService
{
	Task LogoutAsync( string token );
}

public class LbLogoutService : ILbLogoutService
{
	private readonly JsonService _json;

	public LbLogoutService( JsonService json )
	{
		_json = json;
	}

	public async Task LogoutAsync( string token )
	{
		LbAuthData data;
		try
		{
			data = _json.FromBase64Json<LbAuthData>( token ) ?? throw new ArgumentException( "Cannot process given token.", nameof( token ) );
		}
		catch ( FormatException )
		{
			throw new FormatException( "Token is invalid." );
		}
		if ( data is not { DziennikSid: not null, SdziennikSid: not null } )
		{
			throw new ArgumentException( "Token is incomplete." );
		}

		CookieContainer cookies = new();
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.dsidName, data.DziennikSid ) );
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.sdsidName, data.SdziennikSid ) );

		using HttpClientHandler handler = new() { AllowAutoRedirect = false, CookieContainer = cookies };
		using HttpClient http = new( handler );

		using var resp = await http.GetAsync( "https://synergia.librus.pl/wyloguj" );
	}

}

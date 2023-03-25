using Leebruce.Api.Extensions;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbHelperService
{
	Task<string> GetUserNameAsync( ClaimsPrincipal user );
	public bool IsUnauthorized( string document );
}

public partial class LbHelperService : ILbHelperService
{
	private readonly ILbUserService _lbUser;
	private readonly HttpClient _http;

	public LbHelperService( ILbUserService lbUser, HttpClient http )
	{
		_lbUser = lbUser;
		_http = http;
	}

	public bool IsUnauthorized( string document )
	{
		return document.Contains( "<h2 class=\"inside\">Brak dostępu</h2>" )
			&& document.Contains( @"https:\/\/synergia.librus.pl\/loguj" );
	}

	public async Task<string> GetUserNameAsync( ClaimsPrincipal user )
	{
		using var resp = await _http.GetWithCookiesAsync( "https://synergia.librus.pl/uczen/index", _lbUser.UserCookieHeader );
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
	[GeneratedRegex( @"<div id=""user-section""[\s\S]*?jesteś zalogowany jako: <b>[\W\s]*([\w\s-.]*)" )]
	private static partial Regex userNameRx();

}

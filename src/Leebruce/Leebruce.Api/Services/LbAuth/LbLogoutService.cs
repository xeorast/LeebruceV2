using Leebruce.Api.Extensions;
using System.Security.Claims;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbLogoutService
{
	Task LogoutAsync( ClaimsPrincipal user );
}

public class LbLogoutService : ILbLogoutService
{
	private readonly ILbUserService _lbUser;
	private readonly HttpClient _http;

	public LbLogoutService( ILbUserService lbUser, HttpClient http )
	{
		_lbUser = lbUser;
		_http = http;
	}

	public async Task LogoutAsync( ClaimsPrincipal user )
	{
		using var resp = await _http.GetWithCookiesAsync( "https://synergia.librus.pl/wyloguj", _lbUser.UserCookieHeader );
	}

}

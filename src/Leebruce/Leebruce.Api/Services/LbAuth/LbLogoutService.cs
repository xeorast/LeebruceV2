using System.Security.Claims;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbLogoutService
{
	Task LogoutAsync( ClaimsPrincipal user );
}

public class LbLogoutService : ILbLogoutService
{
	private readonly ILbSiteClient _lbClient;

	public LbLogoutService( ILbSiteClient lbClient )
	{
		_lbClient = lbClient;
	}

	public async Task LogoutAsync( ClaimsPrincipal user )
	{
		using var resp = await _lbClient.GetAuthorized( "https://synergia.librus.pl/wyloguj" );
	}

}

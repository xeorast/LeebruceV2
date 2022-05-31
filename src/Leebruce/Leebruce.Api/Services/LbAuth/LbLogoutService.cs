using System.Security.Claims;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbLogoutService
{
	Task LogoutAsync( ClaimsPrincipal user );
}

public class LbLogoutService : ILbLogoutService
{
	private readonly ILbHelperService _lbHelper;

	public LbLogoutService( ILbHelperService lbHelper )
	{
		_lbHelper = lbHelper;
	}

	public async Task LogoutAsync( ClaimsPrincipal user )
	{
		using HttpClientHandler handler = _lbHelper.CreateHandler( user );
		using HttpClient http = new( handler );

		using var resp = await http.GetAsync( "https://synergia.librus.pl/wyloguj" );
	}

}

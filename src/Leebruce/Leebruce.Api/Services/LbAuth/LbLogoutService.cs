namespace Leebruce.Api.Services.LbAuth;

public interface ILbLogoutService
{
	Task LogoutAsync();
}

public class LbLogoutService : ILbLogoutService
{
	private readonly ILbSiteClient _lbClient;

	public LbLogoutService( ILbSiteClient lbClient )
	{
		_lbClient = lbClient;
	}

	public async Task LogoutAsync()
	{
		using var resp = await _lbClient.GetAuthorized( "/wyloguj" );
	}

}

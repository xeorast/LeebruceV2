using Leebruce.Domain;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbMetaService
{
	Task<UpdatesSinceLoginModel> GetNotifications();
	Task<string> GetUsername();
}

public partial class LbMetaService : ILbMetaService
{
	private readonly ILbHelperService _lbHelper;
	private readonly ILbSiteClient _lbClient;

	public LbMetaService( ILbHelperService lbHelper, ILbSiteClient lbClient )
	{
		_lbHelper = lbHelper;
		_lbClient = lbClient;
	}

	public async Task<UpdatesSinceLoginModel> GetNotifications()
	{
		var doc = await _lbClient.GetContentAuthorized( "https://synergia.librus.pl/uczen/index" );
		return _lbHelper.GetNotifications( doc );
	}

	public async Task<string> GetUsername()
	{
		var doc = await _lbClient.GetContentAuthorized( "https://synergia.librus.pl/uczen/index" );
		return _lbHelper.GetUserName( doc );
	}
}

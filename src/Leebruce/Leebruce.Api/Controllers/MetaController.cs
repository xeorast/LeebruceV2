using Leebruce.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leebruce.Api.Controllers;

[Authorize]
[ApiController]
[Route( "api/[controller]" )]
public class MetaController : ExtendedControllerBase
{
	private readonly ILbMetaService _metaService;

	public MetaController( ILbMetaService metaService )
	{
		_metaService = metaService;
	}

	[HttpGet( "updates-since-last-login" )]
	public async Task<ActionResult<UpdatesSinceLoginModel>> GetUpdates()
	{
		try
		{
			return await _metaService.GetNotifications();
		}
		catch ( NotAuthorizedException )
		{
			return Unauthorized( "Session has expired." );
		}
	}

}

using Leebruce.Api.Extensions;
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
		return await _metaService.GetNotifications()
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

	[HttpGet( "username" )]
	public async Task<ActionResult<string>> GetUsername()
	{
		return await _metaService.GetUsername()
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

}

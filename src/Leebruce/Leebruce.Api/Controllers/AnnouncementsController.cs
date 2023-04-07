using Leebruce.Api.Extensions;
using Leebruce.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leebruce.Api.Controllers;

[Authorize]
[ApiController]
[Route( "api/[controller]" )]
public class AnnouncementsController : ExtendedControllerBase
{
	private readonly IAnnouncementsService _annService;

	public AnnouncementsController( IAnnouncementsService annService )
	{
		_annService = annService;
	}

	[HttpGet]
	public async Task<ActionResult<AnnouncementModel[]>> GetAnnouncements()
	{
		return await _annService.GetAnnouncementsAsync()
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}
}

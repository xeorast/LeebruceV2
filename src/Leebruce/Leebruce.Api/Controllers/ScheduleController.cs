using Leebruce.Api.Extensions;
using Leebruce.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leebruce.Api.Controllers;

[Authorize]
[ApiController]
[Route( "api/[controller]" )]
public class ScheduleController : ExtendedControllerBase
{
	private readonly IScheduleService _ScheduleService;

	public ScheduleController( IScheduleService ScheduleService )
	{
		_ScheduleService = ScheduleService;
	}

	[HttpGet]
	public async Task<ActionResult<ScheduleDay[]>> GetSchedule()
	{
		return await _ScheduleService.GetScheduleAsync( User )
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

	[HttpGet( "{date}" )]
	public async Task<ActionResult<ScheduleDay[]>> GetSchedule( DateOnly date )
	{
		return await _ScheduleService.GetScheduleAsync( User, date )
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}


}

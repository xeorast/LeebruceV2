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
		try
		{
			return await _ScheduleService.GetScheduleAsync( User );
		}
		catch ( NotAuthorizedException )
		{
			return Unauthorized( "Session has expired." );
		}
	}

	[HttpGet( "{date}" )]
	public async Task<ActionResult<ScheduleDay[]>> GetSchedule( DateOnly date )
	{
		try
		{
			return await _ScheduleService.GetScheduleAsync( User, date );
		}
		catch ( NotAuthorizedException )
		{
			return Unauthorized( "Session has expired." );
		}
	}


}

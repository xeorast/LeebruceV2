using Leebruce.Api.Extensions;
using Leebruce.Domain.Timetable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leebruce.Api.Controllers;

[Authorize]
[ApiController]
[Route( "api/[controller]" )]
public class TimetableController : ExtendedControllerBase
{
	private readonly ITimetableService _timetableService;

	public TimetableController( ITimetableService timetableService )
	{
		_timetableService = timetableService;
	}

	[HttpGet]
	public async Task<ActionResult<TimetableDayModel[]>> GetCurrentWeek()
	{
		return await _timetableService.GetTimetableAsync()
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

	[HttpGet( "{date}" )]
	public async Task<ActionResult<TimetableDayModel[]>> GetWeek( DateOnly date )
	{
		return await _timetableService.GetTimetableAsync( date )
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

}

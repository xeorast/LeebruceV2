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
		try
		{
			return await _timetableService.GetTimetableAsync( User );
		}
		catch ( NotAuthorizedException )
		{
			return Unauthorized( "Session has expired." );
		}
	}

	[HttpGet( "{date}" )]
	public async Task<ActionResult<TimetableDayModel[]>> GetWeek( DateOnly date )
	{
		try
		{
			return await _timetableService.GetTimetableAsync( User, date );
		}
		catch ( NotAuthorizedException )
		{
			return Unauthorized( "Session has expired." );
		}
	}

}

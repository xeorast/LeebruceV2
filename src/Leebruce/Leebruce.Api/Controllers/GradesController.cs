using Leebruce.Domain;
using Leebruce.Domain.Grades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leebruce.Api.Controllers;

[Authorize]
[ApiController]
[Route( "api/[controller]" )]
public class GradesController : ExtendedControllerBase
{
	private readonly IGradesService _gradesService;

	public GradesController( IGradesService gradesService )
	{
		_gradesService = gradesService;
	}


	[HttpGet]
	public async Task<ActionResult<SubjectGradesModel[]>> GetGrades()
	{
		try
		{
			return await _gradesService.GetGradesAsync( User );
		}
		catch ( NotAuthorizedException )
		{
			return Unauthorized( "Session has expired." );
		}
	}

	[HttpGet( "graph" )]
	public async Task<ActionResult<GradesGraphRecordModel[]>> GetGraph()
	{
		try
		{
			return await _gradesService.GetGraphAsync( User );
		}
		catch ( NotAuthorizedException )
		{
			return Unauthorized( "Session has expired." );
		}
	}

}

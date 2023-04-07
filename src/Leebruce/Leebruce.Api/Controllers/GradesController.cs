using Leebruce.Api.Extensions;
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
	public async Task<ActionResult<GradesPageModel>> GetGrades()
	{
		return await _gradesService.GetGradesAsync()
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

	[HttpGet( "new" )]
	public async Task<ActionResult<GradesPageModel>> GetNewGrades()
	{
		return await _gradesService.GetNewGradesAsync()
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

	[HttpGet( "this-week" )]
	public async Task<ActionResult<GradesPageModel>> GetGradesWeekSummary()
	{
		return await _gradesService.GetGradesWeekSummaryAsync()
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

	[HttpGet( "graph" )]
	public async Task<ActionResult<GradesGraphRecordModel[]>> GetGraph()
	{
		return await _gradesService.GetGraphAsync()
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

}

using Leebruce.Api.Extensions;
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
	public async Task<ActionResult<GradesPageModel>> GetGrades()
	{
		return await _gradesService.GetGradesAsync( User )
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

	[HttpGet( "graph" )]
	public async Task<ActionResult<GradesGraphRecordModel[]>> GetGraph()
	{
		return await _gradesService.GetGraphAsync( User )
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

}

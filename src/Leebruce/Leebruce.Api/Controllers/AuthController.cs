using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Leebruce.Api.Controllers;

[ApiController]
[Route( "api/[controller]" )]
public class AuthController : ExtendedControllerBase
{
	[HttpPost( "login" )]
	public async Task<ActionResult<string>> Login( [FromBody] LoginDto dto, [FromServices] ILbLoginService loginService )
	{
		try
		{
			return await loginService.LoginAsync( dto.Username, dto.Password );
		}
		catch ( NotAuthorizedException e )
		{
			return Unauthorized( e.Message );
		}
		catch ( LbLoginException e )
		{
			return InternalServerError( e.Message );
		}
	}

	[Authorize]
	[HttpPost( "logout" )]
	public async Task<IActionResult> Logout( [FromServices] ILbLogoutService logoutService )
	{
		try
		{
			await logoutService.LogoutAsync( User );
			return NoContent();
		}
		catch ( ArgumentException e )
		{
			return BadRequest( e.Message );
		}
		catch ( FormatException e )
		{
			return BadRequest( e.Message );
		}
		catch ( LbLoginException e )
		{
			return InternalServerError( e.Message );
		}
	}

	public record LoginDto(
		[Required] string Username,
		[Required] string Password );

}

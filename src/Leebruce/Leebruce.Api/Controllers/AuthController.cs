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

	[HttpPost( "logout" )]
	public async Task<IActionResult> Logout( [FromBody] string token, [FromServices] ILbLogoutService logoutService )
	{
		try
		{
			await logoutService.LogoutAsync( token );
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

	public class LoginDto
	{
		[Required]
		public string Username { get; init; }
		[Required]
		public string Password { get; init; }

		public LoginDto(
			string Username,
			string Password )
		{
			this.Username = Username;
			this.Password = Password;
		}
	}

}

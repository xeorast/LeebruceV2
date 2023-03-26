using Leebruce.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leebruce.Api.Controllers;

[Authorize]
[ApiController]
[Route( "api/[controller]" )]
public class MessagesController : ExtendedControllerBase
{
	private readonly IMessagesService _messagesService;

	public MessagesController( IMessagesService messagesService )
	{
		_messagesService = messagesService;
	}

	[HttpGet]
	public async Task<ActionResult<MessageMetadataModel[]>> GetMessages( [FromQuery] int page = 1 )
	{
		try
		{
			return await _messagesService.GetMessagesAsync( User, page - 1 /*page is 1-based for users, but 0-based internally*/ );
		}
		catch ( NotAuthorizedException )
		{
			return Unauthorized( "Session has expired." );
		}
	}

	[HttpGet( "{id}" )]
	public async Task<ActionResult<MessageModel>> GetMessages( string id )
	{
		try
		{
			return await _messagesService.GetMessageAsync( User, id );
		}
		catch ( NotAuthorizedException )
		{
			return Unauthorized( "Session has expired." );
		}
	}

	[HttpGet( "attachments/{id}" )]
	public async Task<IActionResult> GetAttachment( string id )
	{
		try
		{
			var file = await _messagesService.GetAttachmentAsync( User, id );

			return File( file.Content, file.MediaType, file.FileName );
		}
		catch ( NotAuthorizedException )
		{
			return Unauthorized( "Session has expired." );
		}
	}

}

using Leebruce.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leebruce.Api.Controllers;

[Authorize]
[ApiController]
[Route( "api/[controller]" )]
public class MessagesController : ControllerBase
{
	private readonly IMessagesService _messagesService;

	public MessagesController( IMessagesService messagesService )
	{
		_messagesService = messagesService;
	}

	[HttpGet]
	public async Task<ActionResult<MessageMetadataModel[]>> GetMessages( [FromQuery] int page = 1 )
	{
		return await _messagesService.GetMessagesAsync( User, page - 1 /*page is 1-based for users, but 0-based internally*/ );
	}

	[HttpGet( "{id}" )]
	public async Task<ActionResult<MessageModel>> GetMessages( string id )
	{
		return await _messagesService.GetMessageAsync( User, id );
	}

	[HttpGet( "attachments/{id}" )]
	public async Task<IActionResult> GetAttachment( string id )
	{
		var file = await _messagesService.GetAttachmentAsync( User, id );

		return File( file.Content, file.MediaType, file.FileName );
	}

}

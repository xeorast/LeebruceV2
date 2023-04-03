using Leebruce.Api.Extensions;
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
	public async Task<ActionResult<CollectionPage<MessageMetadataModel>>> GetMessages( [FromQuery] int page = 1 )
	{
		return await _messagesService.GetMessagesAsync( User, page - 1 /*page is 1-based for users, but 0-based internally*/ )
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

	[HttpGet( "{id}" )]
	public async Task<ActionResult<MessageModel>> GetMessages( string id )
	{
		return await _messagesService.GetMessageAsync( User, id )
			.WithMappedMaintenanceBreak( ServiceUnavailable )
			.WithMappedUnauthorized( Unauthorized );
	}

	[HttpGet( "attachments/{id}" )]
	public async Task<IActionResult> GetAttachment( string id )
	{
		try
		{
			var file = await _messagesService.GetAttachmentAsync( User, id );

			return File( file.Content, file.MediaType, file.FileName );
		}
		catch ( MaintenanceBreakException )
		{
			return ServiceUnavailable( "Can not contact Lb as it is undergoing maintenance." );
		}
		catch ( NotAuthorizedException )
		{
			return Unauthorized( "Session has expired." );
		}
	}

}

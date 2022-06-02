using Leebruce.Api.Extensions;
using Leebruce.Domain.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services;

public interface IMessagesService
{
	Task<MessageMetadataModel[]> GetMessagesAsync( ClaimsPrincipal principal );
	Task<MessageMetadataModel[]> GetMessagesAsync( ClaimsPrincipal principal, int page );
	Task<MessageModel> GetMessageAsync( ClaimsPrincipal principal, string id );
}

public class MessagesService : IMessagesService
{
	private readonly ILbHelperService _lbHelper;

	public MessagesService( ILbHelperService lbHelper )
	{
		_lbHelper = lbHelper;
	}

	public async Task<MessageMetadataModel[]> GetMessagesAsync( ClaimsPrincipal principal )
	{
		using HttpClientHandler handler = _lbHelper.CreateHandler( principal );
		using HttpClient http = new( handler );

		using var resp = await http.GetAsync( "https://synergia.librus.pl/wiadomosci" );
		string document = await resp.Content.ReadAsStringAsync();

		string table = ExtractListTable( document );

		return ExtractMessages( table ).ToArray();
	}
	public async Task<MessageMetadataModel[]> GetMessagesAsync( ClaimsPrincipal principal, int page )
	{
		using HttpClientHandler handler = _lbHelper.CreateHandler( principal );
		using HttpClient http = new( handler );

		using StringContent pageCtnt = new( page.ToString() );
		using StringContent idPojemnikaCtnt = new( "105" );
		using MultipartFormDataContent form = new()
		{
			{ pageCtnt, "numer_strony105" },
			{ idPojemnikaCtnt, "idPojemnika" },
		};
		using var resp = await http.PostAsync( "https://synergia.librus.pl/wiadomosci", form );
		string document = await resp.Content.ReadAsStringAsync();

		string table = ExtractListTable( document );

		return ExtractMessages( table ).ToArray();
	}

	static string ExtractListTable( string document )
	{
		var tableMatch = messagesListTableRx.Match( document );
		var table = tableMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract table from document." );

		var bodyMatch = messagesListTableBodyRx.Match( table );
		return bodyMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract body from table." ); ;
	}
	static readonly Regex messagesListTableRx = new( @"<table class=""decorated stretch""[\s\S]*?>([\s\S]*?)<\/table>" );
	static readonly Regex messagesListTableBodyRx = new( @"<tbody[\s\S]*?>([\s\S]*?)<\/tbody>" );

	static IEnumerable<MessageMetadataModel> ExtractMessages( string tableBody )
	{
		var matches = messagesTableRowRx.Matches( tableBody );
		foreach ( Match match in matches )
		{
			var dateStr = match.GetGroup( "date" )
				?? throw new ProcessingException( "Failed to extract date from table." );

			if ( !DateTime.TryParse( dateStr, out var date ) )
				throw new ProcessingException( "Date extracted from table was invalid." );

			var offset = TimeZoneInfo.FindSystemTimeZoneById( "Central European Standard Time" ).GetUtcOffset( date );
			DateTimeOffset dateOffset = new( date, offset );

			var link = match.GetGroup( "link" )
				?? throw new ProcessingException( "Failed to extract link from table." );

			var author = match.GetGroup( "author" )
				?? throw new ProcessingException( "Failed to extract author from table." );

			var subject = match.GetGroup( "subject" )
				?? throw new ProcessingException( "Failed to extract subject from table." );

			author = author.DecodeHtml();
			subject = subject.DecodeHtml();
			var id = link.ToUrlBase64();

			yield return new MessageMetadataModel( subject, author, dateOffset, id );
		}
	}
	// $1: link, $2: author, $3: subject, $4: date
	static readonly Regex messagesTableRowRx = new( @"<tr[^>]*>(?:\s*<td[^>]*>[\s\S]*?<\/td>\s*<td[^>]*>[\s\S]*?<\/td>\s*)<td\s*>\s*<a href=""\/wiadomosci\/(?<link>[^""]*)"">[\w\s.-]*?\((?<author>[\w\s.-]*?)\)\s*<\/a>\s*<\/td>\s*<td\s*>\s*<a href=""\/wiadomosci\/\1"">\s*(?<subject>[\s\S]*?)\s*<\/a>\s*<\/td>\s*<td[^>]*>\s*(?<date>[\d\s-:]*)\s*<\/td>" );


	public async Task<MessageModel> GetMessageAsync( ClaimsPrincipal principal, string id )
	{
		using HttpClientHandler handler = _lbHelper.CreateHandler( principal );
		using HttpClient http = new( handler );

		var link = StringExtensions.FromUrlBase64( id );

		using var resp = await http.GetAsync( $"https://synergia.librus.pl/wiadomosci/{link}" );
		string document = await resp.Content.ReadAsStringAsync();

		return ExtractMessage( document );
	}

	static MessageModel ExtractMessage( string document )
	{
		var docMatch = messageDocBodyRx.Match( document );
		var body = docMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract body from document." );

		var dateMatch = messageDateRx.Match( body );
		var dateStr = dateMatch.GetGroup( "date" )
			?? throw new ProcessingException( "Failed to extract date from document." );

		if ( !DateTime.TryParse( dateStr, out var date ) )
			throw new ProcessingException( "Date extracted from table was invalid." );

		var offset = TimeZoneInfo.FindSystemTimeZoneById( "Central European Standard Time" ).GetUtcOffset( date );
		DateTimeOffset dateOffset = new( date, offset );

		var authorMatch = messageAuthorRx.Match( body );
		var author = authorMatch.GetGroup( "author" )
			?? throw new ProcessingException( "Failed to extract author from document." );

		var authorClass = authorMatch.GetGroup( "authorClass" )
			?? throw new ProcessingException( "Failed to extract author from document." );

		var subjectMatch = messageSubjectRx.Match( body );
		var subject = subjectMatch.GetGroup( "subject" )
			?? throw new ProcessingException( "Failed to extract subject from document." );

		var contentMatch = messageContentRx.Match( body );
		var content = contentMatch.GetGroup( "content" )
			?? throw new ProcessingException( "Failed to extract content from document." );

		subject = subject.DecodeHtml();
		author = author.DecodeHtml();
		authorClass = authorClass.DecodeHtml();
		content = content.DecodeHtml();

		return new MessageModel( subject, author, authorClass, dateOffset, content );
	}
	static readonly Regex messageDocBodyRx = new( @"<body([\s\S]*)<\/body>" );
	static readonly Regex messageAuthorRx = new( @"<tr><td class=""medium left""><b>Nadawca<\/b><\/td><td class=""left"">[\s\S]*?\((?<author>[\s\S]*?)\)\s*\[(?<authorClass>[\s\S]*?)\]<\/td><\/tr>" );
	static readonly Regex messageSubjectRx = new( @"<tr><td class=""medium left""><b>Temat<\/b><\/td><td class=""left"">\s*(?<subject>[^<]*?)\s*<\/td><\/tr>" );
	static readonly Regex messageDateRx = new( @"<tr><td class=""medium left""><b>Wysłano<\/b><\/td><td class=""left"">\s*(?<date>[^<]*?)\s*<\/td><\/tr>" );
	static readonly Regex messageContentRx = new( @"<div class=""container-message-content"">(?<content>[\s\S]*?)<\/div>" );



}

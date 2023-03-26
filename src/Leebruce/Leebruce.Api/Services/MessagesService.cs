using Leebruce.Api.Extensions;
using Leebruce.Api.Models;
using Leebruce.Domain;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services;

public interface IMessagesService
{
	Task<MessageMetadataModel[]> GetMessagesAsync( ClaimsPrincipal principal );
	Task<MessageMetadataModel[]> GetMessagesAsync( ClaimsPrincipal principal, int page );
	Task<MessageModel> GetMessageAsync( ClaimsPrincipal principal, string id );
	Task<FileDto> GetAttachmentAsync( ClaimsPrincipal principal, string id );
}

public partial class MessagesService : IMessagesService
{
	private readonly ILbSiteClient _lbClient;
	private readonly ILiblinkService _liblinkService;

	public MessagesService( ILbSiteClient lbClient, ILiblinkService liblinkService )
	{
		_lbClient = lbClient;
		_liblinkService = liblinkService;
	}

	#region messages list
	public async Task<MessageMetadataModel[]> GetMessagesAsync( ClaimsPrincipal principal )
	{
		var document = await _lbClient.GetContentAuthorized( "https://synergia.librus.pl/wiadomosci" );

		string table = ExtractListTable( document );

		return ExtractMessages( table ).ToArray();
	}
	public async Task<MessageMetadataModel[]> GetMessagesAsync( ClaimsPrincipal principal, int page )
	{
		using StringContent pageCtnt = new( page.ToString() );
		using StringContent idPojemnikaCtnt = new( "105" );
		using MultipartFormDataContent form = new()
		{
			{ pageCtnt, "numer_strony105" },
			{ idPojemnikaCtnt, "idPojemnika" },
		};

		var document = await _lbClient.PostContentAuthorized( "https://synergia.librus.pl/wiadomosci", form );

		string table = ExtractListTable( document );

		return ExtractMessages( table ).ToArray();
	}

	static string ExtractListTable( string document )
	{
		var tableMatch = MessagesListTableRx().Match( document );
		var table = tableMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract table from document." );

		var bodyMatch = MessagesListTableBodyRx().Match( table );
		return bodyMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract body from table." ); ;
	}
	[GeneratedRegex( @"<table class=""decorated stretch""[\s\S]*?>([\s\S]*?)<\/table>" )]
	private static partial Regex MessagesListTableRx();
	[GeneratedRegex( @"<tbody[\s\S]*?>([\s\S]*?)<\/tbody>" )]
	private static partial Regex MessagesListTableBodyRx();

	static IEnumerable<MessageMetadataModel> ExtractMessages( string tableBody )
	{
		var matches = MessagesTableRowRx().Matches( tableBody );
		foreach ( var match in matches.Cast<Match>() )
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

			bool hasAttachment = match.GetGroup( "ifAttachment" ) is not null;
			bool isUnread = match.GetGroup( "ifUnread" ) is not null;

			author = author.DecodeHtml();
			subject = subject.DecodeHtml();
			var id = link.ToUrlBase64();

			yield return new MessageMetadataModel( subject, author, dateOffset, hasAttachment, isUnread, id );
		}
	}
	// $1: link, $2: author, $3: subject, $4: date
	//[GeneratedRegex(  @"<tr[^>]*>(?:\s*<td[^>]*>[\s\S]*?<\/td>\s*<td[^>]*>[\s\S]*?<\/td>\s*)<td\s*>\s*<a href=""\/wiadomosci\/(?<link>[^""]*)"">[\w\s.-]*?\((?<author>[\w\s.-]*?)\)\s*<\/a>\s*<\/td>\s*<td\s*>\s*<a href=""\/wiadomosci\/\1"">\s*(?<subject>[\s\S]*?)\s*<\/a>\s*<\/td>\s*<td[^>]*>\s*(?<date>[\d\s-:]*)\s*<\/td>"  )]
	//private static partial Regex MessagesTableRowRx();
	[GeneratedRegex( @"<tr[^>]*>(?:\s*<td[^>]*>\s*<input[^>]*>\s*<\/td>\s*<td[^>]*>\s*(?<ifAttachment><img\s*src=""\/assets\/img\/attachment.png""[^>]*>\s*)?<\/td>\s*)<td\s*(?<ifUnread>style=""font-weight: bold;"")?\s*>\s*<a href=""\/wiadomosci\/(?<link>[^""]*)"">[\w\s.-]*?\((?<author>[\w\s.-]*?)\)\s*<\/a>\s*<\/td>\s*<td\s*(?:style=""font-weight: bold;"")?\s*>\s*<a href=""\/wiadomosci\/\3"">\s*(?<subject>[\s\S]*?)\s*<\/a>\s*<\/td>\s*<td[^>]*>\s*(?<date>[\d\s\-:]*)\s*<\/td>" )]
	private static partial Regex MessagesTableRowRx();

	#endregion

	#region single message
	public async Task<MessageModel> GetMessageAsync( ClaimsPrincipal principal, string id )
	{
		var link = StringExtensions.FromUrlBase64( id );

		var document = await _lbClient.GetContentAuthorized( $"https://synergia.librus.pl/wiadomosci/{link}" );

		return await ExtractMessageAsync( document );
	}

	async Task<MessageModel> ExtractMessageAsync( string document )
	{
		var docMatch = MessageDocBodyRx().Match( document );
		var body = docMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract body from document." );

		var dateMatch = MessageDateRx().Match( body );
		var dateStr = dateMatch.GetGroup( "date" )
			?? throw new ProcessingException( "Failed to extract date from document." );

		if ( !DateTime.TryParse( dateStr, out var date ) )
			throw new ProcessingException( "Date extracted from table was invalid." );

		var offset = TimeZoneInfo.FindSystemTimeZoneById( "Central European Standard Time" ).GetUtcOffset( date );
		DateTimeOffset dateOffset = new( date, offset );

		var authorMatch = MessageAuthorRx().Match( body );
		var author = authorMatch.GetGroup( "author" )
			?? throw new ProcessingException( "Failed to extract author from document." );

		var authorClass = authorMatch.GetGroup( "authorClass" )
			?? throw new ProcessingException( "Failed to extract author from document." );

		var subjectMatch = MessageSubjectRx().Match( body );
		var subject = subjectMatch.GetGroup( "subject" )
			?? throw new ProcessingException( "Failed to extract subject from document." );

		var contentMatch = MessageContentRx().Match( body );
		var content = contentMatch.GetGroup( "content" )
			?? throw new ProcessingException( "Failed to extract content from document." );

		subject = subject.DecodeHtml();
		author = author.DecodeHtml();
		authorClass = authorClass.DecodeHtml();
		content = content.DecodeHtml();
		content = await _liblinkService.ConvertLiblinks( content );
		content = content.NormalizeHtmlAnchors().EncodeHtml();

		var attachments = ExtractAttachments( body ).ToArray();

		return new MessageModel( subject, author, authorClass, dateOffset, content, attachments );
	}
	[GeneratedRegex( @"<body([\s\S]*)<\/body>" )]
	private static partial Regex MessageDocBodyRx();
	[GeneratedRegex( @"<tr><td class=""medium left""><b>Nadawca<\/b><\/td><td class=""left"">[\s\S]*?\((?<author>[\s\S]*?)\)\s*\[(?<authorClass>[\s\S]*?)\]<\/td><\/tr>" )]
	private static partial Regex MessageAuthorRx();
	[GeneratedRegex( @"<tr><td class=""medium left""><b>Temat<\/b><\/td><td class=""left"">\s*(?<subject>[^<]*?)\s*<\/td><\/tr>" )]
	private static partial Regex MessageSubjectRx();
	[GeneratedRegex( @"<tr><td class=""medium left""><b>Wysłano<\/b><\/td><td class=""left"">\s*(?<date>[^<]*?)\s*<\/td><\/tr>" )]
	private static partial Regex MessageDateRx();
	[GeneratedRegex( @"<div class=""container-message-content"">(?<content>[\s\S]*?)<\/div>" )]
	private static partial Regex MessageContentRx();

	static IEnumerable<AttachmentModel> ExtractAttachments( string body )
	{
		var filesTableMatch = MessageFilesTableRx().Match( body );
		var filesTable = filesTableMatch.GetGroup( 1 );
		if ( filesTable is null )
			yield break;

		var fileMatches = MessageFileRx().Matches( filesTable );
		foreach ( var fileMatch in fileMatches.Cast<Match>() )
		{
			var fileName = fileMatch.GetGroup( "fileName" )
				?? throw new ProcessingException( "Failed to extract filename from attachment." );

			var link = fileMatch.GetGroup( "link" )
				?? throw new ProcessingException( "Failed to extract link from attachment." );

			fileName = fileName.DecodeHtml();
			var id = link.Replace( @"\/", "/" ).ToUrlBase64();

			yield return new AttachmentModel( fileName, id );
		}

	}
	[GeneratedRegex( @"<table>\s*<tr>\s*<td colspan=""2"" class=""left"">\s*<b>Pliki:<\/b>\s*([\s\S]*?)<\/table>" )]
	private static partial Regex MessageFilesTableRx();
	// $1: File 1.pdf (may be with spaces), $2: <messageId>\/<attachmentId> (slash is escaped because in source it is in js string)
	[GeneratedRegex( @"<tr>\s*<td>\s*<!-- icon -->\s*<img src=""[\s\S]*?"" \/>\s*<!-- name -->\s*(?<fileName>[\s\S]*?)\s*<\/td>\s*<td>\s*&nbsp;\s*<!-- download button -->\s*<a href=""javascript:void\(0\);"">\s*<\s*img\s*src=""\/assets\/img\/homework_files_icons\/download.png""\s*class=""""\s*title=""""\s*onClick=""\s*otworz_w_nowym_oknie\(\s*&quot;\\\/wiadomosci\\\/pobierz_zalacznik\\\/(?<link>[\s\S]*?)&quot;,\s*&quot;o2&quot;,\s*420,\s*250\s*\)\s*""\s*\/>\s*<\/a>\s*<\/td>\s*<\/tr>" )]
	private static partial Regex MessageFileRx();

	#endregion

	#region attachments
	public async Task<FileDto> GetAttachmentAsync( ClaimsPrincipal principal, string id )
	{
		var link = StringExtensions.FromUrlBase64( id );

		// get actual location
		//using var preResp = await _http.GetWithCookiesAsync( $"https://synergia.librus.pl/wiadomosci/pobierz_zalacznik/{link}", _lbUser.UserCookieHeader );
		//var preDoc = await preResp.Content.ReadAsStringAsync();
		//if ( _lbHelper.IsUnauthorized( preDoc ) )
		//	throw new NotAuthorizedException();

		//var location = preResp.Headers.Location
		//	?? throw new ProcessingException( "Failed to get attachment location." );
		var (_, headers, _) = await _lbClient.GetContentAndHeadersAuthorized( $"https://synergia.librus.pl/wiadomosci/pobierz_zalacznik/{link}" );
		var location = headers.Location
			?? throw new ProcessingException( "Failed to get attachment location." );

		// query location
		var fileResp =
		await TryGetAttachmentFromGetFileAsync( location )
			?? await GetAttachmentFromCSDownloadAsync( location )
			?? throw new ProcessingException( "Unexpected location link." );

		var contentType = fileResp.Content.Headers.ContentType
			?? throw new ProcessingException( "Failed to get attachment content type." );

		var cd = fileResp.Content.Headers.GetValues( HeaderNames.ContentDisposition ).FirstOrDefault()
			?? throw new ProcessingException( "Failed to get attachment content disposition." );

		cd = Encoding.UTF8.GetString( Encoding.Latin1.GetBytes( cd ) );
		string fileName = cd.Split( ';' )
			.Select( x => x.Trim() )
			.Where( x => x.StartsWith( "filename=" ) )
			.FirstOrDefault()
			?? throw new ProcessingException( "Failed to get file name from content disposition." );
		fileName = fileName["filename=".Length..];
		fileName = fileName.Trim( '\"' );

		//ContentDisposition contentDisposition = new( cd );
		//string fileName = contentDisposition.FileName
		//	?? throw new ProcessingException( "Failed to get file name from content disposition." );

		//string fileName = cd.Split( "filename=" ).Skip( 1 ).FirstOrDefault()
		//	?? throw new ProcessingException( "Failed to get file name from content disposition." );

		string mediaType = contentType.MediaType
			?? throw new ProcessingException( "Failed to get media type from content type." );

		HttpResponseMessageStream document = new( fileResp );
		await document.Init();

		return new FileDto( fileName, mediaType, document );
	}

	public async Task<HttpResponseMessage?> TryGetAttachmentFromGetFileAsync( Uri location )
	{
		if ( location.Segments[1] != "GetFile/" )
			return null;

		return await _lbClient.GetAuthorized( location.ToString() + "/get" );
	}
	public async Task<HttpResponseMessage?> GetAttachmentFromCSDownloadAsync( Uri location )
	{
		var locationStr = location.ToString();
		if ( !locationStr.Contains( "CSTryToDownload" ) )
			return null;

		var key = System.Web.HttpUtility.ParseQueryString( location.Query )["singleUseKey"]
			?? throw new ProcessingException( "Failed to extract key from location uri." );

		// check data
		Dictionary<string, string> data = new() { ["singleUseKey"] = key };
		using FormUrlEncodedContent ctnt = new( data );

		// check
		TokenCheckResponse checkResponse;
		do
		{
			var checkResp = await _lbClient.PostAuthorized( "https://sandbox.librus.pl/index.php?action=CSCheckKey", ctnt );
			checkResponse = await checkResp.Content.ReadFromJsonAsync<TokenCheckResponse>()
				?? throw new ProcessingException( "Token check sent invalid response." );

			await Task.Delay( 200 );
		} while ( checkResponse.Status != "ready" );

		// download
		var downloadLocation = locationStr.Replace( "CSTryToDownload", "CSDownload" );
		return await _lbClient.GetAuthorized( downloadLocation );
	}

	/// <summary>
	/// solution to avoid undisposed HttpResponseMessage. I don't know if this is necesary but i guess it stays for now
	/// </summary>
	class HttpResponseMessageStream : Stream
	{
		private Stream? _stream;
		private readonly HttpResponseMessage _message;

		public override bool CanRead => _stream?.CanRead ?? throw new Exception( "Stream not initialized." );
		public override bool CanSeek => _stream?.CanSeek ?? throw new Exception( "Stream not initialized." );
		public override bool CanWrite => _stream?.CanWrite ?? throw new Exception( "Stream not initialized." );
		public override long Length => _stream?.Length ?? throw new Exception( "Stream not initialized." );
		public override long Position
		{
			get => _stream?.Position ?? throw new Exception( "Stream not initialized." );
			set { if ( _stream is not null ) _stream.Position = value; else throw new Exception( "Stream not initialized." ); }
		}

		public HttpResponseMessageStream( HttpResponseMessage message )
		{
			_message = message;
		}
		public async Task Init()
		{
			_stream = await _message.Content.ReadAsStreamAsync();
		}

		public override void Flush()
		{
			if ( _stream is not null )
				_stream.Flush();
			else
				throw new Exception( "Stream not initialized." );
		}
		public override int Read( byte[] buffer, int offset, int count )
		{
			return _stream?.Read( buffer, offset, count )
				?? throw new Exception( "Stream not initialized." );
		}
		public override long Seek( long offset, SeekOrigin origin )
		{
			return _stream?.Seek( offset, origin )
				?? throw new Exception( "Stream not initialized." );
		}
		public override void SetLength( long value )
		{
			if ( _stream is not null )
				_stream.SetLength( value );
			else
				throw new Exception( "Stream not initialized." );
		}
		public override void Write( byte[] buffer, int offset, int count )
		{
			if ( _stream is not null )
				_stream.Write( buffer, offset, count );
			else
				throw new Exception( "Stream not initialized." );
		}

		protected override void Dispose( bool disposing )
		{
			( (IDisposable?)_stream )?.Dispose();
			( (IDisposable)_message ).Dispose();
			base.Dispose( disposing );
		}
		public override ValueTask DisposeAsync()
		{
			( (IDisposable?)_stream )?.Dispose();
			( (IDisposable)_message ).Dispose();
			return base.DisposeAsync();
		}
	}

	public record class TokenCheckResponse(
		string Status );

	#endregion

}

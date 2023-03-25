using System.Text;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services;

public interface ILiblinkService
{
	Task<string> ConvertLiblinks( string liblikedString );
}

public partial class LiblinkService : ILiblinkService
{
	private readonly HttpClient _client;

	public LiblinkService( HttpClient client )
	{
		_client = client;
	}

	public async Task<string> ConvertLiblinks( string liblikedString )
	{
		var liblinks = LiblinkRx().Matches( liblikedString )
			.Select( match => match.Value )
			.ToHashSet();

		var mapping = await Task.WhenAll( liblinks.Select( ConvertLink ) );

		StringBuilder sb = new( liblikedString );
		foreach ( var (liblink, clearLink) in mapping )
		{
			_ = sb.Replace( clearLink, liblink );
		}

		return sb.ToString();
	}
	[GeneratedRegex( @"https://liblink.pl/\w*" )]
	private static partial Regex LiblinkRx();

	private async Task<(string liblink, string clearLink)> ConvertLink( string link )
	{
		var res = await _client.PostAsync( link, null );
		return (res.Headers.Location?.ToString() ?? link, link);
	}
}

using Leebruce.Api.Extensions;
using Leebruce.Domain;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services
{
	public interface IAnnouncementsService
	{
		Task<AnnouncementModel[]> GetAnnouncementsAsync( ClaimsPrincipal principal );
	}

	public class AnnouncementsService : IAnnouncementsService
	{
		private readonly ILbHelperService _lbHelper;
		private static readonly TimeSpan regexTimeout = TimeSpan.FromSeconds( 2 );

		public AnnouncementsService( ILbHelperService lbHelper )
		{
			_lbHelper = lbHelper;
		}

		public async Task<AnnouncementModel[]> GetAnnouncementsAsync( ClaimsPrincipal principal )
		{
			using HttpClientHandler handler = _lbHelper.CreateHandler( principal );
			using HttpClient http = new( handler );

			using var resp = await http.GetAsync( "https://synergia.librus.pl/ogloszenia" );
			string document = await resp.Content.ReadAsStringAsync();

			if ( _lbHelper.IsUnauthorized( document ) )
				throw new NotAuthorizedException();

			var list = ExtractList( document );
			return ExtractItems( list )
				.Select( ExtractModel )
				.ToArray();
		}

		static string ExtractList( string document )
		{
			var tableMatch = annListRx.Match( document );
			return tableMatch.GetGroup( 1 )
				?? throw new ProcessingException( "Failed to extract list from document." );
		}
		static readonly Regex annListRx = new( @"<h\d[^>]*>\s*Ogłoszenia - Tablica ogłoszeń\s*<\/h\d>\s*<div class=""container-background"">([\s\S]*?)<\/div>", RegexOptions.None, regexTimeout );

		static IEnumerable<string> ExtractItems( string list )
		{
			var tableMatches = annItemRx.Matches( list );
			foreach ( var match in tableMatches.Cast<Match>() )
			{
				yield return match.GetGroup( 1 ) ?? throw new ProcessingException( "Failed to extract rows from table." );
			}
		}
		static readonly Regex annItemRx = new( @"<table [^>]*>([\s\S]*?)<\/table>", RegexOptions.None, regexTimeout );

		static AnnouncementModel ExtractModel( string item )
		{
			var dateStr = annDateRx.Match( item ).GetGroup( "date" )
				?? throw new ProcessingException( "Failed to extract date from announcement" );

			if ( !DateOnly.TryParse( dateStr, out var date ) )
				throw new ProcessingException( "Date extracted from announcement was invalid." );

			var title = annTitleRx.Match( item ).GetGroup( "title" )
				?? throw new ProcessingException( "Failed to extract title from announcement" );

			var author = annAuthorRx.Match( item ).GetGroup( "author" )
				?? throw new ProcessingException( "Failed to extract author from announcement" );

			var content = annContentRx.Match( item ).GetGroup( "content" )
				?? throw new ProcessingException( "Failed to extract content from announcement" );

			content = content.DecodeHtml();
			title = title.DecodeHtml();
			author = author.DecodeHtml();

			return new AnnouncementModel( title, date, author, content );
		}
		static readonly Regex annTitleRx = new( @"<thead>\s*<tr>\s*<td[^>]*>(?<title>[^<]*)<\/td>\s*<\/tr>\s*<\/thead>", RegexOptions.None, regexTimeout );
		static readonly Regex annAuthorRx = new( @"<tr[^>]*>\s*<th[^>]*>Dodał<\/th>\s*<td>\s*(?<author>[^<]*)\s*<\/td>\s*<\/tr>", RegexOptions.None, regexTimeout );
		static readonly Regex annDateRx = new( @"<tr[^>]*>\s*<th[^>]*>Data publikacji<\/th>\s*<td>\s*(?<date>[^<]*)\s*<\/td>\s*<\/tr>", RegexOptions.None, regexTimeout );
		static readonly Regex annContentRx = new( @"<tr[^>]*>\s*<th[^>]*>Treść<\/th>\s*<td>\s*(?<content>[\s\S]*?)\s*<\/td>\s*<\/tr>", RegexOptions.None, regexTimeout );

	}
}

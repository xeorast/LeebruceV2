﻿using Leebruce.Api.Extensions;
using Leebruce.Domain;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services.LbPages;

public interface IAnnouncementsService
{
	Task<AnnouncementModel[]> GetAnnouncementsAsync();
}

public partial class AnnouncementsService : IAnnouncementsService
{
	private readonly ILbSiteClient _lbClient;
	private const int regexTimeout = 2000;

	public AnnouncementsService( ILbSiteClient lbClient )
	{
		_lbClient = lbClient;
	}

	public async Task<AnnouncementModel[]> GetAnnouncementsAsync()
	{
		var document = await _lbClient.GetContentAuthorized( "/ogloszenia" );

		var list = ExtractList( document );
		return ExtractItems( list )
			.Select( ExtractModel )
			.ToArray();
	}

	static string ExtractList( string document )
	{
		var tableMatch = AnnListRx().Match( document );
		return tableMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract list from document." );
	}
	[GeneratedRegex( @"<h\d[^>]*>\s*Ogłoszenia - Tablica ogłoszeń\s*<\/h\d>\s*<div class=""container-background"">([\s\S]*?)<\/div>", RegexOptions.None, regexTimeout )]
	private static partial Regex AnnListRx();

	static IEnumerable<string> ExtractItems( string list )
	{
		var tableMatches = AnnItemRx().Matches( list );
		foreach ( var match in tableMatches.Cast<Match>() )
		{
			yield return match.GetGroup( 1 ) ?? throw new ProcessingException( "Failed to extract rows from table." );
		}
	}
	[GeneratedRegex( @"<table [^>]*>([\s\S]*?)<\/table>", RegexOptions.None, regexTimeout )]
	private static partial Regex AnnItemRx();

	static AnnouncementModel ExtractModel( string item )
	{
		var dateStr = AnnDateRx().Match( item ).GetGroup( "date" )
			?? throw new ProcessingException( "Failed to extract date from announcement" );

		if ( !DateOnly.TryParse( dateStr, out var date ) )
			throw new ProcessingException( "Date extracted from announcement was invalid." );

		var title = AnnTitleRx().Match( item ).GetGroup( "title" )
			?? throw new ProcessingException( "Failed to extract title from announcement" );

		var author = AnnAuthorRx().Match( item ).GetGroup( "author" )
			?? throw new ProcessingException( "Failed to extract author from announcement" );

		var content = AnnContentRx().Match( item ).GetGroup( "content" )
			?? throw new ProcessingException( "Failed to extract content from announcement" );

		content = content.DecodeHtml()
			.WrapLinksInHtmlAnchors()
			.NormalizeHtmlAnchors()
			.EncodeHtml();
		title = title.DecodeHtml();
		author = author.DecodeHtml();

		return new AnnouncementModel( title, date, author, content );
	}
	[GeneratedRegex( @"<thead>\s*<tr>\s*<td[^>]*>(?<title>[^<]*)<\/td>\s*<\/tr>\s*<\/thead>", RegexOptions.None, regexTimeout )]
	private static partial Regex AnnTitleRx();
	[GeneratedRegex( @"<tr[^>]*>\s*<th[^>]*>Dodał<\/th>\s*<td>\s*(?<author>[^<]*)\s*<\/td>\s*<\/tr>", RegexOptions.None, regexTimeout )]
	private static partial Regex AnnAuthorRx();
	[GeneratedRegex( @"<tr[^>]*>\s*<th[^>]*>Data publikacji<\/th>\s*<td>\s*(?<date>[^<]*)\s*<\/td>\s*<\/tr>", RegexOptions.None, regexTimeout )]
	private static partial Regex AnnDateRx();
	[GeneratedRegex( @"<tr[^>]*>\s*<th[^>]*>Treść<\/th>\s*<td>\s*(?<content>[\s\S]*?)\s*<\/td>\s*<\/tr>", RegexOptions.None, regexTimeout )]
	private static partial Regex AnnContentRx();

}

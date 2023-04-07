using Leebruce.Api.Extensions;
using Leebruce.Domain;
using Leebruce.Domain.Schedule;
using Leebruce.Domain.Schedule.EventDataTypes;
using System.Text.RegularExpressions;
using System.Web;

namespace Leebruce.Api.Services.LbPages;

public interface IScheduleService
{
	Task<ScheduleDay[]> GetScheduleAsync();
	Task<ScheduleDay[]> GetScheduleAsync( DateOnly date );
}

public partial class ScheduleService : IScheduleService
{
	private readonly ILbSiteClient _lbClient;
	private readonly IWebHostEnvironment _environment;
	private const int regexTimeout = 2000;

	public ScheduleService( ILbSiteClient lbClient, IWebHostEnvironment environment )
	{
		_lbClient = lbClient;
		_environment = environment;
	}

	public Task<ScheduleDay[]> GetScheduleAsync()
		=> GetScheduleAsync( DateOnly.FromDateTime( DateTime.Now ) );

	public async Task<ScheduleDay[]> GetScheduleAsync( DateOnly date )
	{
		Dictionary<string, string> data = new()
		{
			["miesiac"] = date.Month.ToString(),
			["rok"] = date.Year.ToString(),
		};
		using FormUrlEncodedContent ctnt = new( data );

		var document = await _lbClient.PostContentAuthorized( "/terminarz", ctnt );

		return ExtractDays( document ).ToArray();
	}

	IEnumerable<ScheduleDay> ExtractDays( string document )
	{
		// body
		var bodyMatch = ScheduleBodyRx().Match( document );
		var body = bodyMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract body from document." );

		// month
		var monthSelectMatch = ScheduleMonthSelectRx().Match( body );
		var monthSelect = monthSelectMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract month select from document." );

		var monthMatch = ScheduleSelectedRx().Match( monthSelect );
		var monthStr = monthMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract month from select." );

		if ( !int.TryParse( monthStr, out int month ) )
			throw new ProcessingException( "Month extracted from select was invalid." );

		// year
		var yearSelectMatch = ScheduleYearSelectRx().Match( body );
		var yearSelect = yearSelectMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract year select from document." );

		var yearMatch = ScheduleSelectedRx().Match( yearSelect );
		var yearStr = yearMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract year from select." );

		if ( !int.TryParse( yearStr, out int year ) )
			throw new ProcessingException( "Year extracted from select was invalid." );

		// day and data
		var dayMatches = ScheduleDayRx().Matches( body );
		foreach ( var match in dayMatches.Cast<Match>() )
		{
			ScheduleDay? scheduleDay = null;
			try
			{
				var dayStr = match.GetGroup( "day" )
					?? throw new ProcessingException( "Failed to extract day number from document." );

				if ( !int.TryParse( dayStr, out var day ) )
					throw new ProcessingException( "Day number extracted from document was invalid." );

				var data = match.GetGroup( "data" )
					?? throw new ProcessingException( "Failed to extract day data from document." );

				var events = ExtractEvents( data ).ToArray();
				DateOnly date = new( year, month, day );

				scheduleDay = new( date, events );
			}
			catch ( ProcessingException )
			{
				//todo: log error
			}
			if ( scheduleDay is not null )
				yield return scheduleDay;
		}
	}
	[GeneratedRegex( @"<body[^>]*>([\s\S]*?)<\/body>", RegexOptions.None, regexTimeout )]
	private static partial Regex ScheduleBodyRx();
	[GeneratedRegex( @"<select name=""miesiac""\s*class=""ListaWyboru""[^>]*>([\s\S]*?)</select>", RegexOptions.None, regexTimeout )]
	private static partial Regex ScheduleMonthSelectRx();
	[GeneratedRegex( @"<select name=""rok""\s*class=""ListaWyboru""[^>]*>([\s\S]*?)</select>", RegexOptions.None, regexTimeout )]
	private static partial Regex ScheduleYearSelectRx();
	[GeneratedRegex( @"<option value=""([^""]*)"" selected=""selected"" >", RegexOptions.None, regexTimeout )]
	private static partial Regex ScheduleSelectedRx();
	[GeneratedRegex( @"<td class=""center\s*(?:today\s*)?"" ><div class=""kalendarz-dzien""><div class=""kalendarz-numer-dnia"">\s*(?<day>\d*)\s*<\/div><table><tbody>(?<data>[\s\S]*?)<\/tbody>", RegexOptions.None, regexTimeout )]
	private static partial Regex ScheduleDayRx();

	IEnumerable<ScheduleEvent> ExtractEvents( string data )
	{
		var eventMatches = ScheduleEventRx().Matches( data );
		foreach ( var match in eventMatches.Cast<Match>() )
		{
			ScheduleEvent ev;
			try
			{
				ev = DecodeEventData( match );
			}
			catch ( ProcessingException )
			{
				//todo: log error
				ev = ScheduleServiceHelper.ErrorEvent( "Error occured processing event data." );
			}
			yield return ev;
		}
	}
	[GeneratedRegex( @"<tr><td [^>]*?style=""background-color: .*?;.*?"" (?:title=""(?<title>.*?)"")?\s*(?:onclick=""location.href='/terminarz/(?<link>.*?)'"")?\s*>\s*(?<what>.*?)\s*(?:(?:</td></tr>)|$|(?=<tr><td [^>]*?style=""background-color:))", RegexOptions.None, regexTimeout )]
	private static partial Regex ScheduleEventRx();
	//(?<link>szczegoly(?:_wolne)?/.*?)

	public ScheduleEvent DecodeEventData( Match match )
	{
		// main payload
		var what = match.GetGroup( "what" )
			?? throw new ProcessingException( "Failed to extract event data from day data." );

		// id to details
		var link = match.GetGroup( "link" );
		var id = link?.ToUrlBase64();

		// "title" tag sometimes contains inline details
		var title = match.GetGroup( "title" );

		if ( title is not null )
			title = HttpUtility.HtmlDecode( title );

		var additionalData =
			title is null
			? new()
			: DecodeEventTitle( title );

		// extract data from string and merge with details from "title"
		var eventData = DecodeEventData( what, link ?? "", additionalData );

		// details sometimes contain addition date
		DateTimeOffset? dateOffset = null;
		if ( additionalData.TryGetValue( "Data dodania", out var dateStr ) )
		{
			if ( !DateTime.TryParse( dateStr, out var date ) )
				throw new ProcessingException( "Date extracted from title was invalid." );

			var offset = TimeZoneInfo.FindSystemTimeZoneById( "Central European Standard Time" ).GetUtcOffset( date );
			dateOffset = new( date, offset );
		}

		return ScheduleEvent.From( id, dateOffset, eventData );
	}
	public IEventData DecodeEventData( string data, string link, Dictionary<string, string> additionalData )
	{
		data = HttpUtility.HtmlDecode( data );
		var segments = BrRx().Split( data );

		return ScheduleServiceHelper.TryFromAbsenceData( segments ) as IEventData
			?? ScheduleServiceHelper.TryFromClassAbsenceData( segments ) as IEventData
			?? ScheduleServiceHelper.TryFromFreeDayData( link, segments ) as IEventData
			?? ScheduleServiceHelper.TryFromSubstitutionData( segments ) as IEventData
			?? ScheduleServiceHelper.TryFromCancellationData( segments ) as IEventData
			?? ScheduleServiceHelper.TryFromTestEtcData( segments, additionalData ) as IEventData
			?? ScheduleServiceHelper.FromUnrecognizedData( segments, _environment.IsDevelopment() );

	}
	public static Dictionary<string, string> DecodeEventTitle( string data )
	{
		var matches = AdditionalDataRx().Matches( data );

		Dictionary<string, string> result = new();
		foreach ( var match in matches.Cast<Match>() )
		{
			var category = match.GetGroup( "category" );
			var value = match.GetGroup( "value" );
			if ( category is null || value is null )
			{
				continue;
			}

			result[category] = value.DecodeHtml();
		}

		return result;
	}
	[GeneratedRegex( @"<br\s*/?>", RegexOptions.None, regexTimeout )]
	private static partial Regex BrRx();
	[GeneratedRegex( @"(?<category>[^<>""]*?): (?<value>(?:[^<>""]|<br \/>)*?)(?=<br \/>[^<>""]*: |"")", RegexOptions.None, regexTimeout )]
	private static partial Regex AdditionalDataRx();

}

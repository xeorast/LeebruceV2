using Leebruce.Api.Extensions;
using Leebruce.Domain;
using Leebruce.Domain.Schedule;
using Leebruce.Domain.Schedule.EventDataTypes;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;

namespace Leebruce.Api.Services;

public interface IScheduleService
{
	Task<ScheduleDay[]> GetScheduleAsync( ClaimsPrincipal principal );
	Task<ScheduleDay[]> GetScheduleAsync( ClaimsPrincipal principal, DateOnly date );
}

public class ScheduleService : IScheduleService
{
	private readonly ILbHelperService _lbHelper;
	private readonly IWebHostEnvironment _environment;
	private static readonly TimeSpan regexTimeout = TimeSpan.FromSeconds( 2 );

	public ScheduleService( ILbHelperService lbHelper, IWebHostEnvironment environment )
	{
		_lbHelper = lbHelper;
		_environment = environment;
	}

	public async Task<ScheduleDay[]> GetScheduleAsync( ClaimsPrincipal principal )
	{
		using HttpClientHandler handler = _lbHelper.CreateHandler( principal );
		using HttpClient http = new( handler );

		using var resp = await http.GetAsync( "https://synergia.librus.pl/terminarz" );
		string document = await resp.Content.ReadAsStringAsync();

		if ( _lbHelper.IsUnauthorized( document ) )
			throw new NotAuthorizedException();

		return ExtractDays( document ).ToArray();
	}
	public async Task<ScheduleDay[]> GetScheduleAsync( ClaimsPrincipal principal, DateOnly date )
	{
		using HttpClientHandler handler = _lbHelper.CreateHandler( principal );
		using HttpClient http = new( handler );

		Dictionary<string, string> data = new()
		{
			["miesiac"] = date.Month.ToString(),
			["rok"] = date.Year.ToString(),
		};
		using FormUrlEncodedContent ctnt = new( data );

		using var resp = await http.PostAsync( "https://synergia.librus.pl/terminarz", ctnt );
		string document = await resp.Content.ReadAsStringAsync();

		if ( _lbHelper.IsUnauthorized( document ) )
			throw new NotAuthorizedException();

		return ExtractDays( document ).ToArray();
	}

	IEnumerable<ScheduleDay> ExtractDays( string document )
	{
		// body
		var bodyMatch = ScheduleBodyRx.Match( document );
		var body = bodyMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract body from document." );

		// month
		var monthSelectMatch = ScheduleMonthSelectRx.Match( body );
		var monthSelect = monthSelectMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract month select from document." );

		var monthMatch = ScheduleSelectedRx.Match( monthSelect );
		var monthStr = monthMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract month from select." );

		if ( !int.TryParse( monthStr, out int month ) )
			throw new ProcessingException( "Month extracted from select was invalid." );

		// year
		var yearSelectMatch = ScheduleYearSelectRx.Match( body );
		var yearSelect = yearSelectMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract year select from document." );

		var yearMatch = ScheduleSelectedRx.Match( yearSelect );
		var yearStr = yearMatch.GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract year from select." );

		if ( !int.TryParse( yearStr, out int year ) )
			throw new ProcessingException( "Year extracted from select was invalid." );

		// day and data
		var dayMatches = ScheduleDayRx.Matches( body );
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
	static readonly Regex ScheduleBodyRx = new( @"<body[^>]*>([\s\S]*?)<\/body>", RegexOptions.None, regexTimeout );
	static readonly Regex ScheduleMonthSelectRx = new( @"<select name=""miesiac""\s*class=""ListaWyboru""[^>]*>([\s\S]*?)</select>", RegexOptions.None, regexTimeout );
	static readonly Regex ScheduleYearSelectRx = new( @"<select name=""rok""\s*class=""ListaWyboru""[^>]*>([\s\S]*?)</select>", RegexOptions.None, regexTimeout );
	static readonly Regex ScheduleSelectedRx = new( @"<option value=""([^""]*)"" selected=""selected"" >", RegexOptions.None, regexTimeout );
	static readonly Regex ScheduleDayRx = new( @"<td class=""center"" ><div class=""kalendarz-dzien""><div class=""kalendarz-numer-dnia"">\s*(?<day>\d*)\s*<\/div><table><tbody>(?<data>[\s\S]*?)<\/tbody>", RegexOptions.None, regexTimeout );

	IEnumerable<ScheduleEvent> ExtractEvents( string data )
	{
		var eventMatches = ScheduleEventRx.Matches( data );
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
	static readonly Regex ScheduleEventRx = new( @"<tr><td [^>]*?style=""background-color: .*?;.*?"" (?:title=""(?<title>.*?)"")?\s*(?:onclick=""location.href='/terminarz/(?<link>.*?)'"")?\s*>\s*(?<what>.*?)\s*(?:(?:</td></tr>)|$|(?=<tr><td [^>]*?style=""background-color:))", RegexOptions.None, regexTimeout );
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
		var eventData = DecodeEventData( what, additionalData );

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
	public IEventData DecodeEventData( string data, Dictionary<string, string> additionalData )
	{
		data = HttpUtility.HtmlDecode( data );
		var segments = brRx.Split( data );

		return ScheduleServiceHelper.TryFromAbsenceData( segments ) as IEventData
			?? ScheduleServiceHelper.TryFromClassAbsenceData( segments ) as IEventData
			?? ScheduleServiceHelper.TryFromSubstitutionData( segments ) as IEventData
			?? ScheduleServiceHelper.TryFromCancellationData( segments ) as IEventData
			?? ScheduleServiceHelper.TryFromTestEtcData( segments, additionalData ) as IEventData
			?? ScheduleServiceHelper.FromUnrecognizedData( segments, _environment.IsDevelopment() );

	}
	public static Dictionary<string, string> DecodeEventTitle( string data )
	{
		data = HttpUtility.HtmlDecode( data );
		var segments = brRx.Split( data );

		var s = segments.Select( x => x.Split( ": " ) );
		return s.ToDictionary( x => x[0], x => x[1] );
	}
	static readonly Regex brRx = new( @"<br\s*/?>", RegexOptions.None, regexTimeout );

}

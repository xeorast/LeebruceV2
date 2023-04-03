using Leebruce.Api.Extensions;
using Leebruce.Domain.Timetable;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services.LbPages;

public interface ITimetableService
{
	Task<TimetableDayModel[]> GetTimetableAsync( ClaimsPrincipal principal );
	Task<TimetableDayModel[]> GetTimetableAsync( ClaimsPrincipal principal, DateOnly date );
}

public partial class TimetableService : ITimetableService
{
	private readonly ILbSiteClient _lbClient;
	private const int regexTimeout = 2000;

	public TimetableService( ILbSiteClient lbClient )
	{
		_lbClient = lbClient;
	}

	public async Task<TimetableDayModel[]> GetTimetableAsync( ClaimsPrincipal principal )
	{
		var document = await _lbClient.GetContentAuthorized( "/przegladaj_plan_lekcji" );

		return ProcessResponse( document );
	}
	public async Task<TimetableDayModel[]> GetTimetableAsync( ClaimsPrincipal principal, DateOnly date )
	{
		Dictionary<string, string> data = new() { ["tydzien"] = GetWeek( date ) };
		using FormUrlEncodedContent ctnt = new( data );

		var document = await _lbClient.PostContentAuthorized( "/przegladaj_plan_lekcji", ctnt );

		return ProcessResponse( document );
	}

	private static string GetWeek( DateOnly date )
	{
		var dayOfWeek = date.DayOfWeek;
		var offset = dayOfWeek is DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;

		var firstDay = date.AddDays( -offset );
		var lastDay = firstDay.AddDays( 6 );

		var startStr = firstDay.ToString( "O" );
		var endStr = lastDay.ToString( "O" );

		return $"{startStr}_{endStr}";
	}
	private static TimetableDayModel[] ProcessResponse( string document )
	{
		var (header, body) = ExtractTable( document );

		DateOnly[] dates = ExtractDates( header ).ToArray();
		string[] bodyRows = ExtractRows( body ).ToArray();
		LessonTimeModel[] times = bodyRows.Select( ExtractTime ).ToArray();

		string?[][] cellRows = bodyRows.Select( ExtractLessonCell ).ToArray();
		string?[][] cellColumns = cellRows.Transpose();

		LessonModel?[][] lessonColumns = ( from column in cellColumns
										   select column.Zip( times, ExtractLesson )
										   .TrimEnd( null ).ToArray()
										   ).ToArray();

		var days = dates.Zip( lessonColumns, ( date, lessons ) => new TimetableDayModel( date, lessons ) ).ToArray();

		return days;
	}

	#region Timetable regexes
	static (string header, string body) ExtractTable( string document )
	{
		var tableMatch = TimetableTableRx().Match( document );
		var table = tableMatch.GetGroup( "table" ) ?? throw new ProcessingException( "Failed to extract table from document." );

		var headerBodyMatch = TimetableHeaderBodyRx().Match( table );
		var header = headerBodyMatch.GetGroup( "header" ) ?? throw new ProcessingException( "Failed to extract header from table." );
		var body = headerBodyMatch.GetGroup( "body" ) ?? throw new ProcessingException( "Failed to extract body from table." );

		return (header, body);
	}
	[GeneratedRegex( @"<table class=""decorated plan-lekcji""[\s\S]*?>(?<table>[\s\S]*?)<\/table>", RegexOptions.None, regexTimeout )]
	private static partial Regex TimetableTableRx();
	[GeneratedRegex( @"<thead[\s\S]*?>(?<header>[\s\S]*?)<\/thead>\s*?(?<body>[\s\S]*?)\s*?<tfoot>", RegexOptions.None, regexTimeout )]
	private static partial Regex TimetableHeaderBodyRx();

	static IEnumerable<DateOnly> ExtractDates( string header )
	{
		var cellMatches = TimetableHeaderDaysRx().Matches( header );
		foreach ( var cellMatch in cellMatches.Cast<Match>() )
		{
			var dateStr = cellMatch.GetGroup( "date" ) ?? throw new ProcessingException( "Failed to extract date from header table header." );
			yield return DateOnly.TryParse( dateStr, out var date ) ? date : throw new ProcessingException( "Date extracted from header table was invalid." );
		}
	}
	// $1: DayOfWeek (in polish), $2: 2021-10-11
	[GeneratedRegex( @"<td>(?<weekDay>[^<>]*?)<BR[^<>]*?\/>(?<date>[^<>]*?)<\/td>", RegexOptions.None, regexTimeout )]
	private static partial Regex TimetableHeaderDaysRx();//todo: remove (unused) day of week group

	static IEnumerable<string> ExtractRows( string body )
	{
		var headerMatch = TimetableBodyRowsRx().Matches( body );
		foreach ( var match in headerMatch.Cast<Match>() )
		{
			yield return match.GetGroup( "row" ) ?? throw new ProcessingException( "Failed to extract rows from table." );
		}
	}
	[GeneratedRegex( @"<tr class=""line1"">(?<row>[\s\S]*?)<\/tr>", RegexOptions.None, regexTimeout )]
	private static partial Regex TimetableBodyRowsRx();

	static LessonTimeModel ExtractTime( string row )
	{
		var match = TimetableTimeRx().Match( row );

		string numStr = match.GetGroup( "number" ) ?? throw new ProcessingException( "Failed to extract lesson number from table." );
		if ( !int.TryParse( numStr, out var number ) )
			throw new ProcessingException( "Lesson number extracted from table was invalid." );

		string startStr = match.GetGroup( "start" ) ?? throw new ProcessingException( "Failed to extract lesson start time from table." );
		if ( !TimeOnly.TryParse( startStr, out var start ) )
			throw new ProcessingException( "Lesson start time extracted from table was invalid." );

		string endStr = match.GetGroup( "end" ) ?? throw new ProcessingException( "Failed to extract lesson end time from table." );
		if ( !TimeOnly.TryParse( endStr, out var end ) )
			throw new ProcessingException( "Lesson end time extracted from table was invalid." );

		return new( number, start, end );
	}
	// $1: 1, $2: 07:50, $3: 08:35
	[GeneratedRegex( @"<td[^<>]*?>(?<number>[\s\S]*?)<\/td>[^<>]*?<th[^<>]*?>(?<start>[\d:]*).*?;-.*?;(?<end>[\d:]*?)<\/th>", RegexOptions.None, regexTimeout )]
	private static partial Regex TimetableTimeRx();

	static string?[] ExtractLessonCell( string row )
	{
		return ExtractLessonCellInternal( row ).ToArray();

		static IEnumerable<string?> ExtractLessonCellInternal( string row )
		{
			var matches = TimetableLessonCellRx().Matches( row );

			foreach ( var match in matches.Cast<Match>() )
			{
				var cell = match.GetGroup( "cell" ) ?? throw new ProcessingException( "Failed to extract lesson cell from table." ); ;
				yield return cell.NullIfHtmlWhiteSpace();
			}
		}
	}
	// $1: cell (may be empty)
	[GeneratedRegex( @"<td class=""line1""[^<>]*?>(?<cell>[\s\S]*?)<\/td>", RegexOptions.None, regexTimeout )]
	private static partial Regex TimetableLessonCellRx();

	static LessonModel? ExtractLesson( string? cell, LessonTimeModel time )
	{
		if ( cell is null )
			return null;

		var match = TimetableLessonRx().Match( cell );

		if ( !match.Success )
			throw new ProcessingException( "Failed to extract lesson data from table cell." );

		var subject = match.GetGroup( "subject" ) ?? throw new ProcessingException( "Failed to extract subject from table cell." );
		var surname = match.GetGroup( "surname" ) ?? throw new ProcessingException( "Failed to extract surname from table cell." );
		var name = match.GetGroup( "name" ) ?? throw new ProcessingException( "Failed to extract name from table cell." );
		var group = match.GetGroup( "group" );
		var room = match.GetGroup( "room" );

		subject = subject.DecodeHtml();
		surname = surname.DecodeHtml();
		name = name.DecodeHtml();
		group = group.DecodeHtml();
		room = room.DecodeHtml();

		var substitution = ExtractSubstitution( cell );
		var isClassAbsence = CheckClassAbsence( cell );
		var isCancelled = CheckLessonCancellation( cell );

		return new LessonModel(
			subject,
			name, surname,
			group,
			room,
			time,
			substitution,
			isClassAbsence,
			isCancelled );
	}
	// $1: subject, $2: Surname, $3: Name, [$4: group] [$4: room]
	[GeneratedRegex( @"<b>\s*(?<subject>[^<>]*?)\s*<\/b>[^<]*?<br\/>- ?(?<surname>[\w\s.-]*)(?:.*?;)(?<name>[\w\s.-]*)(?:.*?;)?(?:(?:\((?<group>[\w\s.-]*)\))(?:.*?;)?)?(?:s\..*?;(?<room>[\w\s.-]*))?", RegexOptions.None, regexTimeout )]
	private static partial Regex TimetableLessonRx();

	static SubstitutionModel? ExtractSubstitution( string cell )
	{
		var bodyMatch = TimetableSubstitutionBodyRx().Match( cell );

		if ( !bodyMatch.Success )
			return null;

		var subMatch = TimetableSubstitutionDataRx().Match( bodyMatch.Groups[1].Value );

		var teacher = subMatch.GetGroup( "teacher" );
		var subject = subMatch.GetGroup( "subject" );
		var room = subMatch.GetGroup( "room" );

		return new SubstitutionModel( teacher, subject, room );
	}
	[GeneratedRegex( @"<a[^<>]*title=""([^""]*?)"">[\s\S]*?zastępstwo[\s\S]*?<\/a>", RegexOptions.None, regexTimeout )]
	private static partial Regex TimetableSubstitutionBodyRx();
	// $1: Teacher, $2: Subject
	[GeneratedRegex( @"<b>Nauczyciel:<\/b> (?<teacher>[^""]*?) ->[^""]*?<b>Przedmiot:<\/b> (?:(?<subject>[^<>]*) ->[^""]*|(?:[^<>]*))<br><b>Sala:<\/b> (?:\[brak\]|(?<room>[^<>]*)) ->", RegexOptions.None, regexTimeout )]
	private static partial Regex TimetableSubstitutionDataRx();
	//[GeneratedRegex(  @"<b>Nauczyciel:<\/b> (?<teacher>[\s\S]*?) ->[\s\S]*?<b>Przedmiot:<\/b>(?: (?<subject>[^<>]*?) ->)?", RegexOptions.None, regexTimeout  )]
	//private static partial Regex TimetableSubstitutionDataRx();

	static bool CheckClassAbsence( string cell )
	{
		return TimetableClassAbsenceRx().IsMatch( cell );
	}
	[GeneratedRegex( @"<div[^<>]*class=""[^""]*plan-lekcji-info[^""]*""[^<>]*>[^<>]*?nieobecność klasy[^<>]*?<\/div>", RegexOptions.None, regexTimeout )]
	private static partial Regex TimetableClassAbsenceRx();

	static bool CheckLessonCancellation( string cell )
	{
		return TimetableLessonCancellationRx().IsMatch( cell );
	}
	[GeneratedRegex( @"<div[^<>]*class=""[^""]*plan-lekcji-info[^""]*""[^<>]*>[^<>]*?odwołane[^<>]*?<\/div>", RegexOptions.None, regexTimeout )]
	private static partial Regex TimetableLessonCancellationRx();

	#endregion

}

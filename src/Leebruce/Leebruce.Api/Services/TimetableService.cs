using Leebruce.Domain.Timetable;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;

namespace Leebruce.Api.Services;

public interface ITimetableService
{
	Task<TimetableDayModel[]> GetTimetableAsync( ClaimsPrincipal principal );
	Task<TimetableDayModel[]> GetTimetableAsync( ClaimsPrincipal principal, DateOnly date );
}

public class TimetableService : ITimetableService
{
	private readonly ILbHelperService _lbHelper;
	private static readonly TimeSpan regexTimeout = TimeSpan.FromSeconds( 2 );

	public TimetableService( ILbHelperService lbHelper )
	{
		_lbHelper = lbHelper;
	}

	public async Task<TimetableDayModel[]> GetTimetableAsync( ClaimsPrincipal principal )
	{
		using HttpClientHandler? handler = _lbHelper.CreateHandler( principal );
		using HttpClient http = new( handler );

		using var resp = await http.GetAsync( "https://synergia.librus.pl/przegladaj_plan_lekcji" );

		return await ReadResponse( resp );
	}
	public async Task<TimetableDayModel[]> GetTimetableAsync( ClaimsPrincipal principal, DateOnly date )
	{
		using HttpClientHandler? handler = _lbHelper.CreateHandler( principal );
		using HttpClient http = new( handler );

		Dictionary<string, string> data = new() { ["tydzien"] = GetWeek( date ) };
		using FormUrlEncodedContent ctnt = new( data );

		using var resp = await http.PostAsync( "https://synergia.librus.pl/przegladaj_plan_lekcji", ctnt );

		return await ReadResponse( resp );
	}

	private static async Task<TimetableDayModel[]> ReadResponse( HttpResponseMessage resp )
	{
		string document = await resp.Content.ReadAsStringAsync();

		//if ( _validationService.IsTechnicalBreak( document ) )
		//{
		//	throw new TechnicalBreakException();
		//}

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
										   select Enumerable.Zip( column, times, ExtractLesson )
										   .TrimEnd( null ).ToArray()
										   ).ToArray();

		var days = Enumerable.Zip( dates, lessonColumns, ( date, lessons ) => new TimetableDayModel( date, lessons ) ).ToArray();

		return days;
	}

	#region Timetable regexes
	static (string header, string body) ExtractTable( string document )
	{
		var tableMatch = timetableTable.Match( document );
		var table = tableMatch.GetGroup( "table" ) ?? throw new ProcessingException( "Failed to extract table from document." );

		var headerBodyMatch = timetableHeaderBody.Match( table );
		var header = headerBodyMatch.GetGroup( "header" ) ?? throw new ProcessingException( "Failed to extract header from table." );
		var body = headerBodyMatch.GetGroup( "body" ) ?? throw new ProcessingException( "Failed to extract body from table." );

		return (header, body);
	}
	static readonly Regex timetableTable = new( @"<table class=""decorated plan-lekcji""[\s\S]*?>(?<table>[\s\S]*?)<\/table>", RegexOptions.None, regexTimeout );
	static readonly Regex timetableHeaderBody = new( @"<thead[\s\S]*?>(?<header>[\s\S]*?)<\/thead>\s*?(?<body>[\s\S]*?)\s*?<tfoot>", RegexOptions.None, regexTimeout );

	static IEnumerable<DateOnly> ExtractDates( string header )
	{
		var cellMatches = timetableHeaderDays.Matches( header );
		foreach ( Match cellMatch in cellMatches )
		{
			var dateStr = cellMatch.GetGroup( "date" ) ?? throw new ProcessingException( "Failed to extract date from header table header." );
			yield return DateOnly.TryParse( dateStr, out var date ) ? date : throw new ProcessingException( "Date extracted from header table was invalid." );
		}
	}
	// $1: DayOfWeek (in polish), $2: 2021-10-11
	static readonly Regex timetableHeaderDays = new( @"<td>(?<weekDay>[^<>]*?)<BR[^<>]*?\/>(?<date>[^<>]*?)<\/td>", RegexOptions.None, regexTimeout );//todo: remove (unused) day of week group

	static IEnumerable<string> ExtractRows( string body )
	{
		var headerMatch = timetableBodyRows.Matches( body );
		foreach ( Match match in headerMatch )
		{
			yield return match.GetGroup( "row" ) ?? throw new ProcessingException( "Failed to extract rows from table." );
		}
	}
	static readonly Regex timetableBodyRows = new( @"<tr class=""line1"">(?<row>[\s\S]*?)<\/tr>", RegexOptions.None, regexTimeout );

	static LessonTimeModel ExtractTime( string row )
	{
		var match = timetableTime.Match( row );

		string numStr = match.GetGroup( "number" ) ?? throw new ProcessingException( "Failed to extract lesson number from table." );
		if ( !int.TryParse( numStr, out var number ) )
		{
			throw new ProcessingException( "Lesson number extracted from table was invalid." );
		}

		string startStr = match.GetGroup( "start" ) ?? throw new ProcessingException( "Failed to extract lesson start time from table." );
		if ( !TimeOnly.TryParse( startStr, out var start ) )
		{
			throw new ProcessingException( "Lesson start time extracted from table was invalid." );
		}

		string endStr = match.GetGroup( "end" ) ?? throw new ProcessingException( "Failed to extract lesson end time from table." );
		if ( !TimeOnly.TryParse( endStr, out var end ) )
		{
			throw new ProcessingException( "Lesson end time extracted from table was invalid." );
		}

		return new( number, start, end );
	}
	// $1: 1, $2: 07:50, $3: 08:35
	static readonly Regex timetableTime = new( @"<td[^<>]*?>(?<number>[\s\S]*?)<\/td>[^<>]*?<th[^<>]*?>(?<start>[\d:]*).*?;-.*?;(?<end>[\d:]*?)<\/th>", RegexOptions.None, regexTimeout );

	static string?[] ExtractLessonCell( string row )
	{
		return ExtractLessonCellInternal( row ).ToArray();

		static IEnumerable<string?> ExtractLessonCellInternal( string row )
		{
			var matches = timetableLessonCell.Matches( row );

			foreach ( Match match in matches )
			{
				var cell = match.GetGroup( "cell" ) ?? throw new ProcessingException( "Failed to extract lesson cell from table." ); ;
				yield return cell.NullIfHtmlWhiteSpace();
			}
		}
	}
	// $1: cell (may be empty)
	static readonly Regex timetableLessonCell = new( @"<td class=""line1""[^<>]*?>(?<cell>[\s\S]*?)<\/td>", RegexOptions.None, regexTimeout );

	static LessonModel? ExtractLesson( string? cell, LessonTimeModel time )
	{
		if ( cell is null )
		{
			return null;
		}

		var match = timetableLesson.Match( cell );

		if ( !match.Success )
		{
			throw new ProcessingException( "Failed to extract lesson data from table cell." );
		}

		var subject = match.GetGroup( "subject" ) ?? throw new ProcessingException( "Failed to extract subject from table cell." );
		var surname = match.GetGroup( "surname" ) ?? throw new ProcessingException( "Failed to extract surname from table cell." );
		var name = match.GetGroup( "name" ) ?? throw new ProcessingException( "Failed to extract name from table cell." );
		var group = match.GetGroup( "group" );
		var room = match.GetGroup( "room" );

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
	static readonly Regex timetableLesson = new( @"<b>\s*(?<subject>[^<>]*?)\s*<\/b>[^<]*?<br\/>- ?(?<surname>[\w\s.-]*)(?:.*?;)(?<name>[\w\s.-]*)(?:.*?;)?(?:(?:\((?<group>[\w\s.-]*)\))(?:.*?;)?)?(?:s\..*?;(?<room>[\w\s.-]*))?", RegexOptions.None, regexTimeout );

	static SubstitutionModel? ExtractSubstitution( string cell )
	{
		var bodyMatch = timetableSubstitutionBody.Match( cell );

		if ( !bodyMatch.Success )
		{
			return null;
		}

		var subMatch = timetableSubstitutionData.Match( bodyMatch.Groups[1].Value );

		var teacher = subMatch.GetGroup( "teacher" );
		var subject = subMatch.GetGroup( "subject" );

		return new SubstitutionModel( teacher, subject );
	}
	static readonly Regex timetableSubstitutionBody = new( @"<a[^<>]*title=""([^""]*?)"">[\s\S]*?zastępstwo[\s\S]*?<\/a>", RegexOptions.None, regexTimeout );
	// $1: Teacher, $2: Subject
	static readonly Regex timetableSubstitutionData = new( @"<b>Nauczyciel:<\/b> (?<teacher>[\s\S]*?) ->[\s\S]*?<b>Przedmiot:<\/b>(?: (?<subject>[^<>]*?) ->)?", RegexOptions.None, regexTimeout );

	static bool CheckClassAbsence( string cell )
	{
		return timetableClassAbsence.IsMatch( cell );
	}
	static readonly Regex timetableClassAbsence = new( @"<div[^<>]*class=""[^""]*plan-lekcji-info[^""]*""[^<>]*>[^<>]*?nieobecność klasy[^<>]*?<\/div>", RegexOptions.None, regexTimeout );

	static bool CheckLessonCancellation( string cell )
	{
		return timetableLessonCancellation.IsMatch( cell );
	}
	static readonly Regex timetableLessonCancellation = new( @"<div[^<>]*class=""[^""]*plan-lekcji-info[^""]*""[^<>]*>[^<>]*?odwołane[^<>]*?<\/div>", RegexOptions.None, regexTimeout );

	#endregion

}

public static class RegexExtensions
{
	public static string? GetGroup( this Match match, string groupname )
	{
		var group = match.Groups[groupname];
		return !group.Success ? null : group.Value;
	}
	public static string? GetGroup( this Match match, int groupnum )
	{
		var group = match.Groups[groupnum];
		return !group.Success ? null : group.Value;
	}
}

public static class StringExtensions
{
	public static string? NullIfEmpty( this string? str )
	{
		return string.IsNullOrEmpty( str ) ? null : str;
	}
	public static string? NullIfWhiteSpace( this string? str )
	{
		return string.IsNullOrWhiteSpace( str ) ? null : str;
	}
	public static string? NullIfHtmlWhiteSpace( this string? str )
	{
		return string.IsNullOrWhiteSpace( HttpUtility.HtmlDecode( str ) ) ? null : str;
	}
}

public static class IEnumerableExtensions
{
	public static T[] TrimStart<T>( this T[] arr, T value )
	{
		var firstNotV = 0;

		while ( Equals( arr[firstNotV], value ) )
		{
			++firstNotV;
		}
		return arr[firstNotV..];
	}
	public static T[] TrimEnd<T>( this T[] arr, T value )
	{
		var lastNotV = arr.Length - 1;

		while ( Equals( arr[lastNotV], value ) )
		{
			--lastNotV;
		}
		return arr[..( lastNotV + 1 )];
	}
	public static T[] Trim<T>( this T[] arr, T value )
	{
		var firstNotV = 0;
		var lastNotV = arr.Length - 1;

		while ( Equals( arr[firstNotV], value ) )
		{
			++firstNotV;
		}
		while ( Equals( arr[lastNotV], value ) )
		{
			--lastNotV;
		}
		return arr[firstNotV..( lastNotV + 1 )];
	}
	public static T[][] Transpose<T>( this T[][] arr )
	{
		var colLen = arr.Length;
		var rowLen = arr[0].Length;

		// validate
		foreach ( var row in arr )
		{
			if ( row.Length != rowLen )
			{
				throw new ArgumentException( "Array rows were not same length", nameof( arr ) );
			}
		}

		// prepare empty
		var ret = new T[rowLen][];
		ret.Fill( () => new T[colLen] );

		// copy
		for ( int col = 0; col < rowLen; col++ )
		{
			for ( int row = 0; row < colLen; row++ )
			{
				ret[col][row] = arr[row][col];
			}
		}
		return ret;
	}
	public static void Fill<T>( this T[] arr, Func<T> factory )
	{
		for ( int i = 0; i < arr.Length; i++ )
		{
			arr[i] = factory();
		}
	}

	public static IEnumerable<T> TrimStart<T>( this IEnumerable<T> s, T value )
	{
		using var e = s.GetEnumerator();
		while ( e.MoveNext() && Equals( e.Current, value ) )
		{
		}
		do
		{
			yield return e.Current;
		} while ( e.MoveNext() );
	}
	public static IEnumerable<T> TrimEnd<T>( this IEnumerable<T> s, T value )
	{
		int cnt = 0;
		foreach ( var item in s )
		{
			if ( Equals( item, value ) )
			{
				++cnt;
			}
			else
			{
				while ( cnt > 0 )
				{
					--cnt;
					yield return value;
				}
				yield return item;
			}

		}
	}
	public static IEnumerable<T> Trim<T>( this IEnumerable<T> s, T value )
	{
		return s.TrimStart( value ).TrimEnd( value );
	}

}

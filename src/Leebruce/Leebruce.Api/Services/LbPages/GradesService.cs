using Leebruce.Api.Extensions;
using Leebruce.Domain.Grades;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services.LbPages;

public interface IGradesService
{
	Task<GradesPageModel> GetGradesAsync();
	Task<GradesPageModel> GetNewGradesAsync();
	Task<GradesPageModel> GetGradesWeekSummaryAsync();
	Task<GradesGraphRecordModel[]> GetGraphAsync();
}

public partial class GradesService : IGradesService
{
	private readonly ILbSiteClient _lbClient;
	private const int regexTimeout = 2000;

	public GradesService( ILbSiteClient lbClient )
	{
		_lbClient = lbClient;
	}

	public async Task<GradesPageModel> GetGradesAsync()
		=> await GetGradeWithPostBody( new Dictionary<string, string>()
		{
			["sortowanieOcen"] = "2",
			["zmiany_logowanie_wszystkie"] = "1",
		} );

	public async Task<GradesPageModel> GetNewGradesAsync()
		=> await GetGradeWithPostBody( new Dictionary<string, string>()
		{
			["sortowanieOcen"] = "2",
			["zmiany_logowanie"] = "1",
		} );

	public async Task<GradesPageModel> GetGradesWeekSummaryAsync()
		=> await GetGradeWithPostBody( new Dictionary<string, string>()
		{
			["sortowanieOcen"] = "2",
			["zmiany_logowanie_tydzien"] = "1",
		} );

	public async Task<GradesPageModel> GetGradeWithPostBody( Dictionary<string, string> data )
	{
		using FormUrlEncodedContent ctnt = new( data );
		var document = await _lbClient.PostContentAuthorized( "/przegladaj_oceny/uczen", ctnt );

		bool isByPercent = document.Contains( """<h3 class="center">Oceny punktowe</h3>""" );

		var table = GradesTableRx().Match( document ).GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract table from document." );

		var grades = ExtractSubjects( table, isByPercent, out var _ );//todo: return whether subjects list is complete

		return new( isByPercent, grades );
	}

	static SubjectGradesModel[] ExtractSubjects( string table, bool isByPercent, out bool isComplete )
	{
		isComplete = true;
		var matches = SubjectRx().Matches( table );
		List<SubjectGradesModel> subjects = new( matches.Count );

		foreach ( var subjectMatch in matches.Cast<Match>() )
		{
			try
			{
				subjects.Add( ProcessSubjectMatch( subjectMatch, isByPercent ) );
			}
			catch ( ProcessingException )
			{
				isComplete = false;
				//todo: log exception
			}
		}

		return subjects.ToArray();
	}
	static SubjectGradesModel ProcessSubjectMatch( Match subjectMatch, bool isByPercent )
	{
		// summary
		var summary = subjectMatch.GetGroup( "summary" )
			?? throw new ProcessingException( "Failed to extract subject summary from table." );

		var summaryMatch = SummaryRx().Match( summary );

		// comments
		var grades1 = summaryMatch.GetGroup( "grades1" )
			?? throw new ProcessingException( "Failed to extract term 1 grades from subject summary." );

		var grades2 = summaryMatch.GetGroup( "grades2" )
			?? throw new ProcessingException( "Failed to extract term 2 grades from subject summary." );

		Dictionary<string, string?> comments = new();
		foreach ( var (id, comment) in ExtractGradesComments( grades1 ).Concat( ExtractGradesComments( grades2 ) ) )
		{
			comments[id] = comment;
		}

		// details
		var detailsTable = subjectMatch.GetGroup( "table" )
			?? throw new ProcessingException( "Failed to extract subject details from table." );

		// subject
		var subjectName = summaryMatch.GetGroup( "subject" )
			?? throw new( "Failed to extract subject field from grades summary." );

		// grades
		var splitTable = GradeDetailsTermSplitRx().Split( detailsTable );
		var firstTermGrades = ExtractGrades( splitTable[0], comments, out var isComplete1 );

		bool isComplete2 = true;
		var secondTermGrades = splitTable.Length > 1
			? ExtractGrades( splitTable[1], comments, out isComplete2 )
			: Array.Empty<GradeModel>();

		// validate
		var averageTotalStr = summaryMatch.GetGroup( "averageTotal" )
			?? throw new( "Failed to extract total average field from grades summary." );

		bool isRepresentative = isComplete1 && isComplete2;

		var average = !isByPercent && double.TryParse( averageTotalStr, CultureInfo.InvariantCulture, out var a ) ? (double?)a : null;
		var percent = isByPercent && double.TryParse( averageTotalStr, CultureInfo.InvariantCulture, out var p ) ? (double?)p : null;

		return new SubjectGradesModel( subjectName, firstTermGrades, secondTermGrades, isRepresentative, average, percent );
	}

	static (string id, string? comment)[] ExtractGradesComments( string grades )
	{
		var matches = SummaryGradesRx().Matches( grades );
		List<(string id, string? comment)> comments = new( matches.Count );

		foreach ( var gradeMatch in matches.Cast<Match>() )
		{
			var id = gradeMatch.GetGroup( "link" );
			if ( id is null )
				continue;

			var comment = gradeMatch.GetGroup( "comment" );
			comments.Add( (id, comment) );
		}
		return comments.ToArray();
	}

	static GradeModel[] ExtractGrades( string subjectTable, Dictionary<string, string?> comments, out bool isComplete )
	{
		isComplete = true;
		var matches = SubjectGradesRx().Matches( subjectTable );
		List<GradeModel> grades = new( matches.Count );

		foreach ( var gradeRowMatch in matches.Cast<Match>() )
		{
			var gradeMatch = GradeInfoRx().Match( gradeRowMatch.Value );

			try
			{
				grades.Add( ProcessGradeMatch( gradeMatch, comments ) );
			}
			catch ( ProcessingException )
			{
				isComplete = false;
				//todo: log exception
			}
		}

		return grades.ToArray();
	}
	static GradeModel ProcessGradeMatch( Match gradeMatch, Dictionary<string, string?> comments )
	{
		static ProcessingException ExceptionFor( string field ) => new( $"Failed to extract {field} field from grade." );
		static ProcessingException FormatExceptionFor( string field ) => new( $"{field} field extracted from grade was invalid." );

		// grade
		var gradeStr = gradeMatch.GetGroup( "grade" )
			?? gradeMatch.GetGroup( "gradeAlt" )
			?? throw ExceptionFor( "grade" );

		int? grade = null;
		SpecialGrade? specialGrade = null;
		string? verySpecialGrade = null;
		if ( int.TryParse( gradeStr, out var gradeNotNull ) )
			grade = gradeNotNull;
		else
			specialGrade = gradeStr switch
			{
				"+" => SpecialGrade.Plus,
				"-" => SpecialGrade.Minus,
				"np" => SpecialGrade.Unprepared,
				_ => null,
				//_ => throw FormatExceptionFor( "Grade" ),
			};
		if ( specialGrade is null )
		{
			verySpecialGrade = gradeStr;
		}

		// weight
		var weightStr = gradeMatch.GetGroup( "weight" );

		int? weight;
		if ( string.IsNullOrWhiteSpace( weightStr ) || weightStr is "-" )
			weight = null;
		else if ( int.TryParse( weightStr, out var weightNotNull ) )
			weight = weightNotNull;
		else
			throw FormatExceptionFor( "Weight" );

		// date
		var dateStr = gradeMatch.GetGroup( "date" )
			?? throw ExceptionFor( "date" );

		if ( !DateOnly.TryParse( dateStr, out var date ) )
			throw FormatExceptionFor( "Date" );

		// count
		var countStr = gradeMatch.GetGroup( "count" );
		var count = countStr switch
		{
			"aktywne" => true,
			"nieaktywne" => false,
			null => true,
			_ => throw FormatExceptionFor( "Count to average" )
		};

		// color
		var color = gradeMatch.GetGroup( "color" )
			?? throw ExceptionFor( "color" );

		// category
		var category = gradeMatch.GetGroup( "category" )
			?? throw ExceptionFor( "category" );

		//// resit
		//var resit = gradeMatch.GetGroup( "resit" )
		//	?? throw ExceptionFor( "resit" );

		// teacher
		var teacher = gradeMatch.GetGroup( "teacher" )
			?? throw ExceptionFor( "teacher" );

		//added by
		var addedBy = gradeMatch.GetGroup( "addedBy" )
			?? throw ExceptionFor( "addedBy" );

		// id
		var id = GradeIdRx().Match( gradeMatch.Value ).GetGroup( "link" );

		// comment
		var comment = id is null ? null : comments[id];

		return new GradeModel( grade, specialGrade, verySpecialGrade, count, weight, category, comment, date, teacher, addedBy, color );
	}


	// $1: content
	[GeneratedRegex( @"<table class=""decorated stretch""\s*>([\s\S]*)<\/table>", RegexOptions.None, regexTimeout )]
	private static partial Regex GradesTableRx();
	// $summary; $table
	//[GeneratedRegex( @"<tr class=""(line\d)"">\s*(?<summary><td class='center micro screen-only'><img src=""/images/tree_colapsed\.png"" id=""przedmioty_(\d*)_node[\s\S]*?)<\/tr><tr class=""\1""[^>]*id=""przedmioty_\2""[^>]*>\s*<td[^>]*><table class=""stretch"">(?<table>[\s\S]*?)<\/table><\/td><\/tr>", RegexOptions.None, regexTimeout )]
	[GeneratedRegex( """<tr class=['"]line\d['"]>\s*(?<summary><td class='center micro screen-only'>\s*<img src="\/images\/tree_colapsed\.png" id="przedmioty_([\w_]*)_node[\s\S]*?)<\/tr>\s*<tr class="line\d"[^>]*id="przedmioty_\1"[^>]*>\s*(?:<td>&nbsp;<\/td>\s*)?<td[^>]*>\s*<table class="stretch">(?<table>[\s\S]*?)<\/table>\s*<\/td>\s*<\/tr>""", RegexOptions.None, regexTimeout )]
	private static partial Regex SubjectRx();
	// $subject; $grades1; $average1; $suggestedTerm1; $term1; $grades2; $average2; $term2; $averageTotal; $suggestedTotal; $total
	//[GeneratedRegex( @"<td class='center micro screen-only'>[\s\S]*?<\/td>\s*<td\s*>(?<subject>[\s\S]*?)<\/td><td\s*>(?<grades1>[\s\S]*?)<\/td><td class=""right"">(?<average1>[\s\S]*?)<\/td>(?:<td class=""center""\s*>(?<suggestedTerm1>[\s\S]*?)<\/td>)?<td class=""center""\s*>(?<term1>[\s\S]*?)<\/td><td\s*>(?<grades2>[\s\S]*?)<\/td><td\s*class=""right"">(?<average2>[\s\S]*?)<\/td>(?:<td class=""center""\s*>(?<suggestedTerm2>[\s\S]*?)<\/td>)?<td class=""center""\s*>(?<term2>[\s\S]*?)<\/td><td\s*class=""right""\s*>(?<averageTotal>[\s\S]*?)<\/td><td class=""center""\s*>(?<suggestedTotal>[\s\S]*?)<\/td>(?:<td class=""center""\s*>(?<total>[\s\S]*?)<\/td>)?", RegexOptions.None, regexTimeout )]
	[GeneratedRegex( """<td class='center micro screen-only'>[\s\S]*?<\/td>\s*<td\s*>(?<subject>[^<]*?)(?:<a[^>]*><[^>]*><\/a>)?<\/td><td\s*(?:class=""\s*)?>(?<grades1>[\s\S]*?)<\/td><td class="right">(?<average1>[\s\S]*?)<\/td>(?:<td class="center"\s*>(?<suggestedTerm1>[\s\S]*?)<\/td>)?<td\s*class="center"\s*>(?<term1>[\s\S]*?)<\/td><td\s*>(?<grades2>[\s\S]*?)<\/td><td\s*class="right">(?<average2>[\s\S]*?)<\/td>(?:<td\s*class="center"\s*>(?<suggestedTerm2>[\s\S]*?)<\/td>)?<td\s*class="center"\s*>(?<term2>[\s\S]*?)<\/td><td\s*class="right"\s*>(?<averageTotal>[\s\S]*?)<\/td><td\s*class="center"\s*>(?<suggestedTotal>[\s\S]*?)<\/td>(?:<td\s*class="center"\s*>(?<total>[\s\S]*?)<\/td>)?""", RegexOptions.None, regexTimeout )]
	private static partial Regex SummaryRx();
	// $comment; $link
	//[GeneratedRegex( @"<span[^>]*>\s*<a title=""[^""]*Komentarz: (?<comment>[^""]*)"" class=""ocena"" href=""\/przegladaj_oceny\/szczegoly\/(?<link>[^""]*)"" >[^<]*<\/a><\/span>", RegexOptions.None, regexTimeout )]
	[GeneratedRegex( """<span[^>]*>\s*<a title="[^"]*Komentarz: (?<comment>[^"]*)" (?:class="ocena" )?href="\/przegladaj_oceny(?:_punktowe)?\/szczegoly\/(?<link>[^"]*)" >[^<]*<\/a><\/span>""", RegexOptions.None, regexTimeout )]
	private static partial Regex SummaryGradesRx();
	// row
	[GeneratedRegex( @"<tr class=""line1 detail-grades""[\s\S]*?<\/tr>", RegexOptions.None, regexTimeout )]
	private static partial Regex SubjectGradesRx();
	// $color; $grade; $category; $date; $teacher; $count: "aktywne" - yes, "nieaktywne" - no; $weight; $resit; $addedBy
	//[GeneratedRegex( @"<tr class=""line1 detail-grades"" style=""background-color: #(?<color>\w*);""><td class=""center"">(?<grade>[^<\s]*)<\/td><td class=""center"">[\s\S]*?<\/td><td>(?<category>[\s\S]*?)<\/td><td class=""center"">(?<date>[\s\S]*?)<\/td><td\s*>(?<teacher>[\s\S]*?)<\/td><td class=""center"">[\s\S]*?""\/images\/(?<count>[\s\S]*?)\.png[^>]*><\/td><td class=""right""\s*>(?<weight>[^<]*?)<\/td><td class=""center""\s*>(?<resit>[\s\S]*?)<\/td><td\s*>(?<addedBy>[\s\S]*?)<\/td><\/tr>", RegexOptions.None, regexTimeout )]
	//[GeneratedRegex( """<tr class="line1 detail-grades" style="background-color: #(?<color>\w*);"><td class="center"\s*>(?<grade>[^<\s]*)<\/td><td class="center"\s*>[\s\S]*?<\/td><td\s*(?:class=""\s*)?>(?<category>[\s\S]*?)<\/td><td class="center"\s*>(?<date>[\s\S]*?)<\/td><td\s*(?:class=""\s*)?>(?<teacher>[\s\S]*?)<\/td>(?:<td class="center">[\s\S]*?"\/images\/(?<count>[\s\S]*?)\.png[^>]*><\/td><td class="right"\s*>(?<weight>[^<]*?)<\/td><td class="center"\s*>(?<resit>[\s\S]*?)<\/td>)?<td\s*(?:class=""\s*)?>(?<addedBy>[\s\S]*?)<\/td><\/tr>""", RegexOptions.None, regexTimeout )]
	[GeneratedRegex( """<tr class="line1 detail-grades" style="background-color: #(?<color>\w*);"><td class="center"\s*>(?:(?<grade>[^<\s]*)<\/td><td class="center"\s*>|<a[^>]*>(?<gradeAlt>[^<\s]*)<\/a>)([\s\S]*?)<\/td><td\s*(?:class=""\s*)?>(?<category>[\s\S]*?)<\/td><td class="center"\s*>(?<date>[\s\S]*?)<\/td><td\s*(?:class=""\s*)?>(?<teacher>[\s\S]*?)<\/td>(?:<td class="center">[\s\S]*?"\/images\/(?<count>[\s\S]*?)\.png[^>]*><\/td><td class="right"\s*>(?<weight>[^<]*?)<\/td><td class="center"\s*>(?<resit>[\s\S]*?)<\/td>)?<td\s*(?:class=""\s*)?>(?<addedBy>[\s\S]*?)<\/td><\/tr>""", RegexOptions.None, regexTimeout )]
	private static partial Regex GradeInfoRx();
	// $link
	[GeneratedRegex( @"'\/komentarz_oceny\/\d\/(?<link>[\s\S]*?)'", RegexOptions.None, regexTimeout )]
	private static partial Regex GradeIdRx();
	[GeneratedRegex( """<td colspan="[^"]*" class="center">\s*Okres 2\s*<\/td>""", RegexOptions.None, regexTimeout )]
	private static partial Regex GradeDetailsTermSplitRx();

	public async Task<GradesGraphRecordModel[]> GetGraphAsync()
	{
		var document = await _lbClient.GetContentAuthorized( "/uczen/graph_ajax.php?type=wykres_sredniej&classId=74264&userId=1792335&_=1656850225307" );

		var graphMatches = GradeGraphRx().Matches( document );
		List<GradesGraphRecordModel> graph = new( graphMatches.Count );

		foreach ( var barMatch in graphMatches.Cast<Match>() )
		{
			var monthStr = barMatch.GetGroup( "month" )
				?? throw new ProcessingException( "Failed to extract month from document." );

			if ( !DateOnly.TryParse( monthStr, out var month ) )
				throw new ProcessingException( "Month extracted from document was in invalid." );

			var perUserStr = barMatch.GetGroup( "perUser" )
				?? throw new ProcessingException( "Failed to extract per-user average from document." );

			if ( !float.TryParse( perUserStr, CultureInfo.InvariantCulture, out var perUser ) )
				throw new ProcessingException( "Per-user average extracted from document was in invalid." );

			var perLevelStr = barMatch.GetGroup( "perLevel" )
				?? throw new ProcessingException( "Failed to extract per-level average from document." );

			if ( !float.TryParse( perLevelStr, CultureInfo.InvariantCulture, out var perLevel ) )
				throw new ProcessingException( "Per-level average extracted from document was in invalid." );

			graph.Add( new( month, perUser, perLevel ) );
		}

		return graph.ToArray();
	}

	// $month; $perUser; $perLevel
	[GeneratedRegex( @"{\s*columnGradeAverangeGraphDiv:""(?<month>[^""]*)"",\s*x0:\s*(?<perUser>[\d\.]*),\s*x1:\s*(?<perLevel>[\d\.]*)}", RegexOptions.None, regexTimeout )]
	private static partial Regex GradeGraphRx();

}

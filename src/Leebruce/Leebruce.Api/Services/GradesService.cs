using Leebruce.Api.Extensions;
using Leebruce.Domain;
using Leebruce.Domain.Grades;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services;

public interface IGradesService
{
	Task<SubjectGradesModel[]> GetGradesAsync( ClaimsPrincipal principal );
	Task<GradesGraphRecordModel[]> GetGraphAsync( ClaimsPrincipal principal );
}

public class GradesService : IGradesService
{
	private readonly ILbHelperService _lbHelper;
	private static readonly TimeSpan regexTimeout = TimeSpan.FromSeconds( 2 );

	public GradesService( ILbHelperService lbHelper )
	{
		_lbHelper = lbHelper;
	}

	public async Task<SubjectGradesModel[]> GetGradesAsync( ClaimsPrincipal principal )
	{
		using HttpClientHandler handler = _lbHelper.CreateHandler( principal );
		using HttpClient http = new( handler );

		using var resp = await http.GetAsync( "https://synergia.librus.pl/przegladaj_oceny/uczen" );
		string document = await resp.Content.ReadAsStringAsync();

		if ( _lbHelper.IsUnauthorized( document ) )
			throw new NotAuthorizedException();

		var table = gradesTableRx.Match( document ).GetGroup( 1 )
			?? throw new ProcessingException( "Failed to extract table from document." );

		return ExtractSubjects( table, out var _ );//todo: return whether subjects list is complete
	}

	static SubjectGradesModel[] ExtractSubjects( string table, out bool isComplete )
	{
		isComplete = true;
		var matches = subjectRx.Matches( table );
		List<SubjectGradesModel> subjects = new( matches.Count );

		foreach ( Match subjectMatch in matches )
		{
			try
			{
				subjects.Add( ProcessSubjectMatch( subjectMatch ) );
			}
			catch ( ProcessingException )
			{
				isComplete = false;
				//todo: log exception
			}
		}

		return subjects.ToArray();
	}
	static SubjectGradesModel ProcessSubjectMatch( Match subjectMatch )
	{
		// summary
		var summary = subjectMatch.GetGroup( "summary" )
			?? throw new ProcessingException( "Failed to extract subject summary from table." );

		var summaryMatch = summaryRx.Match( summary );

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
		var grades = ExtractGrades( detailsTable, comments, out var isComplete );

		// validate
		var averageTotalStr = summaryMatch.GetGroup( "averageTotal" )
			?? throw new( "Failed to extract total average field from grades summary." );

		bool isRepresentative = isComplete && ValidateGrades( grades, averageTotalStr );

		return new SubjectGradesModel( subjectName, grades, isRepresentative );
	}
	static bool ValidateGrades( GradeModel[] grades, string averageTotalStr )
	{
		static ProcessingException FormatExceptionFor( string field ) => new( $"{field} field extracted from grades summary was invalid." );

		if ( averageTotalStr == "-" )
			return false;

		if ( !double.TryParse( averageTotalStr.Replace( '.', ',' ), out var averageTotal ) )
			throw FormatExceptionFor( "Total average" );

		var toAverage = grades
			.Where( g => g.CountToAverage == true )
			.Where( g => g.Value is not null and > 0 ) // filter zeros as they are not counted but keep the CountToAverage of grade they substitute
			.Select( g => (g.Value!.Value, Weight: g.Weight!.Value) );

		var averageTotalCalculated = (float)toAverage.Sum( g => g.Value * g.Weight ) / toAverage.Sum( g => g.Weight );

		return Math.Round( averageTotalCalculated, 2 ) == averageTotal;
	}

	static (string id, string? comment)[] ExtractGradesComments( string grades )
	{
		var matches = summaryGradesRx.Matches( grades );
		List<(string id, string? comment)> comments = new( matches.Count );

		foreach ( Match gradeMatch in matches )
		{
			var link = gradeMatch.GetGroup( "link" );
			if ( link is null )
				continue;

			var comment = gradeMatch.GetGroup( "comment" );
			comments.Add( (link, comment) );
		}
		return comments.ToArray();
	}

	static GradeModel[] ExtractGrades( string subjectTable, Dictionary<string, string?> comments, out bool isComplete )
	{
		isComplete = true;
		var matches = subjectGradesRx.Matches( subjectTable );
		List<GradeModel> grades = new( matches.Count );

		foreach ( Match gradeRowMatch in matches )
		{
			var gradeMatch = gradeInfoRx.Match( gradeRowMatch.Value );

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
			?? throw ExceptionFor( "grade" );

		int? grade = null;
		SpecialGrade? specialGrade = null;
		if ( int.TryParse( gradeStr, out var gradeNotNull ) )
			grade = gradeNotNull;
		else
			specialGrade = gradeStr switch
			{
				"+" => SpecialGrade.Plus,
				"-" => SpecialGrade.Minus,
				"np" => SpecialGrade.Unprepared,
				_ => throw FormatExceptionFor( "Grade" ),
			};

		// weight
		var weightStr = gradeMatch.GetGroup( "weight" )
			?? throw ExceptionFor( "weight" );

		int? weight;
		if ( string.IsNullOrWhiteSpace( weightStr ) )
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
		var countStr = gradeMatch.GetGroup( "count" )
			?? throw ExceptionFor( "count" );
		var count = countStr switch
		{
			"aktywne" => true,
			"nieaktywne" => false,
			_ => throw FormatExceptionFor( "Count to average" )
		};

		// color
		var color = gradeMatch.GetGroup( "color" )
			?? throw ExceptionFor( "color" );

		// category
		var category = gradeMatch.GetGroup( "category" )
			?? throw ExceptionFor( "category" );

		// resit
		var resit = gradeMatch.GetGroup( "resit" )
			?? throw ExceptionFor( "resit" );

		// teacher
		var teacher = gradeMatch.GetGroup( "teacher" )
			?? throw ExceptionFor( "teacher" );

		//added by
		var addedBy = gradeMatch.GetGroup( "addedBy" )
			?? throw ExceptionFor( "addedBy" );

		// id
		var link = gradeIdRx.Match( gradeMatch.Value ).GetGroup( "link" );
		var id = link?.ToUrlBase64();

		// comment
		var comment = link is null ? null : comments[link];

		return new GradeModel( id, grade, specialGrade, count, weight, category, comment, date, teacher, addedBy );
	}


	// $1: content
	static readonly Regex gradesTableRx = new( @"<table class=""decorated stretch"">([\s\S]*)<\/table>", RegexOptions.None, regexTimeout );
	// $summary; $table
	static readonly Regex subjectRx = new( @"<tr class=""(line\d)"">\s*(?<summary><td class='center micro screen-only'><img src=""/images/tree_colapsed\.png"" id=""przedmioty_(\d*)_node[\s\S]*?)<\/tr><tr class=""\1""[^>]*id=""przedmioty_\2""[^>]*>\s*<td[^>]*><table class=""stretch"">(?<table>[\s\S]*?)<\/table><\/td><\/tr>", RegexOptions.None, regexTimeout );
	// $subject; $grades1; $average1; $suggestedTerm1; $term1; $grades2; $average2; $term2; $averageTotal; $suggestedTotal; $total
	static readonly Regex summaryRx = new( @"<td class='center micro screen-only'>[\s\S]*?<\/td>\s*<td\s*>(?<subject>[\s\S]*?)<\/td><td\s*>(?<grades1>[\s\S]*?)<\/td><td class=""right"">(?<average1>[\s\S]*?)<\/td><td class=""center""\s*>(?<suggestedTerm1>[\s\S]*?)<\/td><td class=""center""\s*>(?<term1>[\s\S]*?)<\/td><td\s*>(?<grades2>[\s\S]*?)<\/td><td\s*class=""right"">(?<average2>[\s\S]*?)<\/td><td class=""center""\s*>(?<term2>[\s\S]*?)<\/td><td\s*class=""right""\s*>(?<averageTotal>[\s\S]*?)<\/td><td class=""center""\s*>(?<suggestedTotal>[\s\S]*?)<\/td><td class=""center""\s*>(?<total>[\s\S]*?)<\/td>", RegexOptions.None, regexTimeout );
	// $comment; $link
	static readonly Regex summaryGradesRx = new( @"<span[^>]*>\s*<a title=""[^""]*Komentarz: (?<comment>[^""]*)"" class=""ocena"" href=""\/przegladaj_oceny\/szczegoly\/(?<link>[^""]*)"" >[^<]*<\/a><\/span>", RegexOptions.None, regexTimeout );
	// row
	static readonly Regex subjectGradesRx = new( @"<tr class=""line1 detail-grades""[\s\S]*?<\/tr>", RegexOptions.None, regexTimeout );
	// $color; $grade; $category; $date; $teacher; $count: "aktywne" - yes, "nieaktywne" - no; $weight; $resit; $addedBy
	static readonly Regex gradeInfoRx = new( @"<tr class=""line1 detail-grades"" style=""background-color: #(?<color>\w*);""><td class=""center"">(?<grade>[^<\s]*)<\/td><td class=""center"">[\s\S]*?<\/td><td>(?<category>[\s\S]*?)<\/td><td class=""center"">(?<date>[\s\S]*?)<\/td><td\s*>(?<teacher>[\s\S]*?)<\/td><td class=""center"">[\s\S]*?""\/images\/(?<count>[\s\S]*?)\.png[^>]*><\/td><td class=""right""\s*>(?<weight>[^<]*?)<\/td><td class=""center""\s*>(?<resit>[\s\S]*?)<\/td><td\s*>(?<addedBy>[\s\S]*?)<\/td><\/tr>", RegexOptions.None, regexTimeout );
	// $link
	static readonly Regex gradeIdRx = new( @"'\/komentarz_oceny\/1\/(?<link>[\s\S]*?)'", RegexOptions.None, regexTimeout );

	public async Task<GradesGraphRecordModel[]> GetGraphAsync( ClaimsPrincipal principal )
	{
		using HttpClientHandler handler = _lbHelper.CreateHandler( principal );
		using HttpClient http = new( handler );

		using var resp = await http.GetAsync( "https://synergia.librus.pl/uczen/graph_ajax.php?type=wykres_sredniej&classId=74264&userId=1792335&_=1656850225307" );
		string document = await resp.Content.ReadAsStringAsync();

		if ( _lbHelper.IsUnauthorized( document ) )
			throw new NotAuthorizedException();

		var graphMatches = gradeGraphRx.Matches( document );
		List<GradesGraphRecordModel> graph = new( graphMatches.Count );

		foreach ( Match barMatch in graphMatches )
		{
			var monthStr = barMatch.GetGroup( "month" )
				?? throw new ProcessingException( "Failed to extract month from document." );

			if ( !DateOnly.TryParse( monthStr, out var month ) )
				throw new ProcessingException( "Month extracted from document was in invalid." );

			var perUserStr = barMatch.GetGroup( "perUser" )
				?? throw new ProcessingException( "Failed to extract per-user average from document." );

			if ( !float.TryParse( perUserStr.Replace( '.', ',' ), out var perUser ) )
				throw new ProcessingException( "Per-user average extracted from document was in invalid." );

			var perLevelStr = barMatch.GetGroup( "perLevel" )
				?? throw new ProcessingException( "Failed to extract per-level average from document." );

			if ( !float.TryParse( perLevelStr.Replace( '.', ',' ), out var perLevel ) )
				throw new ProcessingException( "Per-level average extracted from document was in invalid." );

			graph.Add( new( month, perUser, perLevel ) );
		}

		return graph.ToArray();
	}

	// $month; $perUser; $perLevel
	static readonly Regex gradeGraphRx = new( @"{\s*columnGradeAverangeGraphDiv:""(?<month>[^""]*)"",\s*x0:\s*(?<perUser>[\d\.]*),\s*x1:\s*(?<perLevel>[\d\.]*)}", RegexOptions.None, regexTimeout );

}

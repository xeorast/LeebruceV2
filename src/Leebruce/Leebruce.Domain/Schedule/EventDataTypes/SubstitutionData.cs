using System.Text.RegularExpressions;

namespace Leebruce.Domain.Schedule.EventDataTypes;

//Zastępstwo z
public record SubstitutionData(
	string Who,
	string? Subject,
	int? LessonNo )
	: IEventData
{
	private static readonly TimeSpan regexTimeout = TimeSpan.FromSeconds( 2 );
	public static SubstitutionData? TryFrom( string[] segments )
	{
		if ( segments.Length > 1 )
			return null;

		var match = substitutionRx.Match( segments[0] );
		if ( !match.Success )
			return null;

		var who = match.Groups["who"].Value;
		var noStr = match.Groups["no"].Value;
		var subject = match.Groups["subject"].Value;

		int? lessonNo = null;
		if ( noStr is not null )
		{
			if ( int.TryParse( noStr, out var result ) )
				lessonNo = result;
			else
				throw new FormatException();//todo: different exception;
		}

		return new SubstitutionData( who, subject, lessonNo );
	}
	static readonly Regex substitutionRx = new( @"Zastępstwo z (?<who>.*) na lekcji nr: (?<no>.*) \((?<subject>.*)\)", RegexOptions.None, regexTimeout );
}

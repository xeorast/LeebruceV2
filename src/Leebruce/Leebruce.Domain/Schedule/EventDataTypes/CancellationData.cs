using System.Text.RegularExpressions;

namespace Leebruce.Domain.Schedule.EventDataTypes;

//Odwołane zajęcia
public record CancellationData(
	string Who,
	string? Subject,
	int? LessonNo )
	: IEventData
{
	private static readonly TimeSpan regexTimeout = TimeSpan.FromSeconds( 2 );
	public static CancellationData? TryFrom( string[] segments )
	{
		if ( segments[0].Trim() != "Odwołane zajęcia" )
			return null;

		var match = cancellationRx.Match( segments[1] );
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

		return new CancellationData( who, subject, lessonNo );
	}
	static readonly Regex cancellationRx = new( @"(?<who>.*) na lekcji nr: (?<no>.*) \((?<subject>.*)\)", RegexOptions.None, regexTimeout );
}

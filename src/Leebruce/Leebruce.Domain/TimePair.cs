using System.Diagnostics.CodeAnalysis;

namespace Leebruce.Domain;

public struct TimePair
{
	public TimeOnly Start { get; set; }
	public TimeOnly End { get; set; }

	public TimePair( TimeOnly start, TimeOnly end )
	{
		Start = start;
		End = end;
	}

	public static bool TryParse( [NotNullWhen( true )] string? str, out TimePair result )
	{
		if ( str is null )
			return false;

		var parts = str.Split( "-" );
		if ( parts.Length < 2 )
			return false;

		if ( !TimeOnly.TryParse( parts[0], out var start )
			|| !TimeOnly.TryParse( parts[1], out var end ) )
			return false;

		result = new( start, end );
		return true;
	}
}
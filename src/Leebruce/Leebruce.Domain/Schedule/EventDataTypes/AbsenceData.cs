namespace Leebruce.Domain.Schedule.EventDataTypes;

//Nieobecność:
public record AbsenceData(
	string Who,
	string? CharacterClass,
	TimePair? Time )
	: IEventData
{
	public static AbsenceData? TryFrom( string[] segments )
	{
		if ( segments[0] != "Nieobecność:" )
			return null;

		var whoParts = segments[1].Split( ": ", 2 );

		var who = whoParts.Length > 1 ? whoParts[1] : whoParts[0];
		var cls = whoParts.Length > 1 ? whoParts[0] : null;

		var timeStr = segments.Where( x => x.StartsWith( "Godziny: " ) )
			.FirstOrDefault()?["Godziny: ".Length..];

		TimePair? time = null;
		if ( timeStr is not null )
		{
			if ( TimePair.TryParse( timeStr.Replace( " do ", "-" ), out var result ) )
				time = result;
			else
				throw new FormatException();//todo: different exception;
		}

		return new AbsenceData( who, cls, time );
	}
}

namespace Leebruce.Domain.Schedule.EventDataTypes;

//Nieobecność klasy:
public record ClassAbsenceData(
	string Class,
	string? When )
	: IEventData
{
	public static ClassAbsenceData? TryFrom( string[] segments )
	{
		if ( segments[0] != "Nieobecność klasy:" )
			return null;

		var when = segments.Length > 2 ? segments[1] : null;
		var who = segments.Length > 2 ? segments[2] : segments[1];

		return new ClassAbsenceData( who, when );
	}
}

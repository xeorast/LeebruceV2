namespace Leebruce.Domain.Schedule.EventDataTypes;

//<unrecognized>
public record UnrecognizedData(
	string Value )
	: IEventData
{
	public static UnrecognizedData From( string[] segments )
	{
		return new UnrecognizedData( string.Join( "\n", segments ) );
	}
}

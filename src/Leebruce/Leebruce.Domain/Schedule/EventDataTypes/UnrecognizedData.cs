namespace Leebruce.Domain.Schedule.EventDataTypes;

//<unrecognized>
public record UnrecognizedData(
	string Value )
	: IEventData;

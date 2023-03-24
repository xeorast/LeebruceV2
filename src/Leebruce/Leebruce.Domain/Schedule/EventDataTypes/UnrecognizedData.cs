namespace Leebruce.Domain.Schedule.EventDataTypes;

//<unrecognized>
public record class UnrecognizedData(
	string Value )
	: IEventData;

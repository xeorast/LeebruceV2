namespace Leebruce.Domain.Schedule.EventDataTypes;

// terminarz/szczegoly_wolne/\d*
public record class FreeDayData(
	string Who,
	string? What )
	: IEventData;

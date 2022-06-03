namespace Leebruce.Domain.Schedule.EventDataTypes;

//Nieobecność:
public record AbsenceData(
	string Who,
	string? CharacterClass,
	TimePair? Time )
	: IEventData;

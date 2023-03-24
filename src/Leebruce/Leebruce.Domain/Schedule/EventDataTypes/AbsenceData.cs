namespace Leebruce.Domain.Schedule.EventDataTypes;

//Nieobecność:
public record class AbsenceData(
	string Who,
	string? CharacterClass,
	TimePair? Time )
	: IEventData;

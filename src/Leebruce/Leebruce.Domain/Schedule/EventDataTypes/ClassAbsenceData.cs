namespace Leebruce.Domain.Schedule.EventDataTypes;

//Nieobecność klasy:
public record ClassAbsenceData(
	string Class,
	string? When )
	: IEventData;

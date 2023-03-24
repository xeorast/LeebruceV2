namespace Leebruce.Domain.Schedule.EventDataTypes;

//Nieobecność klasy:
public record class ClassAbsenceData(
	string Class,
	string? When )
	: IEventData;

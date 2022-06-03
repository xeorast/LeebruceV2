namespace Leebruce.Domain.Schedule.EventDataTypes;

//Zastępstwo z
public record SubstitutionData(
	string Who,
	string? Subject,
	int? LessonNo )
	: IEventData;

namespace Leebruce.Domain.Schedule.EventDataTypes;

//Zastępstwo z
public record class SubstitutionData(
	string Who,
	string? Subject,
	int? LessonNo )
	: IEventData;

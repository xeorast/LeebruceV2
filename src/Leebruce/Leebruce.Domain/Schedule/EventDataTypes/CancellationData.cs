namespace Leebruce.Domain.Schedule.EventDataTypes;

//Odwołane zajęcia
public record class CancellationData(
	string Who,
	string? Subject,
	int? LessonNo )
	: IEventData;

namespace Leebruce.Domain.Schedule.EventDataTypes;

//Odwołane zajęcia
public record CancellationData(
	string Who,
	string? Subject,
	int? LessonNo )
	: IEventData;

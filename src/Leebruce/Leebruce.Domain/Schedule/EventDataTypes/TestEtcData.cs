namespace Leebruce.Domain.Schedule.EventDataTypes;

//Nr lekcji: 1\nKartkówka
public record TestEtcData(
	string? Subject,
	string? Creator,
	string? What,
	string? Description,
	int? LessonNo,
	string? Room,
	string? Group )
	: IEventData;

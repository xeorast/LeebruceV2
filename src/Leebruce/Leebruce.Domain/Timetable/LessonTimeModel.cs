namespace Leebruce.Domain.Timetable;

public record LessonTimeModel(
	int Number,
	TimeOnly Start,
	TimeOnly End );

namespace Leebruce.Domain.Timetable;

public record class LessonTimeModel(
	int Number,
	TimeOnly Start,
	TimeOnly End );

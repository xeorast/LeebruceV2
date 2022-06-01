namespace Leebruce.Domain.Timetable;

public record TimetableDayModel(
	DateOnly Date,
	LessonModel?[] Lessons);

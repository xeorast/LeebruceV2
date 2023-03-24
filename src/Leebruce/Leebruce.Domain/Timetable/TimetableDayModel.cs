namespace Leebruce.Domain.Timetable;

public record class TimetableDayModel(
	DateOnly Date,
	LessonModel?[] Lessons);

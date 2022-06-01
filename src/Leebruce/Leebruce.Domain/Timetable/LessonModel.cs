namespace Leebruce.Domain.Timetable;

public record LessonModel(
	string Subject,
	string TeacherName,
	string TeacherSurname,
	string? Group,
	string? Room,
	LessonTimeModel Time,
	SubstitutionModel? Substitution,
	bool ClassAbsence,
	bool IsCancelled );

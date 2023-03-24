namespace Leebruce.Domain.Timetable;

public record class LessonModel(
	string Subject,
	string TeacherName,
	string TeacherSurname,
	string? Group,
	string? Room,
	LessonTimeModel Time,
	SubstitutionModel? Substitution,
	bool ClassAbsence,
	bool IsCancelled );

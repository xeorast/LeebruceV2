namespace Leebruce.Domain.Timetable;

public record class SubstitutionModel(
	string? OriginalTeacher,
	string? OriginalSubject,
	string? OriginalRoom );

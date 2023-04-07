namespace Leebruce.Domain.Grades;

public record class GradesPageModel(
	bool IsByPercent,
	SubjectGradesModel[] Subjects );

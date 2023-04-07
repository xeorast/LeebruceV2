using Leebruce.Domain.Grades;

namespace Leebruce.Domain;

public record class GradesPageModel(
	bool IsByPercent,
	SubjectGradesModel[] Subjects );

public record class SubjectGradesModel(
	string Subject,
	GradeModel[] FirstTermGrades,
	GradeModel[] SecondTermGrades,
	bool IsRepresentative,
	double? Average,
	double? Percent );

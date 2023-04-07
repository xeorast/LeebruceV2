namespace Leebruce.Domain.Grades;

public record class SubjectGradesModel(
	string Subject,
	GradeModel[] FirstTermGrades,
	GradeModel[] SecondTermGrades,
	bool IsRepresentative,
	double? Average,
	double? Percent );

using Leebruce.Domain.Grades;

namespace Leebruce.Domain;

public record class SubjectGradesModel(
	string Subject,
	GradeModel[] Grades,
	bool IsRepresentative );

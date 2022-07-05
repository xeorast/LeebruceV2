using Leebruce.Domain.Grades;

namespace Leebruce.Domain;

public record SubjectGradesModel(
	string Subject,
	GradeModel[] Grades,
	bool IsRepresentative );

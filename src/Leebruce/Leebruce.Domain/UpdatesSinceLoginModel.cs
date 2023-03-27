namespace Leebruce.Domain;

public record class UpdatesSinceLoginModel(
	int NewGrades,
	int NewAbsences,
	int NewMessages,
	int NewAnnouncements,
	int NewEvents,
	int NewHomeworks );

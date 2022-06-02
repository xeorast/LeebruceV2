namespace Leebruce.Domain;

public record AnnouncementModel(
	string Title,
	DateOnly Date,
	string Author,
	string Content );

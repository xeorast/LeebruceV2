namespace Leebruce.Domain;

public record class AnnouncementModel(
	string Title,
	DateOnly Date,
	string Author,
	string Content );

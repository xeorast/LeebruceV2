namespace Leebruce.Domain.Models;

public record MessageModel(
	string Subject,
	string Author,
	string CharacterClass,
	DateTimeOffset Date,
	string Content );

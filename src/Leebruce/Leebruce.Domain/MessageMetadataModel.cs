namespace Leebruce.Domain.Models;

public record MessageMetadataModel(
	string Subject,
	string Author,
	DateTimeOffset Date,
	string Id );

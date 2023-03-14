namespace Leebruce.Domain;

public record MessageMetadataModel(
	string Subject,
	string Author,
	DateTimeOffset Date,
	bool HasAttachments,
	bool IsUnread,
	string Id );

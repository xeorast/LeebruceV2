namespace Leebruce.Domain;

public record class MessageMetadataModel(
	string Subject,
	string Author,
	DateTimeOffset Date,
	bool HasAttachments,
	bool IsUnread,
	string Id );

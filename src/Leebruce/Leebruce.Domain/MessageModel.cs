namespace Leebruce.Domain;

public record MessageModel(
	string Subject,
	string Author,
	string CharacterClass,
	DateTimeOffset Date,
	string Content,
	AttachmentModel[] Attachments );

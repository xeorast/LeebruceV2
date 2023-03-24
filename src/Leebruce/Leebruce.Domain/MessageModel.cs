namespace Leebruce.Domain;

public record class MessageModel(
	string Subject,
	string Author,
	string CharacterClass,
	DateTimeOffset Date,
	string Content,
	AttachmentModel[] Attachments );

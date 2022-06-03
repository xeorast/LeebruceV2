namespace Leebruce.Domain.Schedule;

public record ScheduleEvent(
	string? Id,
	DateTimeOffset? DateAdded,
	EventData Data );

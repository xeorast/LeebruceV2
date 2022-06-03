using Leebruce.Domain.Schedule;

namespace Leebruce.Domain;

public record ScheduleDay(
	DateOnly Day,
	ScheduleEvent[] Events );

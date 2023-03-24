using Leebruce.Domain.Schedule;

namespace Leebruce.Domain;

public record class ScheduleDay(
	DateOnly Day,
	ScheduleEvent[] Events );

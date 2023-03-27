using Leebruce.Domain.Schedule.EventDataTypes;
using System.Text.Json.Serialization;

namespace Leebruce.Domain.Schedule;

public record class ScheduleEvent(
	string? Id,
	DateTimeOffset? DateAdded,
	AbsenceData? AbsenceData = null,
	CancellationData? CancellationData = null,
	ClassAbsenceData? ClassAbsenceData = null,
	FreeDayData? FreeDayData = null,
	SubstitutionData? SubstitutionData = null,
	TestEtcData? TestEtcData = null,
	UnrecognizedData? UnrecognizedData = null,
	string? Error = null )
{
	#region ignore properties when null
	private const JsonIgnoreCondition whenNull = JsonIgnoreCondition.WhenWritingNull;

	[JsonIgnore( Condition = whenNull )]
	public string? Id { get; init; } = Id;
	[JsonIgnore( Condition = whenNull )]
	public DateTimeOffset? DateAdded { get; init; } = DateAdded;
	[JsonIgnore( Condition = whenNull )]
	public AbsenceData? AbsenceData { get; init; } = AbsenceData;
	[JsonIgnore( Condition = whenNull )]
	public CancellationData? CancellationData { get; init; } = CancellationData;
	[JsonIgnore( Condition = whenNull )]
	public ClassAbsenceData? ClassAbsenceData { get; init; } = ClassAbsenceData;
	[JsonIgnore( Condition = whenNull )]
	public FreeDayData? FreeDayData { get; init; } = FreeDayData;
	[JsonIgnore( Condition = whenNull )]
	public SubstitutionData? SubstitutionData { get; init; } = SubstitutionData;
	[JsonIgnore( Condition = whenNull )]
	public TestEtcData? TestEtcData { get; init; } = TestEtcData;
	[JsonIgnore( Condition = whenNull )]
	public UnrecognizedData? UnrecognizedData { get; init; } = UnrecognizedData;
	[JsonIgnore( Condition = whenNull )]
	public string? Error { get; init; } = Error;

	#endregion

	public static ScheduleEvent From( string? id, DateTimeOffset? dateAdded, IEventData data )
	{
		return data switch
		{
			AbsenceData d => new ScheduleEvent( id, dateAdded, AbsenceData: d ),
			CancellationData d => new ScheduleEvent( id, dateAdded, CancellationData: d ),
			ClassAbsenceData d => new ScheduleEvent( id, dateAdded, ClassAbsenceData: d ),
			FreeDayData d => new ScheduleEvent( id, dateAdded, FreeDayData: d ),
			SubstitutionData d => new ScheduleEvent( id, dateAdded, SubstitutionData: d ),
			TestEtcData d => new ScheduleEvent( id, dateAdded, TestEtcData: d ),
			UnrecognizedData d => new ScheduleEvent( id, dateAdded, UnrecognizedData: d ),
			_ => throw new NotImplementedException( $"Unrecognized implementation of {nameof( IEventData )}." ),
		};
	}

}

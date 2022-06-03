using Leebruce.Domain.Schedule.EventDataTypes;
using System.Text.Json.Serialization;

namespace Leebruce.Domain.Schedule;

public record EventData(
	AbsenceData? AbsenceData = null,
	CancellationData? CancellationData = null,
	ClassAbsenceData? ClassAbsenceData = null,
	SubstitutionData? SubstitutionData = null,
	TestEtcData? TestEtcData = null,
	UnrecognizedData? UnrecognizedData = null,
	string? Error = null )
{
	#region ignore properties when null
	private const JsonIgnoreCondition whenNull = JsonIgnoreCondition.WhenWritingNull;

	[JsonIgnore( Condition = whenNull )]
	public AbsenceData? AbsenceData { get; init; } = AbsenceData;
	[JsonIgnore( Condition = whenNull )]
	public CancellationData? CancellationData { get; init; } = CancellationData;
	[JsonIgnore( Condition = whenNull )]
	public ClassAbsenceData? ClassAbsenceData { get; init; } = ClassAbsenceData;
	[JsonIgnore( Condition = whenNull )]
	public SubstitutionData? SubstitutionData { get; init; } = SubstitutionData;
	[JsonIgnore( Condition = whenNull )]
	public TestEtcData? TestEtcData { get; init; } = TestEtcData;
	[JsonIgnore( Condition = whenNull )]
	public UnrecognizedData? UnrecognizedData { get; init; } = UnrecognizedData;
	[JsonIgnore( Condition = whenNull )]
	public string? Error { get; init; } = Error;

	#endregion

	public static EventData From( IEventData data )
	{
		return data switch
		{
			AbsenceData d => new EventData( AbsenceData: d ),
			CancellationData d => new EventData( CancellationData: d ),
			ClassAbsenceData d => new EventData( ClassAbsenceData: d ),
			SubstitutionData d => new EventData( SubstitutionData: d ),
			TestEtcData d => new EventData( TestEtcData: d ),
			UnrecognizedData d => new EventData( UnrecognizedData: d ),
			_ => throw new NotImplementedException( $"Unrecognized implementation of {nameof( IEventData )}." ),
		};
	}
}
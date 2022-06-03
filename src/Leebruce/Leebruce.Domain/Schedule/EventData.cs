using Leebruce.Domain.Schedule.EventDataTypes;

namespace Leebruce.Domain.Schedule;

public record EventData(
	AbsenceData? AbsenceData = null,
	CancellationData? CancellationData = null,
	ClassAbsenceData? ClassAbsenceData = null,
	SubstitutionData? SubstitutionData = null,
	TestEtcData? TestEtcData = null,
	UnrecognizedData? UnrecognizedData = null )
{
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
			_ => throw new NotImplementedException( $"Unrecognized implementation od {nameof( IEventData )}." ),
		};
	}
}
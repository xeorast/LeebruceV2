using System.Text.Json;

namespace Leebruce.Domain.Converters.Json;

public class TimeOnlyJsonConverter : ToStringJsonConverter<TimeOnly>
{
	protected override TimeOnly FromString( string? str )
	{
		if ( str is null )
			throw new JsonException( "Cannot convert null to time." );

		if ( !TimeOnly.TryParse( str, out var time ) )
			throw new JsonException( "Invalid time format." );

		return time;
	}
	protected override string ToString( TimeOnly value )
	{
		return value.ToLongTimeString();
	}

}

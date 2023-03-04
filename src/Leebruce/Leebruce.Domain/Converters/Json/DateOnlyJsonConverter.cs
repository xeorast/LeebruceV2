using System.Text.Json;

namespace Leebruce.Domain.Converters.Json;

public class DateOnlyJsonConverter : ToStringJsonConverter<DateOnly>
{
	protected override DateOnly FromString( string? str )
	{
		if ( str is null )
			throw new JsonException( "Cannot convert null to date." );

		if ( !DateOnly.TryParse( str, out var date ) )
			throw new JsonException( "Invalid date format." );

		return date;
	}
	protected override string ToString( DateOnly value )
	{
		return value.ToString("o");
	}

}

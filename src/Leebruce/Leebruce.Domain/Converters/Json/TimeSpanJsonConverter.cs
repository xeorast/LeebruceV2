using System.Text.Json;
using System.Xml;

namespace Leebruce.Domain.Converters.Json;

public class TimeSpanJsonConverter : ToStringJsonConverter<TimeSpan>
{
	protected override TimeSpan FromString( string? str )
	{
		if ( str is null )
			throw new JsonException( "Cannot convert null to timespan." );

		try
		{
			return XmlConvert.ToTimeSpan( str );
		}
		catch ( FormatException )
		{
			throw new JsonException( "Invalid timespan format." );
		}
	}
	protected override string ToString( TimeSpan value )
	{
		return XmlConvert.ToString( value );
	}

}

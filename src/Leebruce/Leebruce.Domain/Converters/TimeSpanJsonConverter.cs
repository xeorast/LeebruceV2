using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace Leebruce.Domain.Converters
{
	public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
	{
		public override TimeSpan Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			if ( typeToConvert != typeof( TimeSpan ) )
				throw new ArgumentException( "Can only parse System.TimeSpan.", nameof( typeToConvert ) );

			var value = reader.GetString()
				?? throw new JsonException( "Cannot convert null to timespan." );

			try
			{
				return XmlConvert.ToTimeSpan( value );
			}
			catch ( FormatException )
			{
				throw new JsonException( "Invalid timespan format." );
			}
		}

		public override void Write( Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options )
		{
			writer.WriteStringValue( XmlConvert.ToString( value ) );
		}
	}
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Leebruce.Domain.Converters.Json
{
	public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
	{
		public override TimeOnly Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			if ( typeToConvert != typeof( TimeSpan ) )
				throw new ArgumentException( "Can only parse System.TimeOnly.", nameof( typeToConvert ) );

			var value = reader.GetString()
				?? throw new JsonException( "Cannot convert null to time." );

			if ( !TimeOnly.TryParse( value, out var time ) )
				throw new JsonException( "Invalid time format." );

			return time;
		}

		public override void Write( Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options )
		{
			writer.WriteStringValue( value.ToLongTimeString() );
		}
	}
}

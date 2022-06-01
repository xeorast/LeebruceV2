using System.Text.Json;
using System.Text.Json.Serialization;

namespace Leebruce.Domain.Converters.Json
{
	public class DateOnlyJsonConverter : JsonConverter<DateOnly>
	{
		public override DateOnly Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
		{
			if ( typeToConvert != typeof( TimeSpan ) )
				throw new ArgumentException( "Can only parse System.DateOnly.", nameof( typeToConvert ) );

			var value = reader.GetString()
				?? throw new JsonException( "Cannot convert null to date." );

			if ( !DateOnly.TryParse( value, out var date ) )
				throw new JsonException( "Invalid date format." );

			return date;
		}

		public override void Write( Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options )
		{
			writer.WriteStringValue( value.ToShortDateString() );
		}
	}
}

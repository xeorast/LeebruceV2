using System.Text.Json;
using System.Text.Json.Serialization;

namespace Leebruce.Domain.Converters.Json;

public abstract class ToStringJsonConverter<T> : JsonConverter<T>
{
	public override T? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		var value = reader.GetString();
		return FromString( value );
	}

	public override void Write( Utf8JsonWriter writer, T value, JsonSerializerOptions options )
	{
		writer.WriteStringValue( ToString( value ) );
	}

	public override T ReadAsPropertyName( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		return Read( ref reader, typeToConvert, options )
			?? throw new JsonException( "Property name cannot be null" );
	}

	public override void WriteAsPropertyName( Utf8JsonWriter writer, T? value, JsonSerializerOptions options )
	{
		writer.WritePropertyName( ToString( value ) );
	}

	protected abstract T? FromString( string? str );
	protected abstract string ToString( T? value );

}

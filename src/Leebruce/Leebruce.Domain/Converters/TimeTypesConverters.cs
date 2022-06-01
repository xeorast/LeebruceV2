using Leebruce.Domain.Converters.Json;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Leebruce.Domain.Converters;

public static class TimeTypesConverters
{
	public static void Register()
	{
		_ = TypeDescriptor.AddAttributes( typeof( DateOnly ), new TypeConverterAttribute( typeof( DateOnlyConverter ) ) );
		_ = TypeDescriptor.AddAttributes( typeof( TimeOnly ), new TypeConverterAttribute( typeof( TimeOnlyConverter ) ) );
	}

	public static void RegisterTimeTypesConverters( this IList<JsonConverter> c )
	{
		c.Add( new DateOnlyJsonConverter() );
		c.Add( new TimeOnlyJsonConverter() );
		c.Add( new TimeSpanJsonConverter() );
	}

}

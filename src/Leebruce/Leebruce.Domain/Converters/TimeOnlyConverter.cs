using System.ComponentModel;
using System.Globalization;

namespace Leebruce.Domain.Converters;

public class TimeOnlyConverter : TypeConverter
{
	public override bool CanConvertFrom( ITypeDescriptorContext? context, Type sourceType )
	{
		if ( sourceType == typeof( string ) )
		{
			return true;
		}

		return base.CanConvertFrom( context, sourceType );
	}

	public override object? ConvertFrom( ITypeDescriptorContext? context, CultureInfo? culture, object value )
	{
		if ( value is string timeStr )
		{
			if ( !TimeOnly.TryParse( timeStr.Trim(), out var time ) )
			{
				throw new FormatException( $"String '{timeStr}' was not recognized as a valid TimeOnly." );
			}
			return time;
		}

		return base.ConvertFrom( context, culture, value );
	}

	public override bool CanConvertTo( ITypeDescriptorContext? context, Type? destinationType )
	{
		if ( destinationType == typeof( string ) )
		{
			return true;
		}

		return base.CanConvertTo( context, destinationType );
	}

	public override object? ConvertTo( ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType )
	{
		if ( destinationType == typeof( string ) && value is TimeOnly time )
		{
			return time.ToString( culture );
		}

		return base.ConvertTo( context, culture, value, destinationType );
	}

}

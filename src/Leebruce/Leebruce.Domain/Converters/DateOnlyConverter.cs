using System.ComponentModel;
using System.Globalization;

namespace Leebruce.Domain.Converters;

public class DateOnlyConverter : TypeConverter
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
		if ( value is string dateStr )
		{
			if ( !DateOnly.TryParse( dateStr.Trim(), out var date ) )
			{
				throw new FormatException( $"String '{dateStr}' was not recognized as a valid DateOnly." );
			}
			return date;
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
		if ( destinationType == typeof( string ) && value is DateOnly date )
		{
			return date.ToString( culture );
		}

		return base.ConvertTo( context, culture, value, destinationType );
	}

}

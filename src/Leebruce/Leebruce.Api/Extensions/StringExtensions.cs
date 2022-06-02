using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Leebruce.Api.Extensions;

public static class StringExtensions
{
	public static string? NullIfEmpty( this string? str )
	{
		return string.IsNullOrEmpty( str ) ? null : str;
	}
	public static string? NullIfWhiteSpace( this string? str )
	{
		return string.IsNullOrWhiteSpace( str ) ? null : str;
	}
	public static string? NullIfHtmlWhiteSpace( this string? str )
	{
		return string.IsNullOrWhiteSpace( HttpUtility.HtmlDecode( str ) ) ? null : str;
	}

	public static string ToUrlBase64( this string str, Encoding? encoding = null )
	{
		return Base64UrlTextEncoder.Encode( ( encoding ?? Encoding.UTF8 ).GetBytes( str ) );
	}
	public static string FromUrlBase64( string base64, Encoding? encoding = null )
	{
		return ( encoding ?? Encoding.UTF8 ).GetString( Base64UrlTextEncoder.Decode( base64 ) );
	}

	[return: NotNullIfNotNull( "str" )]
	public static string? DecodeHtml( this string? str )
	{
		if ( str is null )
			return null;

		str = str.Replace( "\r\n", "" );
		str = str.Replace( "\n", "" );
		str = brRx.Replace( str, "\n" );
		return HttpUtility.HtmlDecode( str );
	}
	static readonly Regex brRx = new( @"<br\s*?\/>" );

}

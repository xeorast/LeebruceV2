using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Leebruce.Api.Extensions;

public static partial class StringExtensions
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

	[return: NotNullIfNotNull( nameof( str ) )]
	public static string? DecodeHtml( this string? str )
	{
		if ( str is null )
			return null;

		str = str.Replace( "\r\n", "" );
		str = str.Replace( "\n", "" );
		str = brRx().Replace( str, "\n" );
		return HttpUtility.HtmlDecode( str );
	}
	[GeneratedRegex( @"<br\s*?\/>" )]
	private static partial Regex brRx();

	[return: NotNullIfNotNull( nameof( str ) )]
	public static string? NormalizeHtmlAnchors( this string? str )
	{
		if ( str is null )
			return null;

		var segments = anchorRx().Split( str );
		StringBuilder sb = new();
		for ( int i = 0; i < segments.Length; i += 3 )
		{
			var encodedSegment = HttpUtility.HtmlEncode( segments[i] );
			_ = sb.Append( encodedSegment );
			if ( segments.Length > i + 1 )
			{
				var anchor = @$"<a href=""{segments[i + 1]}"" target=""_blank"" rel=""noopener noreferrer"">{segments[i + 2]}</a>";
				_ = sb.Append( anchor );
			}
		}

		return sb.ToString();

	}

	[return: NotNullIfNotNull( nameof( str ) )]
	public static string? WrapLinksInHtmlAnchors( this string? str )
	{
		if ( str is null )
			return null;

		return UrlWithSchemeRx().Replace( str, """<a href="$0" target="_blank" rel="noopener noreferrer">$0</a>""" );
	}

	[return: NotNullIfNotNull( nameof( str ) )]
	public static string? EncodeHtml( this string? str )
	{
		if ( str is null )
			return null;

		// encode line breaks
		var paragraphs = paragraphRx().Split( str );
		StringBuilder sb = new();
		foreach ( var paragraph in paragraphs )
		{
			_ = sb.Append( $"<p>{paragraph}</p>" );
		}
		str = sb.ToString();
		str = str.Replace( "\r\n", "<br/>" );
		str = str.Replace( "\n", "<br/>" );
		return str;
	}

	[GeneratedRegex( @"https?:\/\/(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b(?:[\(\)\-a-zA-Z0-9@:%_\+.~#?&\/\/=]*)" )]
	private static partial Regex UrlWithSchemeRx();
	[GeneratedRegex( @"(?<!\n\n)\n\n" )]
	private static partial Regex paragraphRx();
	[GeneratedRegex( @"<a[^<]*href=""([^""]*)""[^<]*>([^<]*)</a>" )]
	private static partial Regex anchorRx();

}

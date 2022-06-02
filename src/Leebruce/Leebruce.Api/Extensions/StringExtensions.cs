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
}

using System.Text.RegularExpressions;

namespace Leebruce.Api.Extensions;

public static class RegexExtensions
{
	public static string? GetGroup( this Match match, string groupname )
	{
		var group = match.Groups[groupname];
		return !group.Success ? null : group.Value;
	}
	public static string? GetGroup( this Match match, int groupnum )
	{
		var group = match.Groups[groupnum];
		return !group.Success ? null : group.Value;
	}
}

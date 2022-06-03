using System.Text.RegularExpressions;

namespace Leebruce.Domain.Schedule.EventDataTypes;

//Nr lekcji: 1\nKartkówka
public record TestEtcData(
	string? Subject,
	string? Creator,
	string? What,
	string? Description,
	int? LessonNo,
	string? Room,
	string? Group )
	: IEventData
{
	private static readonly TimeSpan regexTimeout = TimeSpan.FromSeconds( 2 );

	public static TestEtcData? TryFrom( string[] segments, Dictionary<string, string> additionalData )
	{
		if ( segments.Length < 2 )
			return null;

		if ( !segments[0].StartsWith( "Nr lekcji: " ) )
			return null;

		var noStr = segments[0]["Nr lekcji: ".Length..];

		int? lessonNo = null;
		if ( noStr is not null )
		{
			if ( int.TryParse( noStr, out var result ) )
				lessonNo = result;
			else
				throw new FormatException();//todo: different exception;
		}

		_ = additionalData.TryGetValue( "Nauczyciel", out var creator );
		_ = additionalData.TryGetValue( "Opis", out var description );

		string? subject = null;
		string? what = null;
		string? room = null;
		string? group = null;
		List<string> unknown = new();

		foreach ( var item in segments.Skip( 1 ) )
		{
			if ( item.StartsWith( "Sala: " ) )
			{
				room = item["Sala: ".Length..];
				continue;
			}

			var whatMatch = whatRx.Match( item );
			if ( whatMatch.Success )
			{
				subject = whatMatch.Groups["subject"].Value;
				what = whatMatch.Groups["what"].Value;
				continue;
			}

			// zone of no format, basically im relaying on order
			if ( ( what ??= item ) == item )
				continue;

			if ( ( group ??= item ) == item )
				continue;

			unknown.Add( item );
		}

		//todo: something when `unknown` is not empty

		return new TestEtcData( subject, creator, what, description, lessonNo, room, group );
	}
	static readonly Regex whatRx = new( @"<span class=\""przedmiot\"">(?<subject>.*)</span>, (?<what>.*)", RegexOptions.None, regexTimeout );

}

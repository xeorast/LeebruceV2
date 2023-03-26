using Leebruce.Domain;
using Leebruce.Domain.Schedule;
using Leebruce.Domain.Schedule.EventDataTypes;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services.LbPages;

public partial class ScheduleServiceHelper
{
	private const int regexTimeout = 2000;

	public static AbsenceData? TryFromAbsenceData( string[] segments )
	{
		if ( segments[0] != "Nieobecność:" )
			return null;

		var whoParts = segments[1].Split( ": ", 2 );

		var who = whoParts.Length > 1 ? whoParts[1] : whoParts[0];
		var cls = whoParts.Length > 1 ? whoParts[0] : null;

		var timeStr = segments.Where( x => x.StartsWith( "Godziny: " ) )
			.FirstOrDefault()?["Godziny: ".Length..];

		TimePair? time = null;
		if ( timeStr is not null )
		{
			if ( TimePair.TryParse( timeStr.Replace( " do ", "-" ), out var result ) )
				time = result;
			else
				throw new ProcessingException( "Absence time was invalid." );
		}

		return new AbsenceData( who, cls, time );
	}

	public static ClassAbsenceData? TryFromClassAbsenceData( string[] segments )
	{
		if ( segments[0] is not ( "Nieobecność klasy:" or ", Nieobecność klasy:"/* (xd) */ ) )
			return null;

		var when = segments.Length > 2 ? segments[1] : null;
		var who = segments.Length > 2 ? segments[2] : segments[1];

		return new ClassAbsenceData( who, when );
	}

	public static SubstitutionData? TryFromSubstitutionData( string[] segments )
	{
		if ( segments.Length > 1 )
			return null;

		if ( !segments[0].StartsWith( "Zastępstwo z " ) )
			return null;

		var match = SubstitutionRx().Match( segments[0] );
		if ( !match.Success )
			throw new ProcessingException( "Unexpected substitution data format." );

		var who = match.Groups["who"].Value;
		var noStr = match.Groups["no"].Value;
		var subject = match.Groups["subject"].Value;

		int? lessonNo = null;
		if ( noStr is not null )
		{
			if ( int.TryParse( noStr, out var result ) )
				lessonNo = result;
			else
				throw new ProcessingException( "Substitution lesson number was invalid." );
		}

		return new SubstitutionData( who, subject, lessonNo );
	}
	[GeneratedRegex( @"Zastępstwo z (?<who>.*) na lekcji nr: (?<no>.*) \((?<subject>.*)\)", RegexOptions.None, regexTimeout )]
	private static partial Regex SubstitutionRx();

	public static CancellationData? TryFromCancellationData( string[] segments )
	{
		if ( segments[0].Trim() != "Odwołane zajęcia" )
			return null;

		if ( segments.Length < 2 )
			throw new ProcessingException( "Cancellation data missing." );

		var match = CancellationRx().Match( segments[1] );
		if ( !match.Success )
			throw new ProcessingException( "Unexpected cancellation data format." );

		var who = match.Groups["who"].Value;
		var noStr = match.Groups["no"].Value;
		var subject = match.Groups["subject"].Value;

		int? lessonNo = null;
		if ( noStr is not null )
		{
			if ( int.TryParse( noStr, out var result ) )
				lessonNo = result;
			else
				throw new ProcessingException( "Cancelled lesson number was invalid." );
		}

		return new CancellationData( who, subject, lessonNo );
	}
	[GeneratedRegex( @"(?<who>.*) na lekcji nr: (?<no>.*) \((?<subject>.*)\)", RegexOptions.None, regexTimeout )]
	private static partial Regex CancellationRx();

	public static TestEtcData? TryFromTestEtcData( string[] segments, Dictionary<string, string> additionalData )
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
				throw new ProcessingException( "Test lesson number was invalid." );
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

			var whatMatch = WhatRx().Match( item );
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
	[GeneratedRegex( @"<span class=\""przedmiot\"">(?<subject>.*)</span>, (?<what>.*)", RegexOptions.None, regexTimeout )]
	private static partial Regex WhatRx();

	public static UnrecognizedData FromUnrecognizedData( string[] segments, bool showWhateverGotCaptured )
	{
		var value = showWhateverGotCaptured ? string.Join( "\n", segments ) : "Unknown event type.";
		return new UnrecognizedData( value );
	}

	public static ScheduleEvent ErrorEvent( string error )
	{
		return new ScheduleEvent( null, null, Error: error );
	}

}

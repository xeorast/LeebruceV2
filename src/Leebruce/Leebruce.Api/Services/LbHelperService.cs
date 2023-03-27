using Leebruce.Api.Extensions;
using Leebruce.Domain;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbHelperService
{
	string GetUserName( string lbPage );
	bool IsUnauthorized( string document );
	UpdatesSinceLoginModel GetNotifications( string lbPage );
}

public partial class LbHelperService : ILbHelperService
{
	public bool IsUnauthorized( string document )
	{
		return document.Contains( "<h2 class=\"inside\">Brak dostępu</h2>" )
			&& document.Contains( @"https:\/\/synergia.librus.pl\/loguj" );
	}

	public string GetUserName( string lbPage )
	{
		var match = userNameRx().Match( lbPage );
		if ( !match.Success )
		{
			throw new ArgumentException( "Provided page is not valid lb page and does not contain username.", nameof( lbPage ) );
		}

		return match.GetGroup( 1 )
			?? throw new Exception();
	}

	[GeneratedRegex( @"<div id=""user-section""[\s\S]*?jesteś zalogowany jako: <b>[\W\s]*([\w\s-.]*)" )]
	private static partial Regex userNameRx();

	public UpdatesSinceLoginModel GetNotifications( string lbPage )
	{
		var rowMatch = MainNavRowRx().Match( lbPage );
		if ( !rowMatch.Success )
		{
			throw new ArgumentException( "Page provided to extract notifications does not have nav row.", nameof( lbPage ) );
		}

		var gradesStr = GradesCountRx().Match( rowMatch.Value ).GetGroup( 1 );
		var absencesStr = AbsencesCountRx().Match( rowMatch.Value ).GetGroup( 1 );
		var messagesStr = MessagesCountRx().Match( rowMatch.Value ).GetGroup( 1 );
		var announcementsStr = AnnouncementsCountRx().Match( rowMatch.Value ).GetGroup( 1 );
		var eventsStr = EventsCountRx().Match( rowMatch.Value ).GetGroup( 1 );
		var homeworksStr = HomeworksCountRx().Match( rowMatch.Value ).GetGroup( 1 );

		_ = int.TryParse( gradesStr, out var grades );
		_ = int.TryParse( absencesStr, out var absences );
		_ = int.TryParse( messagesStr, out var messages );
		_ = int.TryParse( announcementsStr, out var announcements );
		_ = int.TryParse( eventsStr, out var events );
		_ = int.TryParse( homeworksStr, out var homeworks );

		return new( grades, absences, messages, announcements, events, homeworks );
	}

	#region Notification regexes

	[GeneratedRegex( """<div id="graphic-menu">\s*<ul>[\s\S]*?<\/ul>""" )]
	private static partial Regex MainNavRowRx();

	[GeneratedRegex( """<a title="Liczba ocen dodanych od ostatniego logowania: (\d*)"\s*href="\/przegladaj_oceny\/uczen" id="icon-oceny">""" )]
	private static partial Regex GradesCountRx();

	//untested
	[GeneratedRegex( """Frekwencja<\/a>\s*<a class="button counter blue">(\d*)""" )]
	private static partial Regex AbsencesCountRx();

	//untested
	[GeneratedRegex( """Wiadomości<\/a>\s*<a class="button counter">(\d*)""" )]
	private static partial Regex MessagesCountRx();

	//untested
	[GeneratedRegex( """Ogłoszenia<\/a>\s*<a class="button counter">(\d*)""" )]
	private static partial Regex AnnouncementsCountRx();

	[GeneratedRegex( """Terminarz<\/a>\s*<a class="button counter blue">(\d*)""" )]
	private static partial Regex EventsCountRx();

	//untested
	[GeneratedRegex( """Zadania domowe<\/a>\s*<a class="button counter blue">(\d*)""" )]
	private static partial Regex HomeworksCountRx();

	#endregion

}

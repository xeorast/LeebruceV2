using Leebruce.Api.Extensions;
using Leebruce.Domain;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbHelperService
{
	// todo: change GetUserNameAsync to operate on fetched site rather than fetch on its own
	Task<string> GetUserNameAsync( ClaimsPrincipal user );
	bool IsUnauthorized( string document );
	UpdatesSinceLoginModel GetNotifications( string lbPage );
}

public partial class LbHelperService : ILbHelperService
{
	private readonly ILbUserService _lbUser;
	private readonly HttpClient _http;

	public LbHelperService( ILbUserService lbUser, HttpClient http )
	{
		_lbUser = lbUser;
		_http = http;
	}

	public bool IsUnauthorized( string document )
	{
		return document.Contains( "<h2 class=\"inside\">Brak dostępu</h2>" )
			&& document.Contains( @"https:\/\/synergia.librus.pl\/loguj" );
	}

	public async Task<string> GetUserNameAsync( ClaimsPrincipal user )
	{
		using var resp = await _http.GetWithCookiesAsync( "https://synergia.librus.pl/uczen/index", _lbUser.UserCookieHeader );
		var ctnt = await resp.Content.ReadAsStringAsync();
		if ( ctnt.Contains( "Brak dostępu" ) )
		{
			throw new NotAuthorizedException( "User not logged in." );
		}

		var match = userNameRx().Match( ctnt );
		if ( !match.Success )
		{
			throw new Exception( "Cannot get username." );
		}

		return match.Groups[1].Value;
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

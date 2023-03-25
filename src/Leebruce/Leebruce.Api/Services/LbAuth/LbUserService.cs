using Leebruce.Api.Models;
using System.Net;
using System.Security.Claims;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbUserService
{
	string UserCookieHeader { get; }
}

public class LbUserService : ILbUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private string? userCookieHeader;

	public LbUserService( IHttpContextAccessor httpContextAccessor )
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public string UserCookieHeader => userCookieHeader ??= GetCookieHeader( _httpContextAccessor.HttpContext?.User ) ?? string.Empty;

	public string? GetCookieHeader( ClaimsPrincipal? user )
		=> user is null ? null : GetCookieHeader( LbAuthData.FromClaims( user.Claims ) );
	public string GetCookieHeader( LbAuthData parameters )
	{
		CookieContainer cookies = new();
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.dsidName, parameters.DziennikSid ) );
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.sdsidName, parameters.SdziennikSid ) );
		cookies.Add( LbConstants.lbCookiesDomain, new Cookie( LbConstants.oatokenName, parameters.OAuthToken ) );
		return cookies.GetCookieHeader( LbConstants.lbCookiesDomain );
	}

}

using Leebruce.Api.Extensions;
using System.Net.Http.Headers;

namespace Leebruce.Api.Services;

public interface ILbSiteClient
{
	Task<HttpResponseMessage> GetAuthorized( string url );
	Task<string> GetContentAuthorized( string url );
	Task<HttpBodyWithHeaders> GetContentAndHeadersAuthorized( string url );

	Task<HttpResponseMessage> PostAuthorized( string url, HttpContent content );
	Task<string> PostContentAuthorized( string url, HttpContent content );

	record struct HttpBodyWithHeaders( string Content, HttpResponseHeaders ResponseHeaders, HttpContentHeaders ContentHeaders );
}

public class LbSiteClient : ILbSiteClient
{
	private readonly ILbHelperService _lbHelper;
	private readonly HttpClient _http;
	private readonly ILbUserService _lbUser;

	public LbSiteClient( ILbHelperService lbHelper, HttpClient http, ILbUserService lbUser )
	{
		_lbHelper = lbHelper;
		_http = http;
		_lbUser = lbUser;
	}

	public async Task<HttpResponseMessage> GetAuthorized( string url )
		=> await _http.GetWithCookiesAsync( url, _lbUser.UserCookieHeader );

	public async Task<string> GetContentAuthorized( string url )
	{
		using var resp = await _http.GetWithCookiesAsync( url, _lbUser.UserCookieHeader );
		string document = await resp.Content.ReadAsStringAsync();

		if ( _lbHelper.IsUnauthorized( document ) )
			throw new NotAuthorizedException();

		if ( _lbHelper.IsMaintenanceBreak( document ) )
			throw new MaintenanceBreakException();

		return document;
	}

	public async Task<ILbSiteClient.HttpBodyWithHeaders> GetContentAndHeadersAuthorized( string url )
	{
		using var resp = await _http.GetWithCookiesAsync( url, _lbUser.UserCookieHeader );
		string document = await resp.Content.ReadAsStringAsync();

		if ( _lbHelper.IsUnauthorized( document ) )
			throw new NotAuthorizedException();

		if ( _lbHelper.IsMaintenanceBreak( document ) )
			throw new MaintenanceBreakException();

		return new( document, resp.Headers, resp.Content.Headers );
	}

	public async Task<HttpResponseMessage> PostAuthorized( string url, HttpContent content )
		=> await _http.PostWithCookiesAsync( url, content, _lbUser.UserCookieHeader );

	public async Task<string> PostContentAuthorized( string url, HttpContent content )
	{
		using var resp = await _http.PostWithCookiesAsync( url, content, _lbUser.UserCookieHeader );
		string document = await resp.Content.ReadAsStringAsync();

		if ( _lbHelper.IsUnauthorized( document ) )
			throw new NotAuthorizedException();

		if ( _lbHelper.IsMaintenanceBreak( document ) )
			throw new MaintenanceBreakException();

		return document;
	}

}
